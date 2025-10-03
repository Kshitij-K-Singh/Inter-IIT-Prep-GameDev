using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopManager : MonoBehaviour
{
    [Header("Loop Settings")]
    [Tooltip("How long each loop runs (seconds)")]
    public float loopLength = 10f;

    [Header("References")]
    public GameObject player;                 // assign your player GameObject
    public GameObject echoPrefab;             // assign a prefab with EchoController
    public PlayerRecorder playerRecorder;     // assign the recorder on the player

    // runtime
    private Vector3 loopStartPosition;
    private Quaternion loopStartRotation;
    private bool isLooping = false;
    private Coroutine loopCoroutine;
    private List<GameObject> spawnedEchoes = new List<GameObject>();

    void Update()
    {
        // debug: press L to toggle loop (editor/testing)
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (!isLooping) StartLoop();
            else StopLoopEarly();
        }
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
            Debug.LogError("LoopManager: Missing references. Assign player, playerRecorder and echoPrefab.");
            return;
        }

        // capture player's start transform for this loop
        loopStartPosition = player.transform.position;
        loopStartRotation = player.transform.rotation;

        // start recording
        playerRecorder.StartRecording();

        // begin coroutine that ends after loopLength
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
            // optionally update UI here with (loopLength - t)
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
        // stop recording and grab recorded frames
        playerRecorder.StopRecording();
        List<RecordedFrame> frames = playerRecorder.GetFramesCopy();

        // spawn the echo at the captured start transform and hand it recorded frames
        SpawnEchoAt(loopStartPosition, loopStartRotation, frames, loopLength);

        // reset player to start transform (and zero velocities)
        ResetPlayerToStart();

        isLooping = false;
        Debug.Log("Loop ended, echo spawned. Player reset to start.");
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
        // If player has a Rigidbody, stop velocities first
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
            // disable character controller briefly if present to avoid issues setting transform
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = loopStartPosition;
            player.transform.rotation = loopStartRotation;

            if (cc != null) cc.enabled = true;
        }

        // If player has a player controller script, you may need to reset its internal state (velocity, input buffers)
        // Example: PlayerController pc = player.GetComponent<PlayerController>(); if (pc != null) pc.ResetState();
    }

    // Optional: get the number of active echoes
    public int ActiveEchoCount() => spawnedEchoes.Count;
}
