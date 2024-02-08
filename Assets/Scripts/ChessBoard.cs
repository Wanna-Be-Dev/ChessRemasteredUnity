using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    [Header("Art")]
    [SerializeField]private Material tileMaterial;
    //chess board size
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;

    private GameObject[,] tiles; 
    private Camera currentCamera;
    private void Awake()
    {
        GenerateAllTiles(1, 8, 8);
    }
    private void Update()
    {
        if(!currentCamera)
        {
            currentCamera = Camera.current;
            return;
        }
        RaycastHit info;
    }
    /// <summary>
    /// Generating board with meshes
    /// </summary>
    /// <param name="tileSize">Scale of tile</param>
    /// <param name="tileCountX">Number of tiles in X</param>
    /// <param name="tileCountY">Number of tiles in Y</param>
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for(int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }
    /// <summary>
    /// Generating a single tile of the board
    /// </summary>
    /// <param name="tileSize">size of tile</param>
    /// <param name="x">Coordinates in X</param>
    /// <param name="y">Coordinates in Y</param>
    /// <returns>tile object</returns>
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        //creating
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, 0,y * tileSize);
        vertices[1] = new Vector3(x * tileSize, 0,(y+1) * tileSize);
        vertices[2] = new Vector3((x+1) * tileSize, 0,y * tileSize);
        vertices[3] = new Vector3((x+1) * tileSize, 0,(y+1) * tileSize);
        //creating triangles
        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }
}
///test
