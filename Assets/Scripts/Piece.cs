using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{

    enum TETRO_TYPE
    {
        L,
        I,
        O,
        S,
        Z,
        T
    }

    private int[,] tiles = new int[4, 4];
    private TETRO_TYPE type;
    
    public Piece()
    {
        type = TETRO_TYPE.L;
        generatePiece();
    }

    private void generatePiece()
    {
        switch (type)
        {
            case TETRO_TYPE.L:
                tiles[0, 0] = 0; tiles[1, 0] = 0; tiles[2, 0] = 0; tiles[3, 0] = 0;
                tiles[0, 1] = 0; tiles[1, 1] = 0; tiles[2, 1] = 0; tiles[3, 1] = 0;
                tiles[0, 2] = 0; tiles[1, 2] = 0; tiles[2, 2] = 0; tiles[3, 2] = 0;
                tiles[0, 3] = 0; tiles[1, 3] = 0; tiles[2, 3] = 0; tiles[3, 3] = 0;
                
                break;
            default:
                break;
        }
    }
    
}
