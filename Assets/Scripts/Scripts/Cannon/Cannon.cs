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

    //public AudioMixer masterMixer; 

    void Start()
    {
        initialGrab = false;
        handleHand.GetComponent<MeshRenderer>().enabled = false;
        grabHandleComplete = true;
        grabHandle = false;
        particleSystem = GetComponentInChildren<ParticleSystem>();
        audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        IVRInputDevice primaryInput = VRDevice.Device != null ? VRDevice.Device.PrimaryInputDevice : null;
        IVRInputDevice secondaryInput = VRDevice.Device != null ? VRDevice.Device.SecondaryInputDevice : null;

        // ---------- PC Editor Grab ----------
#if UNITY_EDITOR
        // In the editor, press 'E' to simulate grabbing the handles and start the game.
        if (Application.isEditor && Input.GetKeyDown(KeyCode.E) && !grabHandle)
        {
            grabHandle = true;
            grabHandleComplete = true;
            initialGrab = true;
            handleHand.GetComponent<MeshRenderer>().enabled = true;
            hand.GetComponent<MeshRenderer>().enabled = false;
            if (secondaryHand != null) secondaryHand.SetActive(false);

            // Tell SpawnerManager to begin waves
            if (spawnerManager != null)
                spawnerManager.BeginSpawning();

            // Turn off Tutorial Hands when turret grabbed
            if (tutorialHands != null)
                tutorialHands.SetActive(false);

            // Turn off Tutorial Dialogue when turret grabbed
            if (tutorialDialogue != null)
                tutorialDialogue.SetActive(false);

            // Turn on Game Start Dialogue when turret grabbed
            if (gameStartDialogue != null)
                gameStartDialogue.SetActive(true);
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
                    grabHandle = true;
                    grabHandleComplete = true;
                    initialGrab = true;
                    handleHand.GetComponent<MeshRenderer>().enabled = true;
                    hand.GetComponent<MeshRenderer>().enabled = false;
                    if (secondaryHand != null) secondaryHand.SetActive(false);

                    // Turn off Tutorial Hands when turret grabbed
                    if (tutorialHands != null)
                        tutorialHands.SetActive(false);

                    // Turn off Tutorial Dialogue when turret grabbed
                    if (tutorialDialogue != null)
                        tutorialDialogue.SetActive(false);

                    // Turn on Game Start Dialogue when turret grabbed
                    if (gameStartDialogue != null)
                        gameStartDialogue.SetActive(true);

                    // Trigger waves when cannon is first grabbed
                    if (spawnerManager != null)
                        spawnerManager.BeginSpawning();
                }
            }
            else
            {
                if (grabHandle)
                {
                    grabHandle = false;
                    grabHandleComplete = false;
                    handleHand.GetComponent<MeshRenderer>().enabled = false;
                    hand.GetComponent<MeshRenderer>().enabled = true;
                    hand.transform.position = primaryHandAnchor.position;
                    hand.transform.rotation = primaryHandAnchor.rotation;
                    initialGrab = false;
                    if (secondaryHand != null) secondaryHand.SetActive(true);

                    // STOP any firing state when handle is released (prevent continued full-auto)
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
                        // play release tail only if we were in full-auto
                        if (isFullAutoShot && endClip != null)
                            audio.PlayOneShot(endClip, endVolume);
                    }
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

                float handX = Mathf.Clamp(rotation.x, -0.4f, 0.2f);
                float handY = Mathf.Clamp(rotation.y, -0.4f, 0.4f);

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
                    // MODIFICATION: Check for left OR right mouse button to mirror VR 'either hand' functionality.
                    // GetMouseButton(0) is left click, GetMouseButton(1) is right click.
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

                // Use holdFire for full-auto, downFire for single-shot/scatter
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
                        // Start audio chain (A -> B)
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
                    // play shotgun-style audio for scatter
                    FireCannon();
                    audio.PlayOneShot(scatterClip, scatterVolume);
                }
                else // player released trigger/mouse
                {
                    if (autoFireRoutine != null)
                    {
                        StopCoroutine(autoFireRoutine);
                        autoFireRoutine = null;

                        // Stop continuous loop and play end sound (C)
                        audio.loop = false;
                        audio.Stop();
                        // only play end sound for full-auto mode
                        if (isFullAutoShot && endClip != null)
                            audio.PlayOneShot(endClip, endVolume);
                    }
                }
            }
        }
    }

    private void FireCannon()
    {
        if (isScatterShot)
        {
            for (int i = 0; i < shotgunSpread; i++)
            {
                // Exact uniform sampling over a cone (no axis bias):
                // pick cos(theta) uniformly between cos(maxAngle) and 1.
                float maxAngle = 1f; // degrees
                float maxAngleRad = maxAngle * Mathf.Deg2Rad;

                // Sample uniformly on the spherical cap/cone
                float u = Random.value; // [0,1)
                float cosTheta = Mathf.Lerp(Mathf.Cos(maxAngleRad), 1f, u);
                float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
                float phi = Random.Range(0f, Mathf.PI * 2f);

                // Direction in barrel's local space (z-forward)
                Vector3 localDir = new Vector3(sinTheta * Mathf.Cos(phi), sinTheta * Mathf.Sin(phi), cosTheta);

                // Convert to world space and get rotation
                Vector3 worldDir = barrelEnd.transform.TransformDirection(localDir).normalized;
                Quaternion spreadRotation = Quaternion.LookRotation(worldDir, barrelEnd.transform.up);

                SpawnCannonball(barrelEnd.transform.position, spreadRotation);
            }
        }
        else
        {
            SpawnCannonball(barrelEnd.transform.position, barrelEnd.transform.rotation);

            // only play single-shot sound if not in full-auto (full-auto is handled elsewhere)
            if (!isFullAutoShot && singleShotClip != null)
                audio.PlayOneShot(singleShotClip, singleShotVolume);
        }


        // 🎇 Randomize muzzle flash Z rotation
        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startRotation = Random.Range(0f, Mathf.PI * 2f); // radians
            particleSystem.Play();
        }


        // Recoil
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
        // Use a local reference to avoid cross-iteration state when spawning multiple pooled objects
        CannonBall localCb = returnedGameObject.GetComponent<CannonBall>();
        localCb.firedFrom = this;

        // Small forward offset to reduce immediate collider overlap when multiple shots spawn simultaneously
        float spawnOffset = 0.2f;
        Vector3 forward = rot * Vector3.forward;
        localCb.rb.transform.position = pos + forward * spawnOffset;
        localCb.rb.transform.rotation = rot;

        returnedGameObject.SetActive(true);

        // Make kinematic for a single physics step, then enable and apply impulse to avoid overlap resolution pushing them aside
        localCb.rb.isKinematic = true;
        // Clear any previous velocity
        localCb.rb.velocity = Vector3.zero;

        // Start coroutine to enable physics and apply force on next FixedUpdate
        StartCoroutine(EnablePhysicsNextFixed(localCb));
    }

    private IEnumerator EnablePhysicsNextFixed(CannonBall cbLocal)
    {
        // Wait for the next physics step so the spawned object isn't immediately resolved against nearby colliders
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