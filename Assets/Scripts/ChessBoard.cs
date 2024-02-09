using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.AnnotationUtility;

public class ChessBoard : MonoBehaviour
{


    [Header("Board Centering")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    //chess board size
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;

    [Header("Prefabs and Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    private ChessPiece[,] ChessPieces;

    private GameObject[,] tiles; 
    private Camera currentCamera;

    private Vector2Int currentHover;

    private Vector3 bounds;
    private void Awake()
    {
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        SpawnAllPieces("aboba");

        PositionAllPieces();
    }
    private void Update()
    {
        if(!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out info,100, LayerMask.GetMask("Tile", "Hover")))
        {
            //Get the indexes of the tiles raycast hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            //If we hovering a tile after not govering any tile
            if(currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            //If we were already hovering a tile, change the prev one
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
        }
        else
        {
            if(currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
        }
    }
    /// <summary>
    /// Generating board with meshes
    /// </summary>
    /// <param name="tileSize">Scale of tile</param>
    /// <param name="tileCountX">Number of tiles in X</param>
    /// <param name="tileCountY">Number of tiles in Y</param>
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize,0, (tileCountY / 2) * tileSize) + boardCenter;
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
        vertices[0] = new Vector3(x * tileSize, yOffset,y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset,(y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset,y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset,(y+1) * tileSize) - bounds;
        //creating triangles
        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        tileObject.AddComponent<BoxCollider>();

        tileObject.layer = LayerMask.NameToLayer("Tile");// assgins object to layer Tile

        return tileObject;
    }

    /// <summary>
    /// Checks if the Hit tile is  correcnt
    /// </summary>
    /// <param name="hitInfo">Tile information</param>
    /// <returns>Returns coordinates or False</returns>
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0;x < TILE_COUNT_X;x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x,y] == hitInfo)
                    return new Vector2Int(x,y);
            }
        }
        return -Vector2Int.one;// return false Vector (Invalid)
    }

    private void SpawnAllPieces(string FenNotation)
    {
        ChessPieces = new ChessPiece[TILE_COUNT_X,TILE_COUNT_Y];

        int whiteTeam = 0, blackTeam = 1;

        // FUTURE UPDATE: Add a FEN notation for the pieces


        //White team Pattern
        ChessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        ChessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        ChessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        ChessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        ChessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        ChessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        ChessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        ChessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for (int i = 0; i < 8; i++)
            ChessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);

        //Black team Pattern
        ChessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        ChessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        ChessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        ChessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        ChessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        ChessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        ChessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        ChessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < 8; i++)            
            ChessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);

    }
    /// <summary>
    /// Spawn a single Chess piece type
    /// </summary>
    /// <param name="type">Piece Rank</param>
    /// <param name="team">Chess Team</param>
    /// <returns>Chess Piece</returns>
    private ChessPiece SpawnSinglePiece(ChessPieceType type , int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1],transform).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;

        Material[] mats = new Material[2]{ teamMaterials[team], teamMaterials[team] };
        cp.GetComponent<MeshRenderer>().materials = mats;

        return cp;
    }
    /// <summary>
    /// Position All the Pieces on Tiles
    /// </summary>
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (ChessPieces[x, y] != null)
                    PositionSinglePiece(x, y, true);
    }
    /// <summary>
    /// Position A single Piece on the board
    /// </summary>
    /// <param name="x">x coordinates</param>
    /// <param name="y">y coordinates</param>
    /// <param name="force"> pieces snap in place</param>
    private void PositionSinglePiece(int x, int y ,bool force = false)
    {
        ChessPieces[x, y].currentX = x;
        ChessPieces[x, y].currentY = y;

        ChessPieces[x , y].transform.position = GetTileCenter(x, y);
    }
    private Vector3 GetTileCenter(int x,int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2); 
    }
}

