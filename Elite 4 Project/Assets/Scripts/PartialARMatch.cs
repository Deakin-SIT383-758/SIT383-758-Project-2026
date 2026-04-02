using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
[RequireComponent(typeof(ARTrackedImageManager))]
public class LooseMarkerSpawner : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;

    public GameObject prefabToSpawn;

    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        #pragma warning disable 618
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        #pragma warning restore 618
    }

    void OnDisable()
    {
        #pragma warning disable 618
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        #pragma warning restore 618
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
            SpawnOrEnablePrefab(trackedImage);

        foreach (var trackedImage in eventArgs.updated)
            SpawnOrEnablePrefab(trackedImage);

        foreach (var trackedImage in eventArgs.removed)
        {
            if (spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
                spawnedPrefabs[trackedImage.referenceImage.name].SetActive(false);
        }
    }

    private void SpawnOrEnablePrefab(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.None)
        {
            if (spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
                spawnedPrefabs[trackedImage.referenceImage.name].SetActive(false);
            return;
        }

        if (!spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
        {
            GameObject newPrefab = Instantiate(prefabToSpawn, trackedImage.transform);
            newPrefab.transform.localPosition = Vector3.zero;
            newPrefab.transform.localRotation = Quaternion.identity;
            newPrefab.transform.localScale = Vector3.one;
            spawnedPrefabs[trackedImage.referenceImage.name] = newPrefab;
        }
        else
        {
            GameObject existingPrefab = spawnedPrefabs[trackedImage.referenceImage.name];
            existingPrefab.SetActive(true);
            existingPrefab.transform.position = trackedImage.transform.position;
            existingPrefab.transform.rotation = trackedImage.transform.rotation;
        }
    }
}