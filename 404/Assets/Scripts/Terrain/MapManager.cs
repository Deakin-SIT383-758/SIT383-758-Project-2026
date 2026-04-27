using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using TMPro;
using System.IO;
using System.Buffers.Text;
using Unity.Mathematics;
using UnityEngine.Tilemaps;
using System.Collections;
using UnityEngine.Networking;
using UnityEditor.PackageManager.Requests;

public class MapManager : MonoBehaviour
{
    //public TMP_Text statusText;

    public MeshFilter mapObject;
    public Material mapMaterial;

    public GameObject marker;
    public GameObject mapPlane;

    public float longitude = 0.0f; // 144.96f for Melbourne
    public float latitude = 0.0f; // 37.81f for Melbourne
    private int zoom = 14;

    public int tileX = 0; // coordinates of tile in tile group
    public int tileY = 0;

    public int mapX = 0; // coordinates in tiles of terrain/map texture
    public int mapY = 0;

    private static bool TrustCertificate(object sender, X509Certificate x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
    {
        return true; // Bad practice
    }

    void Start()
    {
        mapObject = this.GetComponent<MeshFilter>();
        mapPlane = this.gameObject;

        mapMaterial = GetComponent<Renderer>().material;

        ServicePointManager.ServerCertificateValidationCallback = TrustCertificate;
        updateMapView();
    }

    private void getTileCoordinates(float longitude, float latitude, int zoom, out int x, out int y)
    {
        x = (int)(Mathf.Floor((longitude + 180.0f) / 360.0f * Mathf.Pow(2.0f, zoom)));
        y = (int)(Mathf.Floor((1.0f - Mathf.Log(Mathf.Tan(latitude * Mathf.PI / 180.0f) + 1.0f / Mathf.Cos(latitude * Mathf.PI / 180.0f)) / Mathf.PI)
        / 2.0f * Mathf.Pow(2.0f, zoom)));
        mapX = x + tileX;
        mapY = y + tileY;
        Debug.Log($"Tile coordinates: {x} , {y}");
    }

    private void getGeoCoordinates(int x, int y, int zoom, out float longitude, out float latitude)
    {
        float n = Mathf.PI - 2.0f * Mathf.PI * y / Mathf.Pow(2.0f, zoom);

        longitude = x / Mathf.Pow(2.0f, zoom) * 360.0f - 180.0f;
        latitude = 180.0f / Mathf.PI * Mathf.Atan(0.5f * (Mathf.Exp(n) - Mathf.Exp(-n)));
    }

    private float[] convertElevationTexture(Texture2D tex, out float minHeight, out float maxHeight)
    {
        float[] hTexData = new float[tex.width * tex.height];
        bool setLimits = false;
        minHeight = 0.0f;
        maxHeight = 0.0f;

        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                Color a = tex.GetPixel(x, y);
                float height = ((a.r * 255.0f * 256.0f) + (a.g * 255.0f) + (a.b * 255.0f / 256.0f)) - 32768.0f;
                hTexData[y * tex.width + x] = height;

                if ((!setLimits) || (height < minHeight))
                {
                    minHeight = height;
                }
                if ((!setLimits) || (height > maxHeight))
                {
                    maxHeight = height;
                }
                setLimits = true;
            }
        }
        return hTexData;
    }


    private void updateMesh(Texture2D tex, int mWidth, int mHeight, float heightRange)
    {
        float minHeight;
        float maxHeight;
        float[] heightTex = convertElevationTexture(tex, out minHeight, out maxHeight);

        Vector3[] vertices = new Vector3[(mWidth + 1) * (mHeight + 1)];
        Vector2[] uvs = new Vector2[(mWidth + 1) * (mHeight + 1)];
        int[] triangles = new int[6 * mWidth * mHeight];

        int triangleIndex = 0;
        for (int y = 0; y <= mHeight; y++)
        {
            for (int x = 0; x <= mWidth; x++)
            {
                float xc = (float)x / mWidth;
                float zc = (float)y / mHeight;
                //float yc = 0.0f;
                float yc = heightTex[(int)(zc * (tex.height - 1)) * tex.width + (int)(xc * (tex.width - 1))];
                if (yc < 0.0f) yc = 0.0f;
                //yc = heightRange * (yc - minHeight) / (maxHeight - minHeight);
                yc *= 0.001f; //temporary height scaling for demo purposes

                vertices[y * (mWidth + 1) + x] = new Vector3(xc - 0.5f, yc, zc - 0.5f) * 10.0f;
                uvs[y * (mWidth + 1) + x] = new Vector3(xc, zc);

                //Instantiate(marker, new Vector3(xc - 0.5f, yc, zc - 0.5f) * 10.0f, Quaternion.identity);

                // Skip last row/col
                if ((x != mWidth) && (y != mHeight))
                {
                    int topLeft = y * (mWidth + 1) + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + mWidth + 1;
                    int bottomRight = bottomLeft + 1;

                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomRight;
                }
            }
        }

        Mesh m = new Mesh();
        m.vertices = vertices;
        m.uv = uvs;
        m.triangles = triangles;
        m.RecalculateNormals();
        mapObject.mesh = m;

        GetComponent<MeshCollider>().sharedMesh = m; // update collider's mesh
    }

    IEnumerator updateTexture(int x, int y, int z)
    {
        // Elevation tiles, sourced from Mapzen (https://www.mapzen.com/blog/terrain-tile-service/)
        string url = "https://s3.amazonaws.com/elevation-tiles-prod/terrarium/" + z + "/" + x + "/" + y + ".png";
        Debug.Log("Retrieving: " + url);

        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url)) // use non-blocking UnityWebRequest over blocking HttpWebRequest
        {
            req.SetRequestHeader("User-Agent", "TerrainMaps");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Web request failed: " + req.error);
                yield break;
            }

            Texture2D tex = DownloadHandlerTexture.GetContent(req); // get texture data from web request
            updateMesh(tex, 128, 128, 0.1f);
            //mapMaterial.mainTexture = tex;
        }
    }
    IEnumerator updateColourTexture(int x, int y, int z)
    {
        // Map tiles sources from openstreetmap
        string url = "https://tile.openstreetmap.org/" + z + "/" + x + "/" + y + ".png";
        Debug.Log("Retrieving: " + url);

        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url)) // use non-blocking UnityWebRequest over blocking HttpWebRequest
        {
            req.SetRequestHeader("User-Agent", "TerrainMaps");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Web request failed: " + req.error);
                yield break;
            }

            Texture2D tex = DownloadHandlerTexture.GetContent(req); // get texture data from web request
            //updateMesh(tex, 128, 128, 0.1f);
            mapMaterial.mainTexture = tex;
        }
    }

    [ContextMenu("Update map")]
    private void updateMapView()
    {
        int x;
        int y;
        getTileCoordinates(longitude, latitude, zoom, out x, out y);

        x += tileX; // offset from centre tile
        y += tileY;

        StartCoroutine(updateTexture(x, y, zoom));
        StartCoroutine(updateColourTexture(x, y, zoom));

        // Place a marker at current position on the tile
        float cornerLatA;
        float cornerLongA;
        float cornerLatB;
        float cornerLongB;
        getGeoCoordinates(x, y, zoom, out cornerLongA, out cornerLatA);
        getGeoCoordinates(x + 1, y + 1, zoom, out cornerLongB, out cornerLatB);

        // Interpolate current coordinates relative to tile corners
        // Assumes the plane coordinates run from (-5, -5) to (5, 5)
        float r = 10.0f * ((-(longitude - cornerLongA) / (cornerLongB - cornerLongA))) + 5.0f;
        float d = 10.0f * ((-(latitude - cornerLatA) / (cornerLatB - cornerLatA))) + 5.0f;
        marker.transform.position = mapPlane.transform.position - mapPlane.transform.forward * d + mapPlane.transform.right * r;

        //statusText.text = longitude + "," + latitude + "(" + zoom + ")" + "[" + x + ", " + y + "]";
    }

    public void onButtonEvent(float dx, float dy, int dz)
    {
        zoom += dz;

        // Calculate step size to take several button
        // presses to traverse tile present at zoom level
        float step = 1.0f * 1.0f / Mathf.Pow(2.0f, zoom);
        longitude += 360.0f * dx * step;
        latitude += 180.0f * dy * step;


        updateMapView();
    }

    [ContextMenu("Left")]
    public void leftButton() { onButtonEvent(-1.0f, 0.0f, 0); }
    [ContextMenu("Right")]
    public void rightButton() { onButtonEvent(1.0f, 0.0f, 0); }
    [ContextMenu("Up")]
    public void upButton() { onButtonEvent(0.0f, 1.0f, 0); Debug.Log("Tile moved up"); }
    [ContextMenu("Down")]
    public void downButton() { onButtonEvent(0.0f, -1.0f, 0); Debug.Log("Tile moved down"); }
    [ContextMenu("In")]
    public void inButton() { onButtonEvent(0.0f, 0.0f, 1); }
    [ContextMenu("Out")]
    public void outButton() { onButtonEvent(0.0f, 0.0f, -1); }
}
