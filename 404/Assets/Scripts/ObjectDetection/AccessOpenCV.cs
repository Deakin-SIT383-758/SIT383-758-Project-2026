using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.XR.LegacyInputHelpers;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Net.NetworkInformation;

public class AccessOpenCV : MonoBehaviour
{
    public Material cameraMaterial;

    public GameObject markerTemplate;
    public GameObject markerParent;

    public GameObject hudTemplate;
    public GameObject hudParent;

    private Dictionary<string, Vector3> objects = new Dictionary<string, Vector3>(); // names of objects and positions


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
        //cameraMaterial = markerParent.GetComponent<Renderer>().material;

        // Fill object dictionary
        objects.Add("North Chair", new Vector3(0.0599999987f, 2.30999994f, 13.46f));
        objects.Add("West Chair", new Vector3(-11.3902359f, 2.30999994f, -0.106083527f));
        objects.Add("East Chair", new Vector3(14.1231422f, 2.30999994f, 0.0802500024f));
        objects.Add("South Chair", new Vector3(0.316463053f, 2.30999994f, -11.6630659f));
        objects.Add("Far South Chair", new Vector3(0.920000017f, 2.30999994f, -28.2399998f));
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
        // Markers for demonstration view
        GameObject g = GameObject.Instantiate(markerTemplate);
        g.transform.position = new Vector3((-5.0f * (sx + ex) + 5.0f) / 10.0f, (-5.0f * (sy + ey) + 5.0f) / 10.0f, 0.51f);
        g.transform.localScale = new Vector3(Mathf.Abs(sx - ex), Mathf.Abs(sy - ey), 1);
        g.GetComponentInChildren<TMP_Text>().text = name + "\n" + confidence;
        g.transform.SetParent(markerParent.transform, false);

        // Markers for user
        Ray centreRay = Camera.main.ScreenPointToRay(new Vector3(Mathf.RoundToInt(Mathf.Abs((sx + ex) / 2.0f) * Camera.main.pixelWidth - 1), Mathf.RoundToInt(Mathf.Abs((sy + ey) / 2) * Camera.main.pixelHeight - 1), 0)); // cast ray from camera
        Debug.Log($"Centre pixel: {Mathf.RoundToInt(Mathf.Abs((sx + ex) / 2.0f) * Camera.main.pixelWidth - 1)}, {Mathf.RoundToInt(Mathf.Abs((sy + ey) / 2) * Camera.main.pixelHeight - 1)}");
        Debug.DrawRay(centreRay.origin, centreRay.direction * 50.0f, Color.green, 3.0f);
        //GameObject m = Instantiate(hudTemplate, centreRay.origin + (centreRay.direction * 50.0f), quaternion.identity, hudParent.transform);
        RaycastHit hit;
        Physics.Raycast(centreRay.origin, centreRay.direction, out hit);
        Debug.Log($"Raycast hit at: {hit.point}, distance {hit.distance}");
        if (hit.distance == 0) return; // Don't place marker if raycast doesn't hit
        GameObject m = Instantiate(hudTemplate, hit.point, quaternion.identity, null);

        // Attempt to identify specific object
        string bestMatch = name;
        float lowestAngle = 180.0f;
        foreach (var o in objects)
        {
            Ray objectRay = new Ray(Camera.main.transform.position, -(Camera.main.transform.position - o.Value).normalized); // cast ray from camera to object
            Debug.DrawRay(objectRay.origin, objectRay.direction * 50.0f, Color.red, 3.0f);
            float angle = Vector3.Angle(centreRay.direction, objectRay.direction);
            Debug.Log($"Angle to {o.Key}: {angle}");

            if (angle < lowestAngle) // find object with lowest angle
            {
                lowestAngle = angle;
                bestMatch = o.Key;
            }
        }
        m.GetComponent<ObjectHUDMarker>().SetText(bestMatch);
    }

    // Update is called once per frame
    void Update()
    {
        delayTime += Time.deltaTime;

        if (modelReady && (delayTime > 2.0f))
        {
            clearVisuals();
            delayTime = 0.0f;

            RenderTexture renderTexture = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, 32);
            //Graphics.Blit(Camera.main.activeTexture, renderTexture);
            Camera.main.targetTexture = renderTexture;
            Camera.main.Render(); // ensure camera frame is rendered
            RenderTexture.active = renderTexture;
            Debug.Log($"Camera.main.pixelWidth/pixelHeight size: {Camera.main.pixelWidth}, {Camera.main.pixelHeight}");
            Debug.Log($"renderTexture size: {renderTexture.width}, {renderTexture.height}");

            Texture2D image = new Texture2D(Camera.main.pixelWidth, Camera.main.pixelHeight, TextureFormat.ARGB32, false);

            image.ReadPixels(new Rect(0, 0, Camera.main.pixelWidth, Camera.main.pixelHeight), 0, 0);
            image.Apply();

            Camera.main.targetTexture = null;
            //RenderTexture.active = null;
            //Destroy(renderTexture);

            //File.WriteAllBytes("frame.png", image.EncodeToPNG());
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
                    //addVisual(CLASSES[0], 1.0f, 0.25f, 0.25f, 0.75f, 0.75f);
                }
            }
            GetComponent<Renderer>().material.SetTexture("_BaseMap", renderTexture);
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
