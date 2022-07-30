using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Networking.Transport;
using UnityEngine.UI;

public enum SpecialMove
{ 
    None = 0,
    EnPassant,
    Castling,
    Promotion

}

public class ChessBoard : MonoBehaviour
{
    [Header("Art Stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 35f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private float dragOffset = 1f;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject CheckScreen;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text rematchIndicatorText;
    [SerializeField] private Button rematchButton;

    [SerializeField] private TMP_Text whiteTimerText;
    [SerializeField] private TMP_Text blackTimerText;

    [Header("Prefab & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    // Logic
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<Vector2Int> specialMoves = new List<Vector2Int>();
    private List<Vector2Int> attackMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool isWhiteTurn;
    private SpecialMove specialMove;
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();

    [SerializeField] private GameObject whiteCharacter;
    [SerializeField] private GameObject blackCharacter;

    private float timerInSeconds = 300f;
    private float whiteTimerValue = 0f;
    private float blackTimerValue = 0f;


    public bool firstMove = false;
    public bool hasGameStarted = false;

    // Multiplayer Logic
    private int playerCount = -1;
    private int currentTeam = -1;
    private int initialAssignedTeam = -1;
    private bool localGame = true;
    private bool[] playerRematch = new bool[2];

    private int random_team;
    private int surrenderedTeam;
    private bool[] playerSurrender = new bool[2];

    private void Awake()
    {
        GetRandomTeam();
        ResetTimers();
    }

    public void ResetTimers()
    {
        whiteTimerValue = blackTimerValue = timerInSeconds;

        float minutes = Mathf.FloorToInt(timerInSeconds / 60);
        float seconds = Mathf.FloorToInt(timerInSeconds % 60);

        whiteTimerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
        blackTimerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private void GetRandomTeam()
    {
        System.Random rnd = new System.Random();
        random_team = rnd.Next(2);
    }

    private void Start()
    {
        isWhiteTurn = true;

        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();

        RegisterEvents();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        if (localGame)
        {
            if (hasGameStarted && firstMove)
            {
                if (currentTeam == 0)
                {
                    if (whiteTimerValue > 0)
                    {
                        whiteTimerValue -= Time.deltaTime;

                        float minutes = Mathf.FloorToInt(whiteTimerValue / 60);
                        float seconds = Mathf.FloorToInt(whiteTimerValue % 60);

                        whiteTimerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
                    }
                    else
                    {
                        CheckMate(1);
                    }
                }
                else if (currentTeam == 1)
                {
                    if (blackTimerValue > 0)
                    {
                        blackTimerValue -= Time.deltaTime;

                        float minutes = Mathf.FloorToInt(blackTimerValue / 60);
                        float seconds = Mathf.FloorToInt(blackTimerValue % 60);

                        blackTimerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
                    }
                    else
                    {
                        CheckMate(0);
                    }
                }
            }
        }
        else
        {
            if (hasGameStarted && firstMove)
            {
                if (currentTeam == 0 && !isWhiteTurn)
                {
                    // Net implementation
                    NetTimer nt = new NetTimer();
                    nt.teamId = currentTeam;
                    if (whiteTimerValue > 0)
                    {
                        whiteTimerValue -= Time.deltaTime;

                        nt.timer = whiteTimerValue;
                    }
                    Client.Instance.SendToServer(nt);
                }
                else if (currentTeam == 1 && isWhiteTurn)
                {
                    // Net implementation
                    NetTimer nt = new NetTimer();
                    nt.teamId = currentTeam;
                    if (blackTimerValue > 0)
                    {
                        blackTimerValue -= Time.deltaTime;

                        nt.timer = blackTimerValue;
                    }
                    Client.Instance.SendToServer(nt);
                }
            }
        }

        if(!FindObjectOfType<GameManager>().isGamePaused)
        {
            RaycastHit info;
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight", "Attack", "Special")))
            {
                // Get the indexes of the tile i've hit
                Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

                // If we are hovering a tile after not hovering any tiles
                if(currentHover == -Vector2Int.one)
                {
                    currentHover = hitPosition;
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                }

                // If we were already hovering a tile, change the previous one
                if (currentHover != hitPosition)
                {
                    if (ContainsValidMove(ref availableMoves, currentHover))
                    {
                        if(ContainsValidMove(ref attackMoves, currentHover))
                            tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Attack");
                        else if (ContainsValidMove(ref specialMoves, currentHover))
                            tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Special");
                        else
                            tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Highlight");
                    }
                    else
                        tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                    // Debug.Log("TODAY IS GONNA BE A GREAT DAY TODAY!!!");
                    // tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                    //tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref attackMoves, currentHover)) ? LayerMask.NameToLayer("Attack") : LayerMask.NameToLayer("Tile");
                    currentHover = hitPosition;
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                }

                // If we press down on the mouse
                if(Input.GetMouseButtonDown(0))
                {
                    if(chessPieces[hitPosition.x, hitPosition.y] != null)
                    {
                        // Is it our turn?
                        if((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn && currentTeam == 0) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn && currentTeam == 1))
                        {
                            currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];

                            // Get a list of where i can go,
                            availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                            // Get a list of special moves as well,
                            specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves, out specialMoves);
                            // Get a list of where i can kill,
                            attackMoves = currentlyDragging.GetAttackMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);

                            PreventCheck();

                            // Highlight tiles as well
                            HighLightTiles();
                            // Special tiles as well
                            SpecialTiles();
                            // Attack tiles as well
                            AttackTiles();

                        }
                    }
                }

                // If we are releasiing the mouse button
                if (currentlyDragging != null && Input.GetMouseButtonUp(0))
                {
                    Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                    if (ContainsValidMove(ref availableMoves, new Vector2Int(hitPosition.x, hitPosition.y)))
                    {
                        if (!firstMove)
                            firstMove = true;

                        MoveTo(previousPosition.x, previousPosition.y, hitPosition.x, hitPosition.y);


                        // Net implementation
                        NetMakeMove mm = new NetMakeMove();
                        mm.originalX = previousPosition.x;
                        mm.originalY = previousPosition.y;
                        mm.destinationX = hitPosition.x;
                        mm.destinationY = hitPosition.y;
                        mm.teamId = currentTeam;
                        Client.Instance.SendToServer(mm);

                        FindObjectOfType<AudioManager>().SelectRandomChessPieceClip();

                    }
                    else
                    {
                        currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                        currentlyDragging = null;
                        RemoveHighLightTiles();
                        RemoveSpecialTiles();
                        RemoveAttackTiles();
                    }
                }
            }
            else
            {
                if(currentHover != -Vector2Int.one)
                {
                    if (ContainsValidMove(ref availableMoves, currentHover))
                    {
                        if (ContainsValidMove(ref attackMoves, currentHover))
                            tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Attack");
                        else if (ContainsValidMove(ref specialMoves, currentHover))
                            tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Special");
                        else
                            tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Highlight");
                    }
                    else
                        tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                    // tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                    // tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref attackMoves, currentHover)) ? LayerMask.NameToLayer("Attack") : LayerMask.NameToLayer("Tile");
                    currentHover = -Vector2Int.one;
                }

                // If we are releasiing the mouse button
                if (currentlyDragging && Input.GetMouseButtonUp(0))
                {
                    currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                    currentlyDragging = null;
                    RemoveHighLightTiles();
                    RemoveSpecialTiles();
                    RemoveAttackTiles();
                
                }
            }

            // If we are dragging a piece
            if(currentlyDragging)
            {
                Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
                float distance = 0.0f;
                if(horizontalPlane.Raycast(ray, out distance))
                {
                    currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
                }
            }

        }
    }

    // Generate the board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = this.transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] triangles = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;


        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>().size = new Vector3(tileSize, 0.1f, tileSize);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return tileObject;
    }

    // Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0, blackTeam = 1;

        // White team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);

        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        }

        // Black team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);

        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        }
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];

        return cp;
    }

    // Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }
    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return (new Vector3(x * tileSize, yOffset*1.5f, y * tileSize) - bounds) + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    // Highlight Tiles
    private void HighLightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    private void RemoveHighLightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        availableMoves.Clear();
    }

    // Special Tiles
    private void SpecialTiles()
    {
        for (int i = 0; i < specialMoves.Count; i++)
        {
            for (int j = 0; j < availableMoves.Count; j++)
            {
                if (tiles[specialMoves[i].x, specialMoves[i].y] == tiles[availableMoves[j].x, availableMoves[j].y])
                    tiles[specialMoves[i].x, specialMoves[i].y].layer = LayerMask.NameToLayer("Special");
            }
        }
    }
    private void RemoveSpecialTiles()
    {
        for (int i = 0; i < specialMoves.Count; i++)
        {
            if (specialMoves[i].x != -1 && specialMoves[i].y != -1)
                tiles[specialMoves[i].x, specialMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        specialMoves.Clear();
        // tiles[specialMoves.x, specialMoves.y].layer = LayerMask.NameToLayer("Tile");
    }

    // Attack Tiles
    private void AttackTiles()
    {
        for (int i = 0; i < attackMoves.Count; i++)
        {
            for (int j = 0; j < availableMoves.Count; j++)
            {
                if (tiles[attackMoves[i].x, attackMoves[i].y] == tiles[availableMoves[j].x, availableMoves[j].y])
                    tiles[attackMoves[i].x, attackMoves[i].y].layer = LayerMask.NameToLayer("Attack");
            }
        }
    }
    private void RemoveAttackTiles()
    {
        for (int i = 0; i < attackMoves.Count; i++)
        {
            tiles[attackMoves[i].x, attackMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        attackMoves.Clear();
    }

    //CheckMate
    private void CheckMate(int team)
    {
        MenuUI.Instance.OnGameOver();
        DisplayVictory(team, false);
    }
    private void DisplayVictory(int winningTeam, bool isSurrender)
    {
        FindObjectOfType<AudioManager>().Play("GameOver");

        CheckScreen.SetActive(false);
        victoryScreen.SetActive(true);
        this.rematchButton.interactable = true;
        this.rematchIndicatorText.gameObject.SetActive(false);

        if (winningTeam == 0)
        {
            winnerText.SetText("White team wins.");
            if(isSurrender)
            {
                this.rematchIndicatorText.gameObject.SetActive(true);
                rematchIndicatorText.SetText("\t\t\t\t\t\t\t\t\t\t\t\t   by surrender.");
            }
        }
        else
        {
            winnerText.SetText("Black team wins.");
            if (isSurrender)
            {
                this.rematchIndicatorText.gameObject.SetActive(true);
                rematchIndicatorText.SetText("\t\t\t\t\t\t\t\t\t\t\t\t   by surrender.");
            }
        }
    }
    public void OnRematchButton()
    {
        if(localGame)
        {
            // White
            NetRematch wnr = new NetRematch();
            wnr.teamId = 0;
            wnr.wantRematch = 1;
            Client.Instance.SendToServer(wnr);

            // Black
            NetRematch bnr = new NetRematch();
            bnr.teamId = 1;
            bnr.wantRematch = 1;
            Client.Instance.SendToServer(bnr);
        }
        else
        {
            NetRematch nr = new NetRematch();
            nr.teamId = (currentTeam == 0) ? 1 : 0;
            nr.wantRematch = 1;
            Client.Instance.SendToServer(nr);
        }

        MenuUI.Instance.OnRematch();
        ResetTimers();
        surrenderedTeam = surrenderedTeam == 0 ? 1 : 0;
    }
    public void GameReset()
    {
        // UI
        victoryScreen.SetActive(false);

        // Fields Reset
        currentlyDragging = null;
        availableMoves.Clear();
        specialMoves.Clear();
        attackMoves.Clear();
        moveList.Clear();
        playerRematch[0] = playerRematch[1] = false;

        // Clean Up
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                    Destroy(chessPieces[x, y].gameObject);

                chessPieces[x, y] = null;
            }
        }

        for (int i = 0; i < deadWhites.Count; i++)
            Destroy(deadWhites[i].gameObject);
        for (int i = 0; i < deadBlacks.Count; i++)
            Destroy(deadBlacks[i].gameObject);

        deadWhites.Clear();
        deadBlacks.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;

    }
    public void OnMenuButton()
    {
        NetRematch nr = new NetRematch();
        nr.teamId = currentTeam;
        nr.wantRematch = 0;
        Client.Instance.SendToServer(nr);

        GameReset();
        MenuUI.Instance.OnLeaveFromGameMenu();

        Invoke("ShutDownRelay", 1.0f);

        // Reset some values
        playerCount = -1;
        currentTeam = -1;
    }
    public void OnSurrenderButton()
    {
        Server.Instance.BroadCast(new NetEndGame());
    }

    // Special Moves
    private void ProcessSpecialMove()
    {
        if(specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            ChessPiece myPawn = chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if(myPawn.currentX == enemyPawn.currentX)
            {
                if(myPawn.currentY == enemyPawn.currentY -1 || myPawn.currentY == enemyPawn.currentY + 1)
                {
                    if(enemyPawn.team == 0)
                    {
                        deadWhites.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, -4f * yOffset, -1 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.forward * deathSpacing) * deadWhites.Count);
                    }
                    else
                    {
                        deadBlacks.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(new Vector3(-1.1f * tileSize, -4f * yOffset, 7.8f * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.back * deathSpacing) * deadBlacks.Count);
                    }
                    chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            }
        }

        if(specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if(targetPawn.type == ChessPieceType.Pawn)
            {
                if(targetPawn.team == 0 && lastMove[1].y == 7)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
                
                if (targetPawn.team == 1 && lastMove[1].y == 0)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }

        if(specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            // Left Rook
            if (lastMove[1].x == 2)
            {
                if(lastMove[1].y == 0) // White Side
                {
                    ChessPiece rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null;

                }
                else if(lastMove[1].y == 7) // Black Side
                {
                    ChessPiece rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }
            // Right Rook
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0) // White Side
                {
                    ChessPiece rook = chessPieces[7, 0];
                    chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null;

                }
                else if (lastMove[1].y == 7) // Black Side
                {
                    ChessPiece rook = chessPieces[7, 7];
                    chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
    }
    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                    if (chessPieces[x,y].type == ChessPieceType.King)
                        if(chessPieces[x,y].team == currentlyDragging.team)
                            targetKing = chessPieces[x, y];

        // Since we are sending ref availableMoves, we will deleting moves that are putting us in check
        SimulateMoveForSinglePiece(currentlyDragging, ref availableMoves, targetKing);
    }
    private void SimulateMoveForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        // Save the current values, to reset after the function call
        int actualX = cp.currentX; 
        int actualY = cp.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        // Going through all the moves, simulate them and check if we are check
        for (int i = 0; i < moves.Count; i++)
        {
            int simX = moves[i].x;
            int simY = moves[i].y;

            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);
            // Did we simulate the king's move
            if (cp.type == ChessPieceType.King)
                kingPositionThisSim = new Vector2Int(simX, simY);

            // Copy the [,] and not a reference
            ChessPiece[,] simulation = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
            List<ChessPiece> simAttackingPieces = new List<ChessPiece>();
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                for (int y = 0; y < TILE_COUNT_Y; y++)
                {
                    if(chessPieces[x,y] != null)
                    {
                        simulation[x, y] = chessPieces[x, y];
                        if(simulation[x,y].team != cp.team)
                        {
                            simAttackingPieces.Add(simulation[x, y]);
                        }
                    }
                }
            }

            // Simulate that move
            simulation[actualX, actualY] = null;
            cp.currentX = simX;
            cp.currentY = simY;
            simulation[simX, simY] = cp;

            // Did one of the piece got taken down  during our simulation
            var deadPiece = simAttackingPieces.Find(c => c.currentX == simX && c.currentY == simY);
            if(deadPiece != null)
            {
                simAttackingPieces.Remove(deadPiece);
            }

            // Get all the simulated attacking pieces moves
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int a = 0; a < simAttackingPieces.Count; a++)
            {
                var pieceMoves = simAttackingPieces[a].GetAvailableMoves(ref simulation, TILE_COUNT_X, TILE_COUNT_Y);
                for (int b = 0; b < pieceMoves.Count; b++)
                {
                    simMoves.Add(pieceMoves[b]);
                }
            }

            // Is the king in trouble? If so, remove the move
            if (ContainsValidMove(ref simMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            // Restore the actual CP data
            cp.currentX = actualX;
            cp.currentY = actualY;
        }

        // Remove from the available move list
        for (int i = 0; i < movesToRemove.Count; i++)
            moves.Remove(movesToRemove[i]);
    }
    private bool CheckForCheckmate()
    {
        CheckScreen.SetActive(false);
        var lastMove = moveList[moveList.Count - 1];
        int targetTeam = (chessPieces[lastMove[1].x, lastMove[1].y].team == 0) ? 1 : 0;

        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        List<ChessPiece> defendingPieces = new List<ChessPiece>();
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].team == targetTeam)
                    {
                        defendingPieces.Add(chessPieces[x, y]);
                        if (chessPieces[x, y].type == ChessPieceType.King)
                            targetKing = chessPieces[x, y];
                    }
                    else
                    {
                        attackingPieces.Add(chessPieces[x, y]);
                    }
                }

        // Is the king attacked right now?
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves = attackingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            for (int j = 0; j < pieceMoves.Count; j++)
            {
                currentAvailableMoves.Add(pieceMoves[j]);
            }
        }

        // Are we in check right now?
        if(ContainsValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX,targetKing.currentY)))
        {
            // HERE

            int defender = defendingPieces[0].team;

            if (defender == 1)
                blackCharacter.GetComponent<ChoosePlayer>().OnCheckMade(false);
            else if (defender == 0)
                whiteCharacter.GetComponent<ChoosePlayer>().OnCheckMade(true);


            // Debug.Log("KING UNDER ATTACK");
            CheckScreen.SetActive(true);


            // King is under attack, can we move something to help him?
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                // Since we are sending ref defendingMoves, we will deleting moves that are putting us in checkmate
                SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);
                if (defendingMoves.Count != 0)
                    return false;
            }
            CheckScreen.SetActive(false);
            return true; // Checkmate exit...
        }
        return false;
    }

    // Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;
        return false;
    }
    private void MoveTo(int originalX, int originalY, int x, int y)
    {
        ChessPiece cp = chessPieces[originalX, originalY];
        Vector2Int previousPosition = new Vector2Int(originalX, originalY);

        // Is there another piece on the target position
        if(chessPieces[x,y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];

            // If its the same team
            if (cp.team == ocp.team)
            {
                return;
            }

            // If its the enemy team
            if (ocp.team == 0)
            {

                if (ocp.type == ChessPieceType.King)
                {
                    CheckMate(1);
                }

                whiteCharacter.GetComponent<ChoosePlayer>().OnPieceEaten(true);

                deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(new Vector3(8 * tileSize, -4f * yOffset, -1 * tileSize) 
                    - bounds
                    + new Vector3(tileSize/2, 0, tileSize/2)
                    +(Vector3.forward * deathSpacing) * deadWhites.Count);
            }
            else
            {
                if (ocp.type == ChessPieceType.King)
                {
                    CheckMate(0);
                }

                blackCharacter.GetComponent<ChoosePlayer>().OnPieceEaten(false);

                deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(new Vector3(-1.1f * tileSize, -4f * yOffset, 7.8f * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.back * deathSpacing) * deadBlacks.Count);
            }

        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        isWhiteTurn = !isWhiteTurn;
        // if (localGame && !CheckForCheckmate())
        // {
        //     currentTeam = (currentTeam == 0) ? 1 : 0;
        //     MenuUI.Instance.ChangeCamera((currentTeam == 0) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
        // }
        moveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });

        ProcessSpecialMove();

        if(currentlyDragging)
            currentlyDragging = null;
        RemoveHighLightTiles();
        RemoveSpecialTiles();
        RemoveAttackTiles();

        if (localGame && !CheckForCheckmate())
        {
            currentTeam = (currentTeam == 0) ? 1 : 0;
            MenuUI.Instance.ChangeCamera((currentTeam == 0) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);

        }
        if (CheckForCheckmate())
            CheckMate(cp.team);


        return;
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(tiles[x,y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one; // InValid
    }

    #region
    public void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;
        NetUtility.S_REMATCH += OnRematchServer;

        NetUtility.S_SURRENDER += OnSurrenderServer;
        NetUtility.S_TIMER += OnTimerServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_END_GAME += OnEndGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_REMATCH += OnRematchClient;

        NetUtility.C_SURRENDER += OnSurrenderClient;
        NetUtility.C_TIMER += OnTimerClient;

        MenuUI.Instance.SetLocalGame += OnSetLocalGame;
    }
    public void UnRegisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;
        NetUtility.S_REMATCH -= OnRematchServer;
        
        NetUtility.S_SURRENDER -= OnSurrenderServer;
        NetUtility.S_TIMER -= OnTimerServer;

        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_END_GAME -= OnEndGameClient;
        NetUtility.C_MAKE_MOVE -= OnMakeMoveClient;
        NetUtility.C_REMATCH -= OnRematchClient;
        
        NetUtility.C_SURRENDER -= OnSurrenderClient;
        NetUtility.C_TIMER -= OnTimerClient;

        MenuUI.Instance.SetLocalGame -= OnSetLocalGame;
    }

    // Server
    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        // Client has connected, assign a team and return the message back to him
        NetWelcome nw = msg as NetWelcome;

        // Assign a team
        ++playerCount;
        if (playerCount == 0)
        {
            GetRandomTeam();
            nw.AssignedTeam = random_team;
            // Debug.Log($"Player1 assigned team is {nw.AssignedTeam}");
        }

        if (playerCount == 1)
        {
            nw.AssignedTeam = (random_team == 0) ? 1 : 0;
            // Debug.Log($"Player2 assigned team is {nw.AssignedTeam}");
        }

        // Return back to the client
        Server.Instance.SendToClient(cnn, nw);

        // If full, start the game
        if(playerCount == 1)
            Server.Instance.BroadCast(new NetStartGame());
    }
    private void OnMakeMoveServer(NetMessage msg, NetworkConnection cnn)
    {
        // Receive, and just broadcast it back
        NetMakeMove nmm = msg as NetMakeMove;

        // This is where you could do some validation checks
        // --

        // Receive, and just broadcast it back
        Server.Instance.BroadCast(nmm);

    }
    private void OnRematchServer(NetMessage msg, NetworkConnection cnn)
    {
        Server.Instance.BroadCast(msg);
    }
    private void OnSurrenderServer(NetMessage msg, NetworkConnection cnn)
    {
        Server.Instance.BroadCast(msg);
    }
    private void OnTimerServer(NetMessage msg, NetworkConnection cnn)
    {
        Server.Instance.BroadCast(msg);
    }

    // Client
    private void OnWelcomeClient(NetMessage msg)
    {
        // Receive the connection message
        NetWelcome nw = msg as NetWelcome;

        // Assign a team
        currentTeam = nw.AssignedTeam;
        initialAssignedTeam = nw.AssignedTeam;
        // Debug.Log($"My assigned team is {nw.AssignedTeam}");

        if (localGame)
        {
            currentTeam = 0;
            Server.Instance.BroadCast(new NetStartGame());
        }
    }
    private void OnStartGameClient(NetMessage msg)
    {
        // We just need to change the camera
        MenuUI.Instance.ChangeCamera((currentTeam == 0) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);

        if (MenuUI.Instance.getIsHost())
        {
            // Server.Instance.timer = MenuUI.Instance.getTimeValue() * 60;
            // SetTimerInSeconds(MenuUI.Instance.getTimeValue() * 60);
        }
        
        timerInSeconds = Server.Instance.GetTimer();
        ResetTimers();
    }
    private void OnEndGameClient(NetMessage msg)
    {
        FindObjectOfType<GameManager>().OnResumeButtonClicked();

        Debug.Log("SURRENDER: " + surrenderedTeam);

        MenuUI.Instance.OnGameOver();
        if (localGame)
            DisplayVictory(currentTeam == 0 ? 1 : 0, true);
        else
            DisplayVictory(surrenderedTeam == 0 ? 1 : 0, true);

    }
    private void OnMakeMoveClient(NetMessage msg)
    {
        // Receive the message
        NetMakeMove nmm = msg as NetMakeMove;

        // Debug.Log($"MM : {nmm.teamId} : {nmm.originalX} {nmm.originalY} -> {nmm.destinationX} {nmm.destinationY}");

        if(nmm.teamId != currentTeam)
        {
            ChessPiece target = chessPieces[nmm.originalX, nmm.originalY];

            availableMoves = target.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            specialMove = target.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves, out specialMoves);
            attackMoves = target.GetAttackMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);

            MoveTo(nmm.originalX, nmm.originalY, nmm.destinationX, nmm.destinationY);
        }
    }
    private void OnRematchClient(NetMessage msg)
    {
        // Receive the connection message
        NetRematch nr = msg as NetRematch;

        // Set the boolean for rematch
        playerRematch[nr.teamId] = nr.wantRematch == 1;

        // Activate the piece of UI
        if (nr.teamId == currentTeam)
        {
            // Debug.Log("ENTERS HERE...Rematch: " + nr.wantRematch);
            if (nr.wantRematch == 1)
            {
                rematchIndicatorText.gameObject.SetActive(true);

                rematchIndicatorText.SetText("Opponent wants a rematch");
                rematchIndicatorText.color = Color.green;
            }
        }

        if (nr.teamId != currentTeam)
        {
            // Debug.Log("ENTERS HERE...Rematch: " + nr.wantRematch);
            if (nr.wantRematch != 1)
            {
                rematchIndicatorText.gameObject.SetActive(true);

                rematchButton.interactable = false;
                rematchIndicatorText.SetText("Opponent left the game");
                rematchIndicatorText.color = Color.red;
            }
        }

        // If both wants to rematch
        if (playerRematch[0] && playerRematch[1])
        {
            if(!localGame)
            {
                currentTeam = (currentTeam == 0) ? 1 : 0;
                GameReset();
                MenuUI.Instance.ChangeCamera((currentTeam == 0) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
            }
            else
            {
                currentTeam = 0;
                GameReset();
                MenuUI.Instance.ChangeCamera((currentTeam == 0) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
            }
        }
    }
    private void OnSurrenderClient(NetMessage msg)
    {
        // Receive the connection message
        NetSurrender ns = msg as NetSurrender;

        // Set the boolean for surrender
        playerSurrender[ns.teamId] = ns.wantSurrender == 1;
        // surrenderedTeam = ns.teamId;
        surrenderedTeam = initialAssignedTeam;
    }
    private void OnTimerClient(NetMessage msg)
    {
        // Receive the message
        NetTimer nt = msg as NetTimer;

        if(nt.teamId == 1)
        {
            float minutes = Mathf.FloorToInt(nt.timer / 60);
            float seconds = Mathf.FloorToInt(nt.timer % 60);

            whiteTimerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");

            if (nt.timer <= 0)
                CheckMate(1);
        }

        if (nt.teamId == 0)
        {
            float minutes = Mathf.FloorToInt(nt.timer / 60);
            float seconds = Mathf.FloorToInt(nt.timer % 60);

            blackTimerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");

            if (nt.timer <= 0)
                CheckMate(0);
        }
    }

    // Shut Down
    private void ShutDownRelay()
    {
        Client.Instance.ShutDown();
        Server.Instance.ShutDown();
    }

    // Local Game
    private void OnSetLocalGame(bool obj)
    {
        playerCount = -1;
        currentTeam = -1;
        localGame = obj;
    }
    #endregion

    // Getters - Setters
    public void SetTimerInSeconds(float tis)
    {
        this.timerInSeconds = tis;
    }
}