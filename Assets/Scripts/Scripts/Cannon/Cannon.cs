using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Input;
using UnityEngine.Audio;

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
    [Range(0f, 1f)] public float singleShotVolume = 1f;
    [Range(0f, 1f)] public float startVolume = 1f;
    [Range(0f, 1f)] public float loopVolume = 1f;
    [Range(0f, 1f)] public float endVolume = 1f;
    [Range(0f, 1f)] public float scatterVolume = 1f;

    [Header("Power Up Settings")]
    public bool isScatterShot = false;
    public bool isFullAutoShot = false;
    private Coroutine powerUpRoutine;
    private Coroutine autoFireRoutine;
    public float fullAutoFireRate = 0.25f;

    [Header("Spawner Manager")]
    public SpawnerManager spawnerManager;

    // Handle interaction
    [HideInInspector] public bool grabHandle;
    [HideInInspector] public bool grabHandleComplete;
    [HideInInspector] public bool initialGrab;

    // Mouse control
    [Header("Rotation Settings")]
    [Tooltip("Mouse sensitivity for Editor mode")]
    public float mouseSensitivity = 50f;
    bool firePressed = false;

    [Tooltip("Rotation speed for VR mode")]
    public float vrRotationSpeed = 15f;

    [Tooltip("Multiplier for VR horizontal (left/right) rotation sensitivity.")]
    public float vrHorizontalSensitivity = 1.0f;
    [Tooltip("Multiplier for VR vertical (up/down) rotation sensitivity.")]
    public float vrVerticalSensitivity = 1.0f;

    private float pitch = 0f;
    private float yaw = 0f;


    // Recoil Settings
    [Header("Recoil Settings")]
    public int shotgunSpread = 4;
    public float recoilAngle = 5f;
    public float recoilRecovery = 10f;
    private float currentRecoil = 0f;

    // Tutorial Hands
    [Header("Tutorial Hands")]
    [SerializeField] private GameObject tutorialHands;

    // Tutorial Dialogue
    [Header("Tutorial Dialogue")]
    [SerializeField] private GameObject tutorialDialogue;

    // Game Start Dialogue
    [Header("Game Start Dialogue")]
    [SerializeField] private GameObject gameStartDialogue;

    [Header("On Release Settings")]
    [Tooltip("These objects will become active when the turret is released, but only after the first grab.")]
    public GameObject[] objectsToActivateOnRelease;

    //public AudioMixer masterMixer; 

    void Start()
    {
        initialGrab = false;
        handleHand.GetComponent<MeshRenderer>().enabled = false;
        grabHandleComplete = true;
        grabHandle = false;
        particleSystem = GetComponentInChildren<ParticleSystem>();
        audio = GetComponent<AudioSource>();

        ToggleReleaseObjects(false);
    }

    void Update()
    {
        IVRInputDevice primaryInput = VRDevice.Device != null ? VRDevice.Device.PrimaryInputDevice : null;
        IVRInputDevice secondaryInput = VRDevice.Device != null ? VRDevice.Device.SecondaryInputDevice : null;

#if UNITY_EDITOR
        // --- MODIFIED: Editor Hold-to-Grab Logic ---
        // Press E to Grab
        if (Application.isEditor && Input.GetKeyDown(KeyCode.E) && !grabHandle)
        {
            HandleGrab();
        }

        // Release E to Ungrab
        if (Application.isEditor && Input.GetKeyUp(KeyCode.E) && grabHandle)
        {
            HandleRelease();
        }
#endif

        // ---------- VR Grab Handle ----------
        if (!Application.isEditor && VRDevice.Device != null)
        {
            bool leftGrab = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
            bool rightGrab = OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);

            if (leftGrab && rightGrab)
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
            // VR Controls
            if (!Application.isEditor && VRDevice.Device != null)
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

                Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0);
                cannon.transform.localRotation = baseCannonRotation * recoilRotation;

                currentRecoil = Mathf.Lerp(currentRecoil, 0f, recoilRecovery * Time.deltaTime);
            }
            else // This block handles Editor mouse controls
            {
                // Mouse Controls
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

                yaw += mouseX;
                pitch -= mouseY;
                pitch = Mathf.Clamp(pitch, -30f, 30f);

                cBase.localRotation = Quaternion.Euler(0f, yaw, 0f);

                Quaternion baseCannonRotation = Quaternion.Euler(pitch, 0f, 0f);
                Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0);
                cannon.localRotation = baseCannonRotation * recoilRotation;

                currentRecoil = Mathf.Lerp(currentRecoil, 0f, recoilRecovery * Time.deltaTime);
            }

            // Fire
            {
                // Fire input handling: detect both "down" (single) and "hold" (continuous)
                bool holdFire = false;
                bool downFire = false;

#if UNITY_EDITOR
                if (Application.isEditor)
                {
                    holdFire = Input.GetMouseButton(0) || Input.GetMouseButton(1);
                    downFire = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
                }
#endif
                if (!Application.isEditor)
                {
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

        currentRecoil += recoilAngle;
        currentRecoil = Mathf.Clamp(currentRecoil, 0, recoilAngle * 2f);
        OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.RTouch | OVRInput.Controller.LTouch);
        StartCoroutine(StopHaptics(0.2f));
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
        localCb.rb.velocity = Vector3.zero;

        StartCoroutine(EnablePhysicsNextFixed(localCb));
    }

    private IEnumerator EnablePhysicsNextFixed(CannonBall cbLocal)
    {
        yield return new WaitForFixedUpdate();

        if (cbLocal == null || cbLocal.rb == null) yield break;

        cbLocal.rb.isKinematic = false;
        cbLocal.rb.velocity = Vector3.zero;
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