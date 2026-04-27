using System.Collections;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    private GameObject[] tiles; // holds all active tile GameObjects

    public GameObject tilePrefab; // prefab for tiles

    public float scale = 10.0f; // scale of tiles

    public float longitude = 0.0f; // 144.96f for Melbourne
    public float latitude = 0.0f; // -37.81f for Melbourne

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tiles = new GameObject[9];
        CreateTiles();
    }

    // Update is called once per frame
    void Update()
    {

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
            }
        }
    }
}
