using UnityEngine;

public class WebcamScript : MonoBehaviour
{
    public int cameraIndex = 0; // camera of index to use
    private WebCamTexture wcTex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (WebCamDevice cam in WebCamTexture.devices)
        {
            Debug.Log("Camera found: " + cam.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (wcTex == null)
        {
            wcTex = new WebCamTexture(WebCamTexture.devices[cameraIndex].name);
            GetComponent<Renderer>().material.mainTexture = wcTex;
        }

        if (!wcTex.isPlaying) wcTex.Play();
    }
}
