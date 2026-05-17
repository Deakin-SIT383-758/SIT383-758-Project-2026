using UnityEngine;

public class RunwaySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int defaultPrefabIndex = 0;
    public Transform spawnReference;
    public float spawnDistance = 5f;
    public float spawnHeightOffset = -1f;

    private GameObject currentRunway;

    public GameObject GetCurrentRunway()
    {
        return currentRunway;
    }

    public void LoadRunway(GameObject runway)
    {
        if (currentRunway != null)
        {
            Destroy(currentRunway);
        }

        Transform reference = spawnReference;

        if (reference == null && Camera.main != null)
        {
            reference = Camera.main.transform;
        }

        if (reference == null)
        {
            Debug.LogWarning("No spawn reference or main camera found.");
            return;
        }

        Vector3 forward = reference.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 spawnPos = reference.position + forward * spawnDistance;
        spawnPos.y += spawnHeightOffset;

        currentRunway = Instantiate(runway, spawnPos, runway.transform.rotation);
    }
}