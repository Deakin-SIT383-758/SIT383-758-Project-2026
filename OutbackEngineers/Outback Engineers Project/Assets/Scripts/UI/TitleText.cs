using UnityEngine;

public class TitleText : MonoBehaviour // Dynamic Title added above dropdown menu in main scene.
{
    void LateUpdate()
    {
        if (Camera.main == null) return;

        Transform cam = Camera.main.transform;
        transform.LookAt(cam);

        // Rotate 180° to face the camera, then tilt slightly downward (e.g., 15 degrees)
        transform.Rotate(-15, 180, 0); // 15 degrees downward tilt
    }
}
