using UnityEngine;

public class LoopZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider area)
    {
        if (area.CompareTag("Player"))
        {
            Debug.Log("Player entered Loop Zone.");
            LoopManager.Instance.SetPlayerInZone(true);
        }
    }

    private void OnTriggerExit(Collider area)
    {
        if (area.CompareTag("Player"))
        {
            Debug.Log("Player exited Loop Zone.");
            LoopManager.Instance.SetPlayerInZone(false);
        }
    }
}
