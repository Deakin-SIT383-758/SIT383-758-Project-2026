using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class MapController : NetworkBehaviour
{
    public Transform targetCube;
    public Button moveUpButton;
    public Button moveDownButton;
    public float moveAmount = 0.5f;

    public override void Spawned()
    {
        if (moveUpButton != null)
        {
            moveUpButton.onClick.AddListener(MoveCubeUp);
        }

        if (moveDownButton != null)
        {
            moveDownButton.onClick.AddListener(MoveCubeDown);
        }
    }

    public void MoveCubeUp()
    {
        RequestAndMove(moveAmount);
    }

    public void MoveCubeDown()
    {
        RequestAndMove(-moveAmount);
    }

    private void RequestAndMove(float amount)
    {
        Object.RequestStateAuthority();
        MoveMap(amount);
    }

    private void MoveMap(float amount)
    {
        if (targetCube == null)
        {
            Debug.LogError("targetCube is NULL in MoveMap");
            return;
        }
        Vector3 pos = targetCube.position;
        pos.y += amount;
        targetCube.position = pos;
    }
}