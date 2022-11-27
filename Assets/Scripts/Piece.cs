using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece
{
    public enum TetroType
    {
        L = 0,
        I,
        O,
        S,
        Z,
        T
    }

    private int[,] tiles = new int[4, 4];
    private TetroType type;

    private int _rotationIndex;

    public Piece(TetroType type)
    {
        type = TetroType.L;
        _rotationIndex = 0;

        GeneratePiece();
    }

    private void GeneratePiece()
    {
        for (int j = 0; j < 4; ++j)
        {
            for (int i = 0; i < 4; ++i)
            {
                tiles[i, j] = Tetro_Data.T_Data[(int)type, _rotationIndex, i, j];
            }
        }
    }

    public void Rotate(bool clockwise = true)
    {
        var dir = (clockwise) ? 1 : -1;

        _rotationIndex += dir;

        if (_rotationIndex > 3) _rotationIndex = 0;
        else if (_rotationIndex < 0) _rotationIndex = 3;

        GeneratePiece();
    }
    
    public int[,] GetTiles() => tiles;

}
