using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoController : MonoBehaviour
{
    private List<RecordedFrame> frames;
    private float loopLength;
    private Coroutine playCoroutine;
    private Animator anim;

    // Lookup of interactables in the scene
    private Dictionary<int, IInteractable> interactableLookup = new Dictionary<int, IInteractable>();

    public void Initialize(List<RecordedFrame> recordedFrames, float totalLoopLength)
    {
        if (recordedFrames == null || recordedFrames.Count == 0)
        {
            Debug.LogWarning("EchoController: no frames to play.");
            return;
        }

        frames = new List<RecordedFrame>(recordedFrames);
        loopLength = totalLoopLength;

        anim = GetComponent<Animator>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>(); // ✅ works if Animator is on child (Mixamo case)


        // Make echo semi-transparent
        MakeSemiTransparent();

        // Assign to Echo layer to avoid collisions with player
        gameObject.layer = LayerMask.NameToLayer("Echo");

        // Build interactable lookup correctly
        interactableLookup.Clear();
        MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var mb in allBehaviours)
        {
            if (mb is IInteractable interactable)
            {
                interactableLookup[mb.gameObject.GetInstanceID()] = interactable;
            }
        }

        playCoroutine = StartCoroutine(PlayLoop());
    }

    // -------------------
    // Make the echo semi-transparent
    private void MakeSemiTransparent()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            Material mat = new Material(rend.material); // new instance
            Color c = mat.color;
            c.a = 0.5f; // 50% transparent
            mat.color = c;

            // Enable transparency for standard shader
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            rend.material = mat;
        }
    }

    IEnumerator PlayLoop()
    {
        while (true)
        {
            float startTime = Time.time;

            for (int i = 0; i < frames.Count; i++)
            {
                RecordedFrame f = frames[i];

                // --- Position + Rotation ---
                transform.position = f.position;
                transform.rotation = f.rotation;

                // --- Animator States ---
                if (anim != null)
                {
                    anim.SetBool("Walk", f.walk);
                    anim.SetBool("Run", f.run);
                    if (f.jumpTriggered)
                        anim.SetTrigger("Jump");
                }

                // --- Interaction Replay ---
                if (f.interacted && f.interactableID != -1)
                {
                    if (interactableLookup.TryGetValue(f.interactableID, out IInteractable interactable))
                    {
                        interactable.Interact();
                    }
                }

                // --- Timing ---
                float nextTime = (i + 1 < frames.Count) ? frames[i + 1].time : loopLength;
                float wait = nextTime - f.time;
                if (wait > 0f) yield return new WaitForSeconds(wait);
            }

            // keep looping consistently
            float elapsed = Time.time - startTime;
            float remaining = loopLength - elapsed;
            if (remaining > 0) yield return new WaitForSeconds(remaining);
        }
    }
}
