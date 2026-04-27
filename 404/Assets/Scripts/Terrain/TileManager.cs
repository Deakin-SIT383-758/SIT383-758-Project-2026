using System.Collections;
using Unity.Collections;
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

        // test if tracked object is within centre tile
        if (objectPos.x > leftBounds && objectPos.x < rightBounds) Debug.Log("X Position centred");
        else if (objectPos.x < leftBounds) Debug.Log("X position to the left");
        else if (objectPos.x > rightBounds) Debug.Log("X position to the right");

        if (objectPos.y < upBounds && objectPos.y > downBounds) Debug.Log("Y Position centred");
        else if (objectPos.y > upBounds) Debug.Log("Y position upwards");
        else if (objectPos.y < downBounds) Debug.Log("Y position downwards");
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
}
