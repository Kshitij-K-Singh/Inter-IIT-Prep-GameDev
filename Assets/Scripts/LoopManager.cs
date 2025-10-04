using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopManager : MonoBehaviour
{
    public static LoopManager Instance; // Singleton access for LoopZone
    public Pressure_Plate pr;
    public Gate_PrsPlt gpr;

    [Header("Loop Settings")]
    [Tooltip("How long each loop runs (seconds)")]
    public float loopLength = 10f;

    [Header("References")]
    public GameObject player;
    public GameObject echoPrefab;
    public PlayerRecorder playerRecorder;

    [Header("Audio Settings")]
    [Tooltip("Sound to play when player teleports at end of loop")]
    public AudioClip teleportClip;
    [Range(0f, 1f)] public float teleportVolume = 1f;
    private AudioSource audioSource;

    private Vector3 loopStartPosition;
    private Quaternion loopStartRotation;
    private bool isLooping = false;
    private Coroutine loopCoroutine;
    private List<GameObject> spawnedEchoes = new List<GameObject>();

    private bool playerInZone = false; // ✅ new flag

    void Awake()
    {
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Only allow L key if player is inside trigger zone
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (!playerInZone)
            {
                Debug.Log("Cannot start loop — player not inside loop zone!");
                return;
            }

            if (!isLooping) StartLoop();
            else StopLoopEarly();
        }
    }

    // Called by LoopZone.cs
    public void SetPlayerInZone(bool inZone)
    {
        playerInZone = inZone;
    }

    public void StartLoop()
    {
        if (isLooping)
        {
            Debug.LogWarning("Loop already running.");
            return;
        }

        if (player == null || playerRecorder == null || echoPrefab == null)
        {
            Debug.LogError("LoopManager: Missing references. Assign player, playerRecorder, and echoPrefab.");
            return;
        }

        // capture player's start transform for this loop
        loopStartPosition = player.transform.position;
        loopStartRotation = player.transform.rotation;

        playerRecorder.StartRecording();
        loopCoroutine = StartCoroutine(LoopTimerCoroutine());
        isLooping = true;

        Debug.Log("Loop started. Start pos: " + loopStartPosition);
    }

    IEnumerator LoopTimerCoroutine()
    {
        float t = 0f;
        while (t < loopLength)
        {
            t += Time.deltaTime;
            yield return null;
        }

        EndLoop();
    }

    public void StopLoopEarly()
    {
        if (!isLooping) return;
        if (loopCoroutine != null) StopCoroutine(loopCoroutine);
        EndLoop();
    }

    private void EndLoop()
    {
        playerRecorder.StopRecording();
        List<RecordedFrame> frames = playerRecorder.GetFramesCopy();
        pr.ForceRelease();
        gpr.ForceRelease();

        SpawnEchoAt(loopStartPosition, loopStartRotation, frames, loopLength);
        PlayTeleportSound();
        ResetPlayerToStart();

        isLooping = false;
        Debug.Log("Loop ended, echo spawned. Player reset to start.");
    }

    private void PlayTeleportSound()
    {
        if (teleportClip == null || audioSource == null) return;
        audioSource.PlayOneShot(teleportClip, teleportVolume);
    }

    private void SpawnEchoAt(Vector3 pos, Quaternion rot, List<RecordedFrame> frames, float totalLoopLength)
    {
        GameObject echoObj = Instantiate(echoPrefab, pos, rot);
        EchoController ec = echoObj.GetComponent<EchoController>();
        if (ec != null)
        {
            ec.Initialize(frames, totalLoopLength);
        }
        else
        {
            Debug.LogWarning("Spawned echo prefab has no EchoController component.");
        }

        spawnedEchoes.Add(echoObj);
    }

    private void ResetPlayerToStart()
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = loopStartPosition;
            rb.rotation = loopStartRotation;
        }
        else
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = loopStartPosition;
            player.transform.rotation = loopStartRotation;

            if (cc != null) cc.enabled = true;
        }
    }

    public int ActiveEchoCount() => spawnedEchoes.Count;
}
