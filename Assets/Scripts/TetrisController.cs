using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
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
    public int NEURONS = 4;

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
        GameObject.FindObjectOfType<GeneticAlgorithm>().Death(board.GetScore(), _network);
        Reset();
    }

    public void Reset()
    {
        board.Reset();
    }
    
    public float moveScore(int terrainHeight, int coutHoles, int clearLines, int minHeight, int maxHeight)
    {
        return _network.RunNetwork(terrainHeight, coutHoles, clearLines, minHeight, maxHeight);
    }
    
    private List<int> FindBestMove()
    {
        List<List<int>> moves = new();
        List<float> moveScores = new();

        //Copy the board to make all temporary move to calculate the score of the move
        //Debug.Log(boardCopy.gameObject);

        // Test all differrent combinaison of rotation and postion of the tetromino
        int it_combin = 1;
        for (int x_add = 5; x_add >= -5; x_add--)
        {
            for (int rotate = 0; rotate <= 3; rotate++)
            {
                //Comment this to see all copy and the all different postition
                if (boardCopy)
                {
                    DestroyImmediate(boardCopy.gameObject);
                }

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

                while (!boardCopy.Move(4))
                { }
                
                //Calculate the score of the move
                var minHeight = boardCopy.GetMinHeight();
                var maxHeight = boardCopy.GetMaxHeight();
                var clearLines= boardCopy.ClearLines().Count;
                boardCopy.TestLineClearing();
                var terrainHeight= boardCopy.CalculateTerrainHeight();
                var countHoles= boardCopy.CountHoles();
                
                Debug.Log("COMBINAISON " + it_combin);
                
                Debug.Log("BUMPINESS: " + terrainHeight);
                Debug.Log("CLEAR LINE: " + clearLines);
                Debug.Log("HOLES: " + countHoles);
                
                float score = moveScore(terrainHeight, countHoles, clearLines, minHeight, maxHeight);
                //float score = clearLines * 10 - countHoles * 2 - terrainHeight;
                
                Debug.Log("SCORE ===> " + score);

                for (int i = 0; i < moveList.Count; ++i)
                {
                    if (moveList[i] == 2) boardCopy.Move(3);
                }
                
                moveScores.Add(score);
                moves.Add(moveList);
                
                it_combin++;
            }
        }
        
        //Get the move with the maximum score
        int max_index = 0;
        int min_index = 0;
        for (int i = 1; i < moveScores.Count; i++)
        {
            if (moveScores[i] < moveScores[min_index]) min_index = i;
            if (moveScores[i] > moveScores[max_index]) max_index = i;
        }
        
        Debug.Log("MAX INDEX: " + max_index);
        Debug.Log("MIN INDEX: " + min_index);
        
        return moves[max_index];
    }
    
    //Calculate score on the board
    private float CalculateFitness()
    {
        return board.score;
    }
    
}
