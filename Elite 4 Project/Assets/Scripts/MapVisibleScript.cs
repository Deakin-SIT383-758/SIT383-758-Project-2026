using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackVisibility : MonoBehaviour
{
    public GameObject modelRoot;
    [Header("Fade Settings")]
    public float fadeSpeed = 2f;
    public float fadeOutDelay = 0.2f;
    private ARTrackedImage trackedImage;
    private Renderer[] renderers;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;
    private float lostTimer = 0f;

    void Awake()
    {
        trackedImage = GetComponentInParent<ARTrackedImage>();

        if (modelRoot != null)
        {
            renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            SetAlpha(0f);
            modelRoot.SetActive(false);
        }
    }

    void Update()
    {
        if (modelRoot == null || renderers == null) return;

        if (trackedImage == null)
            trackedImage = GetComponentInParent<ARTrackedImage>();

        if (trackedImage == null) return;

        bool markerVisible = trackedImage.trackingState == TrackingState.Tracking;

        if (markerVisible)
        {
            lostTimer = 0f;
            targetAlpha = 1f;

            if (!modelRoot.activeSelf)
                modelRoot.SetActive(true);
        }
        else
        {
            lostTimer += Time.deltaTime;

            if (lostTimer >= fadeOutDelay)
                targetAlpha = 0f;
        }

        currentAlpha = Mathf.MoveTowards(
            currentAlpha,
            targetAlpha,
            fadeSpeed * Time.deltaTime
        );

        SetAlpha(currentAlpha);

        if (!markerVisible && currentAlpha <= 0.01f)
        {
            modelRoot.SetActive(false);
        }
    }

    void SetAlpha(float alpha)
    {
        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.materials)
            {
                Color color = mat.color;
                color.a = alpha;
                mat.color = color;
            }
        }
    }
}