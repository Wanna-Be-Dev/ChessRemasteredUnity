using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using static Unity.VisualScripting.AnnotationUtility;
using System.Diagnostics;

public enum SpecialMove
{
    None = 0,
    EnPassant,
    Castleing,
    Promotion
}
public class ChessBoard : MonoBehaviour
{


    [Header("Board Centering")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deadSize = 0.3f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private float dragOffset = 1f;
    //chess board size
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;

    [Header("Prefabs and Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    [Header("UI elements")]
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private TMP_Text winnerText;

    private ChessPiece currentlyDraging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();

    //Chess piece Location
    private ChessPiece[,] ChessPieces;
    //Tile Location
    private GameObject[,] tiles; 
    //Main Camera
    private Camera currentCamera;

    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool isWhiteTurn;

    private List<Vector2Int[]> movelist = new List<Vector2Int[]>();
    private SpecialMove specialMove;

    private void Awake()
    {
        isWhiteTurn = true;
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        SpawnAllPieces("aboba");

        PositionAllPieces();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M)) 
            DebugChessBoard();

        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out info,100, LayerMask.GetMask("Tile", "Hover","Highlight")))
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
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //Mouse Pressed
            if(Input.GetMouseButtonDown(0))
            {
                if (ChessPieces[hitPosition.x,hitPosition.y] != null)
                {
                    //check for turn ? (set true for unlimeted turns)
                    if ((ChessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (ChessPieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
                    {
                        currentlyDraging = ChessPieces[hitPosition.x , hitPosition.y];

                        // get  list of moves for piece, highlight tiles as well
                        availableMoves = currentlyDraging.GetAvailableMoves(ref ChessPieces , TILE_COUNT_X,TILE_COUNT_Y);
                        //get a list of special moves list
                        specialMove = currentlyDraging.GetSpecialMoves(ref ChessPieces, ref movelist, ref availableMoves);

                        HighlightTiles(true);
                    }
                }
            }
            //Mouse released
            if(currentlyDraging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDraging.currentX,currentlyDraging.currentY);

                bool validMove = MoveTo(currentlyDraging, hitPosition.x, hitPosition.y);
                
                if (!validMove)
                    currentlyDraging.SetPosition(GetTileCenter(previousPosition.x , previousPosition.y));

                currentlyDraging = null;
                HighlightTiles(false);
            }
            
        }
        else
        {
            if(currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
            if(currentlyDraging && Input.GetMouseButtonDown(0))
            {
                currentlyDraging.SetPosition(GetTileCenter(currentlyDraging.currentX, currentlyDraging.currentY));
                currentlyDraging = null;
                HighlightTiles(false);
            }
        }

        //if we're dragging a piece
        if (currentlyDraging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
                currentlyDraging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }
    }
    // Generate The board
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
    //Spawning of the Pieces
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

    }//change here after
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
    //Positioning
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

        ChessPieces[x , y].SetPosition(GetTileCenter(x, y),force);
    }
    /// <summary>
    ///  Get Position of the center of the piece
    /// </summary>
    /// <param name="x">Coordinates X</param>
    /// <param name="y">Coordinates Y</param>
    /// <returns> Center Point of tile</returns>
    private Vector3 GetTileCenter(int x,int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2); 
    }

    //Operations
    /// <summary>
    /// Move The Piece to square
    /// </summary>
    /// <param name="cp">current piece</param>
    /// <param name="x">move to X coord</param>
    /// <param name="y">move to Y coord</param>
    /// <returns>can or cannot move</returns>
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
            return false;

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        //Check if there is another piece on the target position

        if (ChessPieces[x,y] != null)
        {
            ChessPiece ocp = ChessPieces[x,y];
            if(cp.team == ocp.team)
                return false;   

            //eating pieces
            if(ocp.team == 0)
            {
                if (ocp.type == ChessPieceType.King)
                    CheckMate(1);
                deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deadSize);
                ocp.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) 
                    - bounds 
                    + new Vector3(tileSize /2 , 0 ,tileSize / 2)
                    + (Vector3.forward * deathSpacing) * deadWhites.Count);
