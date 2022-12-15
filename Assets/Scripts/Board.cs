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

    public Tilemap tilemap;
    public Tilemap pieceTilemap;

    private Vector2Int _prevPos;
    private Vector2Int _piecePos;
    private Piece _piece;

    private float _hightScore = 0.0f;
    public float score = 0.0f;
    
    private const float Delay = 0.25f;
    private float _timer = 0.0f;

    public bool canMove = true;

    private bool _gameOver = false;
    public bool isCopy = false;

    public Board Copy()
    {
        Board board = Instantiate(this);
        
        //Move the copyBoard next to the real booard to debug
        //board.transform.position += new Vector3(10, 0, 0); 
        
        board._piece = _piece.Copy();
        board._piecePos = _piecePos;
        board.isCopy = true;
        
        board.tilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
        board.pieceTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
        board.tilesBase = tilesBase;
        
        return board;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _piecePos = new Vector2Int(5, 16);
        
        //Debug.Log("TEST ISROWFULL");
        //Debug.Log(TestIsRowFull());
        
        //Debug.Log("Count hole");
        //Debug.Log(TestCountHole());
        
        //Debug.Log("GET COL HEIGHT");
        //Debug.Log(TestGetColHeight());
    }

    // Update is called once per frame
    void Update()
    {
        //return;
        
        _prevPos = _piecePos;
        
        
        _timer += Time.deltaTime;
        if (_timer >= Delay)
        {
            _piecePos.y -= 1;
            _timer = 0;
        }
        
        
        pieceTilemap.ClearAllTiles();

        
        if (!isCopy)
        {
            CheckCollision(_piece);
            HandleInput();
        } 
        

        DrawPiece(_piecePos, _piece);

    }

    public bool IsGameOver()
    {
        return _gameOver;
    }
    
    public void Reset()
    {
        pieceTilemap.ClearAllTiles();
        tilemap.ClearAllTiles();
        ResetPiece();
        _gameOver = false;
        
        if (score > _hightScore)
            _hightScore = score;

        score = 0;
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

    //Use by the Tetris controller
    public bool Move(int dir)
    {
        _prevPos = _piecePos;
        
        switch (dir)
        {
            case 0:
                _piecePos.x += 1;
                if(CheckCollisionPiece(_piece)){
                    _piecePos.x -= 1;
                    return true;
                }
                break;
            case 1:
                _piecePos.x -= 1;
                if(CheckCollisionPiece(_piece)){
                    _piecePos.x += 1;
                    return true;
                }
                break;
            case 2:
                _piece.Rotate();
                
                if(CheckCollisionPiece(_piece)){
                    _piece.Rotate(false);
                    return true;
                }

                break;
            case 3:
                _piece.Rotate(false);
                break;
            case 4:
                _piecePos.y -= 1;

                if (CheckCollisionIA(_piece))
                {
                    _piecePos.y += 1;
                    return true;
                }

                break;
        }
        
        return false;
    }

    public bool CheckCollisionPiece(Piece piece)
    {
        for (int j = 0; j < 4; ++j)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (piece.GetTiles()[i, j] != 1) continue;

                var newPos = _piecePos + new Vector2Int(i, j);

                if (newPos.x < 0 || newPos.x > Width - 1) return true;
            }
        }

        return false;
    }
    public bool CheckCollision(Piece piece)
    {
        for (int j = 0; j < 4; ++j)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (piece.GetTiles()[i, j] != 1) continue;
             
                var newPos = _piecePos + new Vector2Int(i, j);
                
                //Test if the piece spawn on block
                if (tilemap.HasTile(new Vector3Int(newPos.x, newPos.y)))
                {
                    _gameOver = true;
                    return false;
                }
                
                if (newPos.x < 0 || newPos.x > Width-1) return true;

                if (newPos.y >= Height+3) {
                    _gameOver = true;
                    return false;
                }
                
                if (newPos.y <= 0 || tilemap.HasTile(new Vector3Int(newPos.x, newPos.y - 1, 0)) )
                {
                    CopyPieceOnBoard(_piece);
                    //score += 20;
                    
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

    public bool CheckCollisionIA(Piece piece)
    {
        for (int j = 0; j < 4; ++j)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (piece.GetTiles()[i, j] != 1) continue;
                var newPos = _piecePos + new Vector2Int(i, j);
                
                //if piece spawn on block
                if (tilemap.HasTile(new Vector3Int(newPos.x, newPos.y))) return true;
                //if piece is out of the terrain
                if (newPos.x < 0 || newPos.x > Width-1) return true;
                
                //If the piece out of the terrain
                if (newPos.y >= Height + 3) return true;
                
                //IF the piece touch the botton of the grid or an another piece
                if (newPos.y <= 0 || tilemap.HasTile(new Vector3Int(newPos.x, newPos.y - 1, 0)))
                {
                    CopyPieceOnBoard(_piece);
                    
                    ClearPiece(_prevPos, piece);
                    ClearPiece(_piecePos, piece);
                    
                    return true;
                }

            }
        }

        return false;
    }
    
    public void TestLineClearing()
    {
        List<int> linesClear = new();
        int max_j = 0;
        int min_j = 40;
        int nbLineFull = 0;
        
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
        if (linesClear.Count == 4) score += 600;
        else if (linesClear.Count > 0) score += linesClear.Count * 100;
        
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
                DrawTileOnBoard(newCoord, tilesBase[_piece.GetType()]);
            }
        }
    }

    public void ResetPiece()
    {
        _piece = new Piece();
        _piecePos = new Vector2Int(5, 16);
        _prevPos = _piecePos;
        _timer = 0;
        canMove = true;
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
                pieceTilemap.SetTile(new Vector3Int(newCoord.x, newCoord.y), tilesBase[_piece.GetType()]);
            }
        }
        
    }

    void DrawTileOnBoard(Vector2Int coord, TileBase tile)
    {
        tilemap.SetTile(new Vector3Int(coord.x, coord.y), tile);
    }

    public float GetScore() => score;
    public float GetHightScore() => _hightScore;
    
    public Piece GetCurrentPiece() => _piece;
    public Vector2Int GetCurrentPiecePos() => _piecePos;

    //All functions needs to the genetic algorithm to find the best move
    public int GetMinHeight()
    {
        int min = Height;
        for (int i = 0; i < Width; i++)
        {
            int h = GetColHeight(i);
            if (h < min)
            {
                min = h;
            }
        }
        
        return min;
    }

    public int GetMaxHeight()
    {
        int max = 0;
        for (int i = 0; i < Width; i++)
        {
            int h = GetColHeight(i);
            if (h > max)
            {
                max = h;
            }
        }
        
        return max;
    }

    public List<int> ClearLines()
    {
        List<int> linesClear = new();
        
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
                linesClear.Add(j);
            }
            
        }

        return linesClear;
    }
    public int CountHoles()
    {
        int holes = 0;

        for (int x = 0; x < Width; x++)
        {
            var hitBlock = false;
            for (int y = Height-1; y >= 0; y--)
            {
                if (tilemap.HasTile(new Vector3Int(x, y))) hitBlock = true;

                if (hitBlock && !tilemap.HasTile(new Vector3Int(x, y))) holes++;
            }
        }
        
        return holes;
    }

    public bool IsRowFull(int y)
    {
        var full = true;

        for (int i = 0; i < Width; i++)
        {
            if (!tilemap.HasTile(new Vector3Int(i, y)))
                full = false;
        }
        
        return full;
    }
    
    public int GetColHeight(int c)
    {
        int h = 0;

        for (int j = Height; j > 0; j--)
        {
            var full = IsRowFull(j);
            if (tilemap.HasTile(new Vector3Int(c, j)) && h == 0 && !full)
                h = j;

            if (h > 0 && full) h--;
        }
        
        return h;
    }
    public int CalculateTerrainHeight()
    {
        int bumpiness = 0;
        int prevHeight = GetColHeight(0);
        for(int i = 1; i < Width; i++)
        {
            int h = GetColHeight(i);
            bumpiness += Math.Abs(h - prevHeight);
            
            prevHeight = h;
        }
      
        return bumpiness;
    }
    
    //Test Function
    public bool TestIsRowFull()
    {
        for(int i = 0; i < Width-1; i++)
            tilemap.SetTile(new Vector3Int(i, 0), tilesBase[0]);
        
        return IsRowFull(0);
    }

    public int TestCountHole()
    {
        for(int i = 0; i < Width-2; i++)
            tilemap.SetTile(new Vector3Int(i, 0), tilesBase[0]);
        tilemap.SetTile(new Vector3Int(Width-2, 1), tilesBase[0]);
        tilemap.SetTile(new Vector3Int(Width-2, 3), tilesBase[0]);
        tilemap.SetTile(new Vector3Int(Width-1, 3), tilesBase[0]);
        
        return CountHoles();
    }

    public int TestGetColHeight()
    {
        for(int j = 0; j < 10; j++)
            tilemap.SetTile(new Vector3Int(0, j), tilesBase[0]);

        return GetColHeight(0);
    }

}
