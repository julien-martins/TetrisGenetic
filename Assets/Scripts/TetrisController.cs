using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Board))]
public class TetrisController : MonoBehaviour
{
    private bool dead = false;
    private bool is_best = false;

    [SerializeField] private Board board;

    private NeuralNetwork[] _populations;

    private bool movesInProgress;
    
    private int _indexMove = 0;
    private List<int> _moves = new ();
    
    void Update()
    {
        if (board.canMove)
        {
            _moves = FindBestMove();
            board.canMove = false;
            _indexMove = 0;
        }

        if (_indexMove < _moves.Count)
        {
            foreach (var move in _moves)
            {
                board.Move(move);
                _indexMove++;
            }
        }

    }
    
    private List<int> FindBestMove()
    {
        List<List<int>> moves = new();

        // Test all differrent combinaison of rotation and postion of the tetromino
        for (int x_add = 5; x_add >= -5; x_add--)
        {
            for (int rotate = 0; rotate <= 3; rotate++)
            {
                List<int> moveList = new();
                Vector2Int tmpPiecePos = board.GetCurrentPiecePos();

                for (int r = 0; r < rotate; r++)
                {
                    moveList.Add(2);
                }

                for (int x = 0; x < Math.Abs(x_add); x++)
                {
                    if(x_add < 0) moveList.Add(0);
                    else moveList.Add(1);
                }
                
                moves.Add(moveList);
            }
        }
        
        
        return moves[Random.Range(0, moves.Count)];
    }
    
    //Calculate score on the board
    private float CalculateFitness()
    {
        return board.score;
    }
    
}
