using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Board))]
public class TetrisController : MonoBehaviour
{
    private bool dead = false;
    private bool is_best = false;

    [SerializeField] private Board board;

    private NeuralNetwork _network;

    private bool movesInProgress;
    
    private int _indexMove = 0;
    private List<int> _moves = new ();

    public int LAYERS = 1;
    public int NEURONS = 3;
    
    void Start()
    {
        _network = new NeuralNetwork();
        _network.Initialize(LAYERS, NEURONS);
    }
    
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

        if (board.IsGameOver())
        {
            Debug.Log("IS Game OVER");
            Death();
        }
    }

    public void ResetWithNetwork(NeuralNetwork net)
    {
        _network = net;
        board.ResetPiece();
    }

    private void Death()
    {
        GameObject.FindObjectOfType<GeneticAlgorithm>().Death(board.GetScore(), _network);
        Reset();
    }

    public void Reset()
    {
        board.Reset();
    }
    
    public float moveScore(int terrainHeight, int coutHoles, int clearLines)
    {
        return _network.RunNetwork(terrainHeight, coutHoles, clearLines);
    }
    
    private List<int> FindBestMove()
    {
        List<List<int>> moves = new();
        List<float> moveScores = new();

        //Copy the board to make all temporary move to calculate the score of the move
        Board boardCopy = board.Copy();
        
        // Test all differrent combinaison of rotation and postion of the tetromino
        for (int x_add = 5; x_add >= -5; x_add--)
        {
            for (int rotate = 0; rotate <= 3; rotate++)
            {
                List<int> moveList = new();
                Vector2Int tmpPiecePos = boardCopy.GetCurrentPiecePos();

                for (int r = 0; r < rotate; r++)
                {
                    moveList.Add(2);
                    boardCopy.Move(2);
                }

                for (int x = 0; x < Math.Abs(x_add); x++)
                {
                    if (x_add < 0)
                    {
                        moveList.Add(0);
                        boardCopy.Move(0);
                    }
                    else
                    {
                        moveList.Add(1);
                        boardCopy.Move(1);
                    }
                }
                
                //Calculate the score of the move
                var terrainHeight= boardCopy.CalculateTerrainHeight();
                var countHoles= boardCopy.CountHoles();
                var clearLines= boardCopy.ClearLines().Count;

                float score = moveScore(terrainHeight, countHoles, clearLines);
                
                moveScores.Add(score);
                moves.Add(moveList);
            }
        }
        
        //Get the move with the maximum score
        int max_index = 0;
        float max_score = moveScores[0];
        for (int i = 1; i < moveScores.Count; i++)
        {
            if (moveScores[i] > max_score)
            {
                max_score = moveScores[i];
                max_index = i;
            }
        }
        
        return moves[max_index];
    }
    
    //Calculate score on the board
    private float CalculateFitness()
    {
        return board.score;
    }
    
}
