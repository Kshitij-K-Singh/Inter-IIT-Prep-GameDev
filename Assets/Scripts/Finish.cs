using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Sound to play when player teleports at end of loop")]
    public AudioClip warpClip;
    [Range(0f, 1f)] public float warpVolume = 1f;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider plate)
    {
        if (plate.CompareTag("Player"))
        {
            if (audioSource == null || warpClip == null) return;
            audioSource.PlayOneShot(warpClip, warpVolume);
            SceneManager.LoadScene(2);
        }
    }
}
