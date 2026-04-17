using UnityEngine;

public class ClearZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ClearZone Triggered by: " + other.name);

        if (!other.CompareTag("Player"))
            return;

        if (StageFlowManager.Instance != null)
        {
            StageFlowManager.Instance.CompleteStage();
        }
    }
}