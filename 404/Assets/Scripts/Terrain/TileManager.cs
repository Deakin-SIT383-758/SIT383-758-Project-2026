using System.Collections;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    private GameObject[] tiles; // holds all active tile GameObjects
    public GameObject tilePrefab; // prefab for tiles

    public Transform trackedObject; // object to update tiles around

    public float scale = 10.0f; // scale of tiles

    public float longitude = 0.0f; // 144.96f for Melbourne
    public float latitude = 0.0f; // -37.81f for Melbourne

    public int zoom = 14;

    // boundary of centre tile
    private float leftBounds;
    private float rightBounds;
    private float upBounds;
    private float downBounds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tiles = new GameObject[9];
        CreateTiles();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 objectPos = new Vector2(trackedObject.position.x, trackedObject.position.z);
        Debug.Log("Tracked object position: " + objectPos);
        Debug.Log($"X bounds at {leftBounds} and {rightBounds}");
        Debug.Log($"Y bounds at {downBounds} and {upBounds}");

        // tile offsets to recentre
        int xOff = 0;
        int yOff = 0;

        // test if tracked object is within centre tile
        if (objectPos.x > leftBounds && objectPos.x < rightBounds) Debug.Log("X Position centred");
        else if (objectPos.x < leftBounds) { Debug.Log("X position to the left"); xOff = 1; }
        else if (objectPos.x > rightBounds) { Debug.Log("X position to the right"); xOff = -1; }

        if (objectPos.y < upBounds && objectPos.y > downBounds) Debug.Log("Y Position centred");
        else if (objectPos.y > upBounds) { Debug.Log("Y position upwards"); yOff = 1; }
        else if (objectPos.y < downBounds) { Debug.Log("Y position downwards"); yOff = -1; }

        if (xOff != 0 || yOff != 0) Recentre(xOff, yOff);
    }

    private void CreateTiles()
    {
        tiles = new GameObject[9];
        int index = 0;
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                GameObject t = Instantiate(tilePrefab, transform.position + new Vector3(x * scale, 0.0f, -y * scale), Quaternion.identity); // instantiate tiles to form 3x3 square
                t.GetComponent<MapManager>().tileX = x;
                t.GetComponent<MapManager>().tileY = y;
                t.GetComponent<MapManager>().zoom = zoom;
                tiles[index] = t;
                index++;

                if (x == 0 && y == 0)
                {
                    // set bounds of centre tile
                    leftBounds = t.transform.position.x - (scale / 2);
                    rightBounds = t.transform.position.x + (scale / 2);
                    upBounds = t.transform.position.z + (scale / 2);
                    downBounds = t.transform.position.z - (scale / 2);
                }
            }
        }
    }

    private void Recentre(int xOff, int yOff)
    {
        Debug.Log("Tiles recentre");
        foreach (GameObject t in tiles)
        {
            t.GetComponent<MapManager>().tileX += xOff;
            t.GetComponent<MapManager>().tileY += yOff;

            if (t.GetComponent<MapManager>().tileX == 0 && t.GetComponent<MapManager>().tileY == 0)
            {
                // set bounds of centre tile
                leftBounds = t.transform.position.x - (scale / 2);
                rightBounds = t.transform.position.x + (scale / 2);
                upBounds = t.transform.position.z + (scale / 2);
                downBounds = t.transform.position.z - (scale / 2);

                TileCentreCoords(t.GetComponent<MapManager>().mapX, t.GetComponent<MapManager>().mapY, zoom, out latitude, out longitude);
            }

            if (t.GetComponent<MapManager>().tileX == 2 || t.GetComponent<MapManager>().tileX == -2)
            {
                t.GetComponent<MapManager>().tileX = -(t.GetComponent<MapManager>().tileX / 2); // roll around to other side of grid
                t.transform.position += (Vector3.left * (scale * 3 * xOff)); // move tile
                t.GetComponent<MapManager>().onButtonEvent(-xOff, 0, 0); // update textures
            }

            if (t.GetComponent<MapManager>().tileY == 2 || t.GetComponent<MapManager>().tileY == -2)
            {
                t.GetComponent<MapManager>().tileY = -(t.GetComponent<MapManager>().tileY / 2); // roll around to other side of grid
                t.transform.position += (Vector3.forward * (scale * 3 * yOff)); // move tile
                t.GetComponent<MapManager>().onButtonEvent(0, yOff, 0); // update textures
            }
        }

        foreach (GameObject t in tiles)
        {
            t.GetComponent<MapManager>().latitude = latitude;
            t.GetComponent<MapManager>().longitude = longitude;
        }
    }

    private void TileCentreCoords(int x, int y, int zoom, out float latitude, out float longitude)
    {
        float n = Mathf.Pow(2.0f, zoom);

        // add 0.5 to tile indices to calculate centre point
        float centreX = x + 0.5f;
        float centreY = y + 0.5f;

        // calculate coordinates
        latitude = Mathf.Atan((float)System.Math.Sinh((Mathf.PI * (1 - 2 * centreY / n)))) * Mathf.Rad2Deg;
        longitude = centreX / n * 360.0f - 180.0f;
    }
}
