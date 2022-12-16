using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

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

    private Board boardCopy;
    
    [Header("UI")]
    public TextMeshProUGUI scoreNumber;
    public TextMeshProUGUI hightScoreNumber;
    
    void Start()
    {
        _network = new NeuralNetwork();
        _network.Initialize(LAYERS, NEURONS);
    }
    
    void Update()
    {  
        scoreNumber.text = board.GetScore().ToString();
        hightScoreNumber.text = board.GetHightScore().ToString();
        
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
        //Debug.Log(board.GetScore());
        GameObject.FindObjectOfType<GeneticAlgorithm>().Death(board.GetScore(), _network);
        Reset();
    }

    public void Reset()
    {
        board.Reset();
    }
    
    private float MoveScore(int terrainHeight, int coutHoles, int clearLines, int aggregateHeight)
    {
        return _network.RunNetwork(terrainHeight, coutHoles, clearLines, aggregateHeight);
    }
    
    private List<int> FindBestMove()
    {
        List<List<int>> moves = new();
        List<float> moveScores = new();


        // Test all differrent combinaison of rotation and postion of the tetromino
        int it_combin = 1;
        for (int x_add = 6; x_add >= -6; x_add--)
        {
            for (int rotate = 0; rotate <= 3; rotate++)
            {
                //Comment this to see all copy and the all different postition
                if (boardCopy)
                {
                    DestroyImmediate(boardCopy.gameObject);
                }

                //Copy the board to make all temporary move to calculate the score of the move
                boardCopy = board.Copy();
                boardCopy.gameObject.name = "Combinaison " + it_combin;
                
                List<int> moveList = new();

                for (int r = 0; r < rotate; r++)
                {
                    if (!boardCopy.Move(2))
                        moveList.Add(2);
                }

                for (int x = 0; x < Math.Abs(x_add); x++)
                {
                    if (x_add < 0)
                    {
                        if (!boardCopy.Move(0))
                            moveList.Add(0);
                    }
                    else
                    {
                        if(!boardCopy.Move(1))
                            moveList.Add(1);
                    }
                }

                //Get down the piece on the board's copy
                while (!boardCopy.Move(4))
                {}
                
                //Get all data need to the neural network
                var clearLines= boardCopy.ClearLines().Count;
                //Clear all lines on the board's copy
                boardCopy.TestLineClearing();
                
                var aggregateHeight = boardCopy.GetAggregateHeight();
                var bumpiness= boardCopy.GetBumpiness();
                var countHoles= boardCopy.CountHoles();
                
                var score = MoveScore(bumpiness, countHoles, clearLines, aggregateHeight);
                
                
                //Debug.Log("COMBINAISON " + it_combin);
                
                //Debug.Log("BUMPINESS: " + bumpiness);
                //Debug.Log("CLEAR LINE: " + clearLines);
                //Debug.Log("HOLES: " + countHoles);
                
                //Debug.Log("SCORE ===> " + score);

                //Rotate the piece in the other direction to replace it in the original rotation
                foreach (var move in moveList)
                {
                    if (move == 2) boardCopy.Move(3);
                }
                
                moveScores.Add(score);
                moves.Add(moveList);
                
                it_combin++;
            }
        }
        
        //Get the move with the maximum score
        var maxIndex = 0;
        for (int i = 1; i < moveScores.Count; i++)
        {
            if (moveScores[i] > moveScores[maxIndex]) maxIndex = i;
        }
        
        //Debug.Log("MAX INDEX: " + max_index); 
        
        return moves[maxIndex];
    }

}
