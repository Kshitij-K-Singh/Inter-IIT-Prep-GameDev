using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RecordedFrame
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;

    // Animator states
    public bool walk;
    public bool run;
    public bool jumpTriggered;

    // Interaction
    public bool interacted;
    public int interactableID; // unique ID for object (use instanceID)

    public RecordedFrame(float t, Vector3 p, Quaternion r, bool w, bool r2, bool j, bool i, int id)
    {
        time = t;
        position = p;
        rotation = r;
        walk = w;
        run = r2;
        jumpTriggered = j;
        interacted = i;
        interactableID = id;
    }
}

public class PlayerRecorder : MonoBehaviour
{
    private List<RecordedFrame> frames = new List<RecordedFrame>();
    private bool recording = false;
    private float recordStartTime = 0f;
    private Animator anim;
    private player_movement movement;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>();  // ✅ fallback if Animator is on child

        movement = GetComponent<player_movement>();
    }


    void Update()
    {
        if (!recording) return;

        float t = Time.time - recordStartTime;

        // Get movement state directly from input
        bool isMoving = Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f;
        bool run = isMoving && Input.GetKey(KeyCode.LeftShift);
        bool walk = isMoving && !run;

        bool jump = Input.GetButtonDown("Jump"); // ✅ true only on the frame you pressed Space

        bool interacted = Input.GetKeyDown(KeyCode.E);

        int targetID = -1;
        if (interacted)
        {
            targetID = movement.TryGetInteractableID();
        }

        frames.Add(new RecordedFrame(
            t, transform.position, transform.rotation,
            walk, run, jump, interacted, targetID
        ));
    }

    // Called by LoopManager
    public void StartRecording()
    {
        frames.Clear();
        recording = true;
        recordStartTime = Time.time;
    }

    // Called by LoopManager
    public void StopRecording()
    {
        recording = false;
    }

    public List<RecordedFrame> GetFramesCopy()
    {
        return new List<RecordedFrame>(frames);
    }
}
