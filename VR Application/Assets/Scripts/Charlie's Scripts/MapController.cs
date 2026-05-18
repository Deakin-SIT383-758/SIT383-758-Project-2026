using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    [Header("References")]
    public Transform targetCube;      // Drag your Cube here
    public Button moveUpButton;
    public Button moveDownButton;     // Optional second button

    [Header("Settings")]
    public float moveAmount = 0.5f;

    void Start()
    {
        if (moveUpButton != null)
            moveUpButton.onClick.AddListener(MoveCubeUp);

        if (moveDownButton != null)
            moveDownButton.onClick.AddListener(MoveCubeDown);
    }

    public void MoveCubeUp()
    {
        if (targetCube != null)
        {
            Vector3 pos = targetCube.position;
            pos.y += moveAmount;
            targetCube.position = pos;
        }
    }

    public void MoveCubeDown()
    {
        if (targetCube != null)
        {
            Vector3 pos = targetCube.position;
            pos.y -= moveAmount;
            targetCube.position = pos;
        }
    }
}