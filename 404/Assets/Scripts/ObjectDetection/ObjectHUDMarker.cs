using TMPro;
using UnityEngine;

public class ObjectHUDMarker : MonoBehaviour
{
    public TMP_Text text;
    private Transform target;

    private float timeToLive = 2.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = Camera.main.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = target.rotation; // Match camera facing
        timeToLive -= Time.deltaTime;
        if (timeToLive <= 0.0f) Destroy(this.gameObject);
    }

    public void SetText(string newText)
    {
        text.text = newText;
    }
}
