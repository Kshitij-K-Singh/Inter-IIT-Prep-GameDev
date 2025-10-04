using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pressure_Plate : MonoBehaviour
{
    [SerializeField]
    public GameObject mecharea;
    [SerializeField]
    public MeshRenderer py;
    [SerializeField]
    public MeshRenderer mr;
    [SerializeField]
    public Animator anim;

    [Header("Audio Settings")]
    [Tooltip("Sound to play when player teleports at end of loop")]
    public AudioClip pressDownClip;
    public AudioClip pressUpClip;
    [Range(0f, 1f)] public float pressVolume = 1f;
    private AudioSource audioSource;

    private bool isPressed = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider plate)
    {
        if (plate.CompareTag("Player") || plate.CompareTag("Echo"))
        {
            if (isPressed) return;
            isPressed = true;

            Debug.Log("Player on plate");
            anim.SetTrigger("StateON");
            if (pressDownClip == null || audioSource == null) return;
            audioSource.PlayOneShot(pressDownClip, pressVolume);
            mr.material.EnableKeyword("_EMISSION");
            py.material.EnableKeyword("_EMISSION");
            mecharea.SetActive(true);

        }
    }
    private void OnTriggerExit(Collider plate)
    {
        if (plate.CompareTag("Player") || plate.CompareTag("Echo"))
        {
            if (!isPressed) return;
            isPressed = false;

            anim.SetTrigger("StateOFF");
            if (audioSource == null || pressUpClip == null) return;
            audioSource.PlayOneShot(pressUpClip, pressVolume);
            mr.material.DisableKeyword("_EMISSION");
            py.material.DisableKeyword("_EMISSION");
            mecharea.SetActive(false);
        }
    }

    public void ForceRelease()
    {
        if (!isPressed) return;
        isPressed = false;

        anim.SetTrigger("StateOFF");
        if (audioSource == null || pressUpClip == null) return;
        audioSource.PlayOneShot(pressUpClip, pressVolume);
        mr.material.DisableKeyword("_EMISSION");
        py.material.DisableKeyword("_EMISSION");
        mecharea.SetActive(false);
    }
}
