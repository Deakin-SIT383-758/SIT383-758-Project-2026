using UnityEngine;

public class FlightMovementTest : MonoBehaviour
{
    public float moveSpeed = 50;
    public float maxMoveDist = 1300;

    private System.Action MovementAction;
    private float startZ;
    
    private void Start()
    {
        MovementAction = MoveNorth;
    }

    public GameObject gazeIndicator;

    void Update()
    {
        gazeIndicator.SetActive(OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger));

        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
            return;

        MovementAction();
    }

    void MoveNorth()
    {
        transform.position += Vector3.forward * Time.deltaTime * moveSpeed;
        if (transform.position.z > startZ + maxMoveDist)
            MovementAction = MoveSouth;
    }

    void MoveSouth()
    {
        transform.position -= Vector3.forward * Time.deltaTime * moveSpeed;
        if (transform.position.z < startZ)
            MovementAction = MoveNorth;
    }
}
