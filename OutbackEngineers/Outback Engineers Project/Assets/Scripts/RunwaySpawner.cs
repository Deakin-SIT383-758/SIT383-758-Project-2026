using UnityEngine;

public class RunwayManager : MonoBehaviour
{
    [Header("Runway Prefab")]
    public GameObject RunwayPrefab;

    [Header("Runway Materials")]
    public Material[] runwayMaterials;

    [Header("Spawn Settings")]
    public float spawnDistance = 10f;
    public int defaultMaterialIndex = 0;

    private GameObject currentRunway;
    private Renderer runwayRenderer;

    void Start()
    {
        SpawnRunway();
        ApplyMaterial(defaultMaterialIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ApplyMaterial(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ApplyMaterial(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ApplyMaterial(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ApplyMaterial(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) ApplyMaterial(4);
    }

    public void SpawnRunway()
    {
        if (currentRunway != null)
        {
            Destroy(currentRunway);
        }

        currentRunway = Instantiate(RunwayPrefab);
        PositionRunway(currentRunway);

        runwayRenderer = currentRunway.GetComponentInChildren<Renderer>();

        if (runwayRenderer == null)
        {
            Debug.LogWarning("No Renderer found on runway prefab!");
        }
    }

    public void ApplyMaterial(int materialIndex)
    {
        if (runwayMaterials == null || runwayMaterials.Length == 0)
        {
            Debug.LogWarning("No runway materials assigned");
            return;
        }

        if (materialIndex < 0 || materialIndex >= runwayMaterials.Length)
        {
            Debug.LogWarning("Invalid material index: " + materialIndex);
            return;
        }

        if (runwayRenderer == null)
        {
            Debug.LogWarning("No runway renderer assigned.");
            return;
        }

        runwayRenderer.material = runwayMaterials[materialIndex];

        Debug.Log("Changed runway material to index: " + materialIndex);
    }

    void PositionRunway(GameObject runway)
    {
        runway.transform.position = new Vector3(0f, 0f, 80f);
        runway.transform.rotation = Quaternion.identity;
    }
}