;            }
            else
            {
                if (ocp.type == ChessPieceType.King)
                    CheckMate(0);

                deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * deadSize);
                ocp.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.back * deathSpacing) * deadBlacks.Count);
            }
        }

        ChessPieces[x, y] = cp;
        ChessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);
        isWhiteTurn = !isWhiteTurn;
        movelist.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y)});

        ProcessSpecialMove();

        return true;
    }
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for(int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;

        return false;
    }
    //HighLight availiable tiles
    private void HighlightTiles(bool state)
    {
        if(state)
        {
            for (int i = 0; i < availableMoves.Count; i++)
                tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
            
        }
        else
        {
            for (int i = 0; i < availableMoves.Count; i++)
                tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
            availableMoves.Clear();
        }
        
        
    }
    //Prints simble notation
    private void DebugChessBoard()
    {
        string chezBoard = "";
        for (int y = 0; y < TILE_COUNT_Y; y++)
        {
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                if (ChessPieces[x, y] != null)
                {
                    ChessPiece type = ChessPieces[x, y];
                    if(type.team  == 0)
                        chezBoard += " " + type.type.ToString().Substring(0, 1).ToUpper();
                    else
                        chezBoard += " " + type.type.ToString().Substring(0, 1).ToLower();
                }
                if (ChessPieces[x, y] == null)
                {
                    chezBoard += " o";
                }
            }
            chezBoard += "\n";
        }
        //Debug.Log(chezBoard);
    }
    //CheckMate
    private void CheckMate(int team)
    {
        DisplayVictory(team);
    }
    private void DisplayVictory(int WinningTeam)
    {
        if (WinningTeam == 0)
            winnerText.text = "White team is the winner";
        else
            winnerText.text = "Black team is the winner";

        victoryScreen.SetActive(true);
    }
    // UI
    public void OnResetButton()
    {
        //UI
        victoryScreen.SetActive(false);

        //Field reset
        currentlyDraging = null;
        availableMoves.Clear();
        movelist.Clear();

        //cleaning chess pieces
        for (int x = 0; x < TILE_COUNT_Y; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (ChessPieces[x, y] != null)
                    Destroy(ChessPieces[x, y].gameObject);

                ChessPieces[x, y] = null;
            }
        }
        for (int i = 0; i < deadWhites.Count; i++)
            Destroy(deadWhites[i].gameObject);
        for (int i = 0; i < deadBlacks.Count; i++)
            Destroy(deadBlacks[i].gameObject);
        deadBlacks.Clear();
        deadWhites.Clear();

        SpawnAllPieces("aboba");
        PositionAllPieces();
        isWhiteTurn = true;
    }
    public void OnExitButton()
    {
        Application.Quit();
    }

    //special Moves
    private void ProcessSpecialMove()
    {
        if(specialMove == SpecialMove.EnPassant)
        {
            var newMove = movelist[movelist.Count - 1];
            ChessPiece myPawn = ChessPieces[newMove[1].x, newMove[1].y];
            var targetPawnAPosition = movelist[movelist.Count - 2];
            ChessPiece enemyPawn = ChessPieces[targetPawnAPosition[1].x, targetPawnAPosition[1].y];

            if(myPawn.currentX == enemyPawn.currentX)
            {
                if(myPawn.currentY ==  enemyPawn.currentY - 1|| myPawn.currentY == enemyPawn.currentY + 1) 
                {
                    if(enemyPawn.team == 0)
                    {
                        deadWhites.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deadSize);
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.forward * deathSpacing) * deadWhites.Count);
                    }
                    else 
                    {
                        deadBlacks.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deadSize);
                        enemyPawn.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.forward * deathSpacing) * deadBlacks.Count);
                    }

                    ChessPieces[enemyPawn.currentX,enemyPawn.currentY] = null;
                }
            }


        }
    }
}

