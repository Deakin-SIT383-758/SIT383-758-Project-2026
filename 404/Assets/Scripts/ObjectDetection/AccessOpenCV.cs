using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AccessOpenCV : MonoBehaviour
{
    public Material cameraMaterial;

    public GameObject markerTemplate;
    public GameObject markerParent;

    private bool modelReady = false;

    private float delayTime = 0.0f;

    public TMP_Text text;

    private string[] CLASSES = { "background", "aeroplane", "bicycle", "bird", "boat", "bottle", "bus", "car", "cat", "chair", "cow", "diningtable", "dog", "horse", "motorbike", "person", "pottedplant", "sheep", "sofa", "train", "tvmonitor" };

    [DllImport("VisualRecognition")]
    private static extern void prepareModel(string dirname);

    [DllImport("VisualRecognition")]
    private static extern int doRecognise(byte[] imageData, int width, int height);

    [DllImport("VisualRecognition")]
    private static extern void retrieveMatch(int i, ref int category, ref float confidence, ref float sx, ref float sy, ref float ex, ref float ey);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(prepareModel());
        cameraMaterial = markerParent.GetComponent<Renderer>().material;
    }

    IEnumerator prepareModel()
    {
        yield return StartCoroutine(extractFile("", "MobileNetSSD_deploy.caffemodel"));
        yield return StartCoroutine(extractFile("", "MobileNetSSD_deploy.prototxt"));

        prepareModel(Application.persistentDataPath);

        modelReady = true;
    }

    private void clearVisuals()
    {
        foreach (Transform child in markerParent.transform) GameObject.Destroy(child.gameObject);
    }

    private void addVisual(string name, float confidence, float sx, float sy, float ex, float ey)
    {
        GameObject g = GameObject.Instantiate(markerTemplate);
        g.transform.position = new Vector3((-5.0f * (sx + ex) + 5.0f) / 10.0f, (-5.0f * (sy + ey) + 5.0f) / 10.0f, 0.51f);
        g.transform.localScale = new Vector3(Mathf.Abs(sx - ex), Mathf.Abs(sx - ex), 1);
        g.GetComponentInChildren<TMP_Text>().text = name + "\n" + confidence;
        g.transform.SetParent(markerParent.transform, false);
    }

    // Update is called once per frame
    void Update()
    {
        delayTime += Time.deltaTime;

        if (modelReady && (delayTime > 2.0f))
        {
            clearVisuals();
            delayTime = 0.0f;

            Debug.Log("cameraMaterial _BaseMap: " + cameraMaterial.GetTexture("_BaseMap"));
            Debug.Log("cameraMaterial _MainTex: " + cameraMaterial.GetTexture("_MainTex"));
            Texture2D image = new Texture2D(cameraMaterial.GetTexture("_BaseMap").width, cameraMaterial.GetTexture("_BaseMap").height, TextureFormat.ARGB32, false);
            RenderTexture renderTexture = new RenderTexture(cameraMaterial.mainTexture.width, cameraMaterial.mainTexture.height, 32);
            Graphics.Blit(cameraMaterial.mainTexture, renderTexture);
            RenderTexture.active = renderTexture;
            image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            image.Apply();

            int numMatch = doRecognise(image.GetRawTextureData(), image.width, image.height);

            text.text = "Matches: " + numMatch;

            for (int i = 0; i < numMatch; i++)
            {
                int category = -1;
                float confidence = 0.0f;
                float sx = 0, sy = 0, ex = 0, ey = 0;
                retrieveMatch(i, ref category, ref confidence, ref sx, ref sy, ref ex, ref ey);
                if (confidence > 0.2f)
                {
                    Debug.Log($"Match: {CLASSES[category]} {confidence} {sx} {sy} {ex} {ey}");
                    addVisual(CLASSES[category], confidence, sx, sy, ex, ey);
                    //addVisual(CLASSES[0], 1.0f, 0.5f, 0.5f, 1.0f, 1.0f);
                }
            }
        }
    }

    IEnumerator extractFile(string assetPath, string assetFile)
    {
        string sourcePath = Application.streamingAssetsPath + "/" + assetPath + assetFile;
        if ((sourcePath.Length > 0) && (sourcePath[0] == '/'))
        {
            sourcePath = "file://" + sourcePath;
        }
        string destinationPath = Application.persistentDataPath + "/" + assetFile;

        WWW w = new WWW(sourcePath);
        yield return w;
        try
        {
            File.WriteAllBytes(destinationPath, w.bytes);
        }
        catch (Exception e)
        {
            Debug.Log("Issue writing: " + destinationPath + " " + e);
        }
        Debug.Log(sourcePath + " -> " + destinationPath + " " + w.bytes.Length);
    }
}
