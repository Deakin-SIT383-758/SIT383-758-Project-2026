using UnityEngine;

public class CloudDrift : MonoBehaviour
{
    public Vector3 moveDirection = new Vector3(0.02f, 0f, 0f);
    public float maxOffset = 0.1f;

    private Vector3 startPosition;

    void OnEnable()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        transform.localPosition += moveDirection * Time.deltaTime;

        if (Vector3.Distance(transform.localPosition, startPosition) > maxOffset)
        {
            transform.localPosition = startPosition;
        }
    }
}