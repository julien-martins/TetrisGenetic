using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class Board : MonoBehaviour
{
    public const int Width = 10;
    public const int Height = 19;
    
    [SerializeField] private TileBase[] tilesBase;

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tilemap pieceTilemap;

    private Vector2Int _prevPos;
    private Vector2Int _piecePos;
    private Piece _piece;

    private const float Delay = 0.25f;
    private float _timer = 0.0f; 
    
    // Start is called before the first frame update
    void Start()
    {
        _piece = new Piece();

        _piecePos = new Vector2Int(5, 16);
    }

    // Update is called once per frame
    void Update()
    {
        _prevPos = _piecePos;
        
        _timer += Time.deltaTime;
        if (_timer >= Delay)
        {
            _piecePos.y -= 1;
            _timer = 0;
        }
        
        pieceTilemap.ClearAllTiles();

        CheckCollision(_piece);
        
        HandleInput();

        DrawPiece(_piecePos, _piece);

    }

    void HandleInput()
    {
        // MOVE RIGHT
        if (Input.GetKeyDown(KeyCode.D))
        {
            _piecePos.x += 1;
            if (CheckCollision(_piece)) _piecePos.x -= 1;
        }

        //MOVE LEFT
        if (Input.GetKeyDown(KeyCode.A))
        {
            _piecePos.x -= 1;
            if (CheckCollision(_piece)) _piecePos.x += 1;
        }
        
        //ROTATE RIGHT
        if (Input.GetKeyDown(KeyCode.W))
        {
            _piece.Rotate();
            if(CheckCollision(_piece)) _piece.Rotate(false);
        }
        
        //ROTATE LEFT
        if (Input.GetKeyDown(KeyCode.S))
        {
            _piece.Rotate(false);
            if(CheckCollision(_piece)) _piece.Rotate();
        }
    }

    
    bool CheckCollision(Piece piece)
    {
        for (int j = 0; j < 4; ++j)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (piece.GetTiles()[i, j] != 1) continue;
                
                var newPos = _piecePos + new Vector2Int(i, j);
                
                if (newPos.x < 0 || newPos.x > Width-1) return true;

                if (newPos.y <= 0 || tilemap.HasTile(new Vector3Int(newPos.x, newPos.y - 1, 0)) )
                {
                    CopyPieceOnBoard(_piece);
                    
                    ClearPiece(_prevPos, piece);
                    ClearPiece(_piecePos, piece);
                    ResetPiece();

                    TestLineClearing();
                    
                    return true;
                }
                
            }
        }
        
        
        return false;
    }

    void TestLineClearing()
    {
        List<int> linesClear = new();
        int nbLineFull = 0;
        int max_j = 0;
        int min_j = 40;
        
        for (int j = 0; j < Height; ++j)
        {
            bool fullLine = true;
            for (int i = 0; i < Width; ++i)
            {
                if (!tilemap.HasTile(new Vector3Int(i, j)))
                {
                    fullLine = false;
                    break;
                }
            }

            if (fullLine)
            {
                nbLineFull++;
                if (j > max_j) max_j = j;
                if (j < min_j) min_j = j;
                linesClear.Add(j);  
            }
            
        }
        
        //Clear line full
        foreach (int j in linesClear)
        {
            for (int i = 0; i < Width; ++i)
            {
                tilemap.SetTile(new Vector3Int(i, j), null);
            }
        }

        if (linesClear.Count > 0)
        {

            //Descent the rest of block
            for (int j = max_j + 1; j < Height; ++j)
            {
                for (int i = 0; i < Width; ++i)
                {
                    var tile = tilemap.GetTile(new Vector3Int(i, j));
                    
                    tilemap.SetTile(new Vector3Int(i, j - linesClear.Count), tile);

                    tilemap.SetTile(new Vector3Int(i, j), null);
                }
            }
        }
    }
    
    void CopyPieceOnBoard(Piece piece)
    {
        var tiles = piece.GetTiles();

        for (int j = 0; j < 4; ++j)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (tiles[i, j] != 1) continue;
                
                var newCoord = _piecePos + new Vector2Int(i, j);
                DrawTileOnBoard(newCoord, tilesBase[0]);
            }
        }
    }

    void ResetPiece()
    {
        _piece = new Piece();
        _piecePos = new Vector2Int(5, 16);
        _prevPos = _piecePos;
        _timer = 0;
    }

    void ClearPiece(Vector2Int coord, Piece piece)
    {
        var tiles = piece.GetTiles();

        for (int j = 0; j < 4; ++j)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (tiles[i, j] != 1) continue;
                
                var newCoord = coord + new Vector2Int(i, j);
                pieceTilemap.SetTile(new Vector3Int(newCoord.x, newCoord.y), null);
            }
        }
        
    }
    
    void DrawPiece(Vector2Int coord, Piece piece)
    {
        var tiles = piece.GetTiles();

        for (int j = 0; j < 4; ++j)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (tiles[i, j] != 1) continue;
                
                var newCoord = coord + new Vector2Int(i, j);
                pieceTilemap.SetTile(new Vector3Int(newCoord.x, newCoord.y), tilesBase[0]);
            }
        }
        
    }

    void DrawTileOnBoard(Vector2Int coord, TileBase tile)
    {
        tilemap.SetTile(new Vector3Int(coord.x, coord.y, 0), tile);
    }
}
