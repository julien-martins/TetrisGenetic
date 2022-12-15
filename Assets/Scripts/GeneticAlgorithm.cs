using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using TMPro;
using UnityEngine.UI;

public class GeneticAlgorithm : MonoBehaviour
{
    [Header("References")] 
    public TetrisController controller;
    
    [Header("Controls")] 
    public int initialPopulation = 85;
    [Range(0.0f, 1.0f)] public float mutationRate = 0.055f;

    public int bestAgentSelection = 9;
    public int worstAgentSelection = 3;
    public int numberToCrossover;

    private List<int> genePool = new();

    private int naturallySelected;
    private NeuralNetwork[] population;

    [Header("UI")] 
    public TextMeshProUGUI generationNumber;
    public TextMeshProUGUI populationNumber;
    public TextMeshProUGUI mutationNumber;
    public TextMeshProUGUI timeScaleNumber;

    [Header("Public View")] 
    public int currentGeneration;
    public int currentGenome = 0;

    void Start()
    {
        CreatePopulation();

        mutationNumber.text = mutationRate * 100 + "%";
    }

    void CreatePopulation()
    {
        population = new NeuralNetwork[initialPopulation];
        FillPopulationWithRandomValues(population, 0);
        ResetToCurrentGenome();
    }

    void ResetToCurrentGenome()
    {
        controller.ResetWithNetwork(population[currentGenome]);


    }

    void FillPopulationWithRandomValues(NeuralNetwork[] newPopulation, int startingIndex)
    {
        while (startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = new NeuralNetwork();
            newPopulation[startingIndex].Initialize(controller.LAYERS, controller.NEURONS);
            startingIndex++;
        }
    }

    public void Death(float fitness, NeuralNetwork network)
    {
        if (currentGenome < population.Length - 1)
        {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetToCurrentGenome();
        }
        else
        {
            Repopulate();
        }

        generationNumber.text = currentGeneration.ToString();
        populationNumber.text = currentGenome.ToString();
    }

    void Repopulate()
    {
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;
        SortPopulation();

        NeuralNetwork[] newPopulation = PickBestPopulation();
        Crossover(newPopulation);
        Mutate(newPopulation);
        
        FillPopulationWithRandomValues(newPopulation, naturallySelected);

        population = newPopulation;
        
        currentGenome = 0;
        ResetToCurrentGenome();
    }

    void Crossover(NeuralNetwork[] newPopulation){
        for (int i = 0; i < numberToCrossover; i += 2)
        {
            int AIndex = i;
            int BIndex = i + 1;

            if (genePool.Count >= 1)
            {
                for (int l = 0; l < 100; l++)
                {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    if (AIndex != BIndex) break;
                }
            }

            NeuralNetwork child1 = new NeuralNetwork();
            NeuralNetwork child2 = new NeuralNetwork();

            child1.Initialize(controller.LAYERS, controller.NEURONS);
            child2.Initialize(controller.LAYERS, controller.NEURONS);

            child1.fitness = 0;
            child2.fitness = 0;

            for (int w = 0; w < child1.weights.Count; w++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child1.weights[w] = population[AIndex].weights[w];
                    child2.weights[w] = population[BIndex].weights[w];
                }
                else
                {
                    child2.weights[w] = population[AIndex].weights[w];
                    child1.weights[w] = population[BIndex].weights[w];
                }
            }
            
            for (int w = 0; w < child1.biases.Count; w++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child1.biases[w] = population[AIndex].biases[w];
                    child2.biases[w] = population[BIndex].biases[w];
                }
                else
                {
                    child2.biases[w] = population[AIndex].biases[w];
                    child1.biases[w] = population[BIndex].biases[w];
                }
            }

            newPopulation[naturallySelected] = child1;
            naturallySelected++;

            newPopulation[naturallySelected] = child2;
            naturallySelected++;
        }
    }
    
    void Mutate(NeuralNetwork[] newPopulation){
        for (int i = 0; i < naturallySelected; i++)
        {
            for (int c = 0; c < newPopulation[i].weights.Count; c++)
            {
                if (Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
                }
            }
            
        }
    }

    Matrix<float> MutateMatrix(Matrix<float> A)
    {
        int randomPoints = Random.Range(1, (A.RowCount * A.ColumnCount) / 7);

        Matrix<float> C = A;
        for (int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0, C.RowCount);

            C[randomRow, randomColumn] = Mathf.Clamp(C[randomRow, randomColumn] + Random.Range(-1, 1f), -1f, 1f);
        }

        return C;
    }
    
    NeuralNetwork[] PickBestPopulation()
    {
        NeuralNetwork[] newPopulation = new NeuralNetwork[initialPopulation];

        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation[naturallySelected] = population[i].InitializeCopy(controller.LAYERS, controller.NEURONS);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;
            
            //int f = Mathf.RoundToInt(population[i].fitness * 10);
            int f = Mathf.RoundToInt(population[i].fitness );
            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }
        }

        for (int i = 0; i < worstAgentSelection; i++)
        {
            int last = population.Length - 1;
            last -= i;
            
            //int f = Mathf.RoundToInt(population[last].fitness * 10);
            int f = Mathf.RoundToInt(population[last].fitness);
            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }
        }

        return newPopulation;
    }
    
    //Buble Sort Algorithm
    void SortPopulation()
    {
        for (int i = 0; i < population.Length; i++)
        {
            for (int j = i; j < population.Length; j++)
            {
                if (population[i].fitness < population[j].fitness)
                {
                    NeuralNetwork tmp = population[i];
                    population[i] = population[j];
                    population[j] = tmp;
                }
            }
        }
    }

    public void ChangeTimeScale(float value)
    {
        timeScaleNumber.text = value.ToString();
        Time.timeScale = value;
    }
    
}
