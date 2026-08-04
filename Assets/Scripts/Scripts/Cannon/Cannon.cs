using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Liminal.SDK.V2;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Input;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Features.Interactions;
using ISCommonUsages = UnityEngine.InputSystem.CommonUsages;

public class Cannon : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject cannonBall;
    [SerializeField] private GameObject barrelEnd;
    public GameObject hand;
    public GameObject handleHand;
    public GameObject primaryHand;
    public GameObject secondaryHand;
    public GameObject cannonPos;
    private CannonBall cb;

    [Header("Transforms")]
    [SerializeField] private Transform primaryHandAnchor;
    [SerializeField] private Transform cannon;
    [SerializeField] private Transform cBase;

    [Header("Effects")]
    private new ParticleSystem particleSystem;
    private new AudioSource audio;

    [Header("Audio Clips")]
    public AudioClip singleShotClip;   // single/semi shot
    public AudioClip startClip;        // start burst for full auto
    public AudioClip loopClip;         // continuous loop
    public AudioClip endClip;          // release tail
    public AudioClip scatterClip;   // short burst sound for shotgun

    [Header("Audio Volumes")]
    [Tooltip("Volume for the standard single cannon shot.")]
    [Range(0f, 1f)] public float singleShotVolume = 1f;
    [Tooltip("Volume for the initial burst sound of the full-auto mode.")]
    [Range(0f, 1f)] public float startVolume = 1f;
    [Tooltip("Volume for the looping firing sound during full-auto mode.")]
    [Range(0f, 1f)] public float loopVolume = 1f;
    [Tooltip("Volume for the tail-off sound when releasing the trigger in full-auto mode.")]
    [Range(0f, 1f)] public float endVolume = 1f;
    [Tooltip("Volume for the scatter shot sound effect.")]
    [Range(0f, 1f)] public float scatterVolume = 1f;

    [Header("Power Up Settings")]
    public bool isScatterShot = false;
    public bool isFullAutoShot = false;
    private Coroutine powerUpRoutine;
    private Coroutine autoFireRoutine;
    public float fullAutoFireRate = 0.25f;

    [Header("Hand Tracking")]
    [Tooltip("While hand tracking is active, the cannon auto-fires at this interval whenever it's grabbed (no trigger input needed).")]
    public float handAutoFireRate = 0.5f;
    private float handAutoFireTimer;

    [Header("Spawner Manager")]
    public SpawnerManager spawnerManager;

    // Handle interaction
    [HideInInspector] public bool grabHandle;
    [HideInInspector] public bool grabHandleComplete;
    [HideInInspector] public bool initialGrab;

    [Header("Rotation Settings")]
    bool firePressed = false;

    [Tooltip("Rotation speed for VR mode")]
    public float vrRotationSpeed = 15f;

    [Tooltip("Multiplier for VR horizontal (left/right) rotation sensitivity.")]
    public float vrHorizontalSensitivity = 1.0f;
    [Tooltip("Multiplier for VR vertical (up/down) rotation sensitivity.")]
    public float vrVerticalSensitivity = 1.0f;


    [Header("Rotational Recoil")]
    public int shotgunSpread = 4;
    [Tooltip("The amount the cannon rotates upwards when firing. Set to 0 to disable rotational recoil.")]
    public float recoilAngle = 5f;
    public float recoilRecovery = 10f;
    private float currentRecoil = 0f;

    // --- NEW: Kick-Back Recoil ---
    [Header("Kick-Back Recoil")]
    [Tooltip("The distance the cannon moves backward when firing. Set to 0 to disable kick-back recoil.")]
    public float kickbackIntensity = 0.1f;
    [Tooltip("How quickly the cannon returns to its original position after kicking back.")]
    public float kickbackRecoverySpeed = 15f;
    private float currentKickback = 0f;
    private Vector3 originalCannonLocalPosition;
    // --- END NEW ---

    [Header("Haptic Settings")]
    [Tooltip("The strength of the haptic burst when first grabbing the cannon (0 to 1).")]
    [Range(0f, 1f)] public float initialGrabHapticStrength = 0.8f;
    [Tooltip("The duration of the haptic burst when first grabbing the cannon (in seconds).")]
    public float initialGrabHapticDuration = 0.5f;
    [Tooltip("The strength of the haptic feedback when firing a shot (0 to 1).")]
    [Range(0f, 1f)] public float fireHapticStrength = 1f;
    [Tooltip("The duration of the haptic feedback when firing a shot (in seconds).")]
    public float fireHapticDuration = 0.2f;

    [Header("Tutorial Hands")]
    [SerializeField] private GameObject tutorialHands;

    [Header("Tutorial Dialogue")]
    [SerializeField] private GameObject tutorialDialogue;

    [Header("Game Start Dialogue")]
    [SerializeField] private GameObject gameStartDialogue;

    [Header("On Release Settings")]
    [Tooltip("These objects will become active when the turret is released, but only after the first grab.")]
    public GameObject[] objectsToActivateOnRelease;

    public static GameObject Player;

    void Awake()
    {
        Player = gameObject;
    }

    void Start()
    {
        initialGrab = false;
        handleHand.GetComponent<MeshRenderer>().enabled = false;
        grabHandleComplete = true;
        grabHandle = false;
        particleSystem = GetComponentInChildren<ParticleSystem>();
        audio = GetComponent<AudioSource>();

        ToggleReleaseObjects(false);

        // --- NEW: Store the cannon's original local position ---
        if (cannon != null)
        {
            originalCannonLocalPosition = cannon.localPosition;
        }
        // --- END NEW ---
    }

    void Update()
    {
        IVRInputDevice primaryInput = VRDevice.Device != null ? VRDevice.Device.PrimaryInputDevice : null;
        IVRInputDevice secondaryInput = VRDevice.Device != null ? VRDevice.Device.SecondaryInputDevice : null;

        // Grab Handle Logic — grip (VRButton.Three) on controllers, pinch-hold on tracked hands
        if (VRDevice.Device != null)
        {
            bool primaryGrab = primaryInput != null &&
                (primaryInput.GetButton(VRButton.Three) || HandPinchHold.IsHeld(primaryInput.Hand));
            bool secondaryGrab = secondaryInput != null &&
                (secondaryInput.GetButton(VRButton.Three) || HandPinchHold.IsHeld(secondaryInput.Hand));

            if (primaryGrab && secondaryGrab)
            {
                if (!grabHandle)
                {
                    HandleGrab();
                }
            }
            else
            {
                if (grabHandle)
                {
                    HandleRelease();
                }
            }
        }

        if (grabHandle)
        {
            Quaternion rotation = Quaternion.LookRotation(
                cannonPos.transform.position - (primaryHand.transform.position - cannonPos.transform.position) * 1000
            );

            float handX = Mathf.Clamp(rotation.x, -0.4f, 0.2f) * vrVerticalSensitivity;
            float handY = Mathf.Clamp(rotation.y, -0.4f, 0.4f) * vrHorizontalSensitivity;

            float rotationSpeed = vrRotationSpeed;
            cBase.transform.rotation = Quaternion.Lerp(
                cBase.transform.rotation,
                new Quaternion(0, handY, 0, cBase.transform.rotation.w),
                rotationSpeed * Time.deltaTime
            );

            Quaternion baseCannonRotation = Quaternion.Lerp(
                cannon.transform.localRotation,
                new Quaternion(handX, 0, 0, cannon.transform.localRotation.w),
                rotationSpeed * Time.deltaTime
            );

            // --- MODIFIED: Apply all recoil types here ---
            // Rotational Recoil
            Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0);
            cannon.localRotation = baseCannonRotation * recoilRotation;

            // Positional Kick-back Recoil
            cannon.localPosition = originalCannonLocalPosition + Vector3.back * currentKickback;

            // Recover from both recoil types over time
            currentRecoil = Mathf.Lerp(currentRecoil, 0f, recoilRecovery * Time.deltaTime);
            currentKickback = Mathf.Lerp(currentKickback, 0f, kickbackRecoverySpeed * Time.deltaTime);
            // --- END MODIFICATION ---

            // Firing Logic
            {
                bool holdFire = false;
                bool downFire = false;

                if (IsHandTrackingActive())
                {
                    // Hands can't pull a trigger while pinch-grabbing, so the cannon
                    // fires itself while grabbed: constant hold plus a pulsed "press"
                    // so the single-shot and scatter paths fire at handAutoFireRate.
                    holdFire = true;
                    handAutoFireTimer += Time.deltaTime;
                    if (handAutoFireTimer >= handAutoFireRate)
                    {
                        handAutoFireTimer = 0f;
                        downFire = true;
                    }
                }
                else
                {
                    handAutoFireTimer = 0f;

                    if (primaryInput != null)
                    {
                        holdFire |= primaryInput.GetButton(VRButton.Trigger);
                        downFire |= primaryInput.GetButtonDown(VRButton.Trigger);
                    }
                    if (secondaryInput != null)
                    {
                        holdFire |= secondaryInput.GetButton(VRButton.Trigger);
                        downFire |= secondaryInput.GetButtonDown(VRButton.Trigger);
                    }
                }

                if (isFullAutoShot)
                    firePressed = holdFire;
                else
                    firePressed = downFire;


                if ((!isFullAutoShot && !isScatterShot) && firePressed)
                {
                    FireCannon();
                }
                else if (isFullAutoShot && firePressed)
                {
                    if (autoFireRoutine == null)
                    {
                        if (startClip != null) audio.PlayOneShot(startClip, startVolume);
                        if (loopClip != null)
                        {
                            audio.loop = true;
                            audio.clip = loopClip;
                            audio.PlayDelayed(startClip != null ? startClip.length : 0f);
                        }

                        autoFireRoutine = StartCoroutine(FullAutoFireCannon());
                    }
                }
                else if ((scatterClip != null) && (isScatterShot && firePressed))
                {
                    FireCannon();
                    audio.PlayOneShot(scatterClip, scatterVolume);
                }
                else
                {
                    if (autoFireRoutine != null)
                    {
                        StopCoroutine(autoFireRoutine);
                        autoFireRoutine = null;

                        audio.loop = false;
                        audio.Stop();

                        if (isFullAutoShot && endClip != null)
                            audio.PlayOneShot(endClip, endVolume);
                    }
                }
            }
        }
    }

    /// <summary>
    /// True when OpenXR hand-interaction devices are present and a hand is actually
    /// tracked — i.e. the player is using hand tracking rather than controllers.
    /// </summary>
    private static bool IsHandTrackingActive()
    {
        var left = InputSystem.GetDevice<HandInteractionProfile.HandInteraction>(ISCommonUsages.LeftHand);
        if (left != null && left.added && left.isTracked.isPressed)
            return true;

        var right = InputSystem.GetDevice<HandInteractionProfile.HandInteraction>(ISCommonUsages.RightHand);
        return right != null && right.added && right.isTracked.isPressed;
    }

    private void HandleGrab()
    {
        grabHandle = true;
        grabHandleComplete = true;

        handleHand.GetComponent<MeshRenderer>().enabled = true;
        hand.GetComponent<MeshRenderer>().enabled = false;
        if (secondaryHand != null) secondaryHand.SetActive(false);

        if (initialGrab)
        {
            ToggleReleaseObjects(false);
        }

        if (!initialGrab)
        {
            initialGrab = true;

            TriggerHaptics(initialGrabHapticStrength, initialGrabHapticDuration);

            if (spawnerManager != null)
                spawnerManager.BeginSpawning();

            if (tutorialHands != null)
                tutorialHands.SetActive(false);

            if (tutorialDialogue != null)
                tutorialDialogue.SetActive(false);

            if (gameStartDialogue != null)
                gameStartDialogue.SetActive(true);
        }
    }

    private void HandleRelease()
    {
        grabHandle = false;
        grabHandleComplete = false;
        handleHand.GetComponent<MeshRenderer>().enabled = false;
        hand.GetComponent<MeshRenderer>().enabled = true;
        hand.transform.position = primaryHandAnchor.position;
        hand.transform.rotation = primaryHandAnchor.rotation;
        if (secondaryHand != null) secondaryHand.SetActive(true);

        ToggleReleaseObjects(true);

        firePressed = false;
        handAutoFireTimer = 0f;

        if (autoFireRoutine != null)
        {
            StopCoroutine(autoFireRoutine);
            autoFireRoutine = null;
        }

        if (audio != null)
        {
            if (audio.loop)
            {
                audio.loop = false;
                audio.Stop();
            }
            if (isFullAutoShot && endClip != null)
                audio.PlayOneShot(endClip, endVolume);
        }
    }

    private void ToggleReleaseObjects(bool setActive)
    {
        if (objectsToActivateOnRelease == null || objectsToActivateOnRelease.Length == 0) return;

        foreach (var obj in objectsToActivateOnRelease)
        {
            if (obj != null)
            {
                obj.SetActive(setActive);
            }
        }
    }

    private void FireCannon()
    {
        if (isScatterShot)
        {
            for (int i = 0; i < shotgunSpread; i++)
            {
                float maxAngle = 1f;
                float maxAngleRad = maxAngle * Mathf.Deg2Rad;

                float u = Random.value;
                float cosTheta = Mathf.Lerp(Mathf.Cos(maxAngleRad), 1f, u);
                float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
                float phi = Random.Range(0f, Mathf.PI * 2f);

                Vector3 localDir = new Vector3(sinTheta * Mathf.Cos(phi), sinTheta * Mathf.Sin(phi), cosTheta);

                Vector3 worldDir = barrelEnd.transform.TransformDirection(localDir).normalized;
                Quaternion spreadRotation = Quaternion.LookRotation(worldDir, barrelEnd.transform.up);

                SpawnCannonball(barrelEnd.transform.position, spreadRotation);
            }
        }
        else
        {
            SpawnCannonball(barrelEnd.transform.position, barrelEnd.transform.rotation);

            if (!isFullAutoShot && singleShotClip != null)
                audio.PlayOneShot(singleShotClip, singleShotVolume);
        }

        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startRotation = Random.Range(0f, Mathf.PI * 2f);
            particleSystem.Play();
        }

        // --- NEW: Add both rotational and kick-back recoil on fire ---
        currentRecoil += recoilAngle;
        currentRecoil = Mathf.Clamp(currentRecoil, 0, recoilAngle * 2f);

        currentKickback += kickbackIntensity;
        currentKickback = Mathf.Clamp(currentKickback, 0, kickbackIntensity * 2f);
        // --- END NEW ---

        TriggerHaptics(fireHapticStrength, fireHapticDuration);
    }

    private void TriggerHaptics(float strength, float duration)
    {
        OVRInput.SetControllerVibration(strength, strength, OVRInput.Controller.RTouch | OVRInput.Controller.LTouch);
        StartCoroutine(StopHaptics(duration));
    }

    private IEnumerator StopHaptics(float duration)
    {
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch | OVRInput.Controller.LTouch);
    }

    private void SpawnCannonball(Vector3 pos, Quaternion rot)
    {
        GameObject returnedGameObject = PoolManager.current.GetPooledObject(cannonBall.name);
        if (returnedGameObject == null) return;

        CannonBall localCb = returnedGameObject.GetComponent<CannonBall>();
        localCb.firedFrom = this;

        float spawnOffset = 0.2f;
        Vector3 forward = rot * Vector3.forward;
        localCb.rb.transform.position = pos + forward * spawnOffset;
        localCb.rb.transform.rotation = rot;

        returnedGameObject.SetActive(true);

        localCb.rb.isKinematic = true;
        localCb.rb.linearVelocity = Vector3.zero;

        StartCoroutine(EnablePhysicsNextFixed(localCb));
    }

    private IEnumerator EnablePhysicsNextFixed(CannonBall cbLocal)
    {
        yield return new WaitForFixedUpdate();

        if (cbLocal == null || cbLocal.rb == null) yield break;

        cbLocal.rb.isKinematic = false;
        cbLocal.rb.linearVelocity = Vector3.zero;
        cbLocal.rb.AddForce(cbLocal.rb.transform.forward * cbLocal.force, ForceMode.Impulse);
    }

    public void ActivateScatterShot()
    {
        isScatterShot = true;
    }

    public void DeactivateScatterShot()
    {
        isScatterShot = false;
    }

    public void ActivateFullAutoShot()
    {
        isFullAutoShot = true;
    }

    public void DeactivateFullAutoShot()
    {
        isFullAutoShot = false;
        if (autoFireRoutine != null)
        {
            StopCoroutine(autoFireRoutine);
            autoFireRoutine = null;
        }

        if (audio != null && audio.loop)
        {
            audio.loop = false;
            audio.Stop();


            if (endClip != null)
                audio.PlayOneShot(endClip, endVolume);
        }
    }

    private IEnumerator FullAutoFireCannon()
    {
        while (isFullAutoShot)
        {
            FireCannon();
            yield return new WaitForSeconds(fullAutoFireRate);
        }
        autoFireRoutine = null;
    }
}