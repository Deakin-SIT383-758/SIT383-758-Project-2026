using UnityEngine;

public class RunwayManager : MonoBehaviour
{
    [Header("Runway Prefabs")]
    public GameObject[] runwayPrefabs;

    [Header("Spawn Settings")]
    public int defaultPrefabIndex = 0;
    public Transform spawnReference;
    public float spawnDistance = 5f;
    public float spawnHeightOffset = -1f;

    private GameObject currentRunway;

    void Start()
    {
        SpawnRunway(defaultPrefabIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnRunway(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnRunway(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnRunway(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SpawnRunway(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SpawnRunway(4);
    }

    public void SpawnRunway(int prefabIndex)
    {
        if (runwayPrefabs == null || runwayPrefabs.Length == 0)
        {
            Debug.LogWarning("No runway prefabs assigned.");
            return;
        }

        if (prefabIndex < 0 || prefabIndex >= runwayPrefabs.Length)
        {
            Debug.LogWarning("Invalid runway prefab index: " + prefabIndex);
            return;
        }

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

        currentRunway = Instantiate(runwayPrefabs[prefabIndex], spawnPos, runwayPrefabs[prefabIndex].transform.rotation);

        Debug.Log("Spawned runway prefab index: " + prefabIndex);
    }
}