using System;
using System.Collections.Generic;
using UnityEngine;

public class GeneManager : MonoBehaviour
{
    public GameObject[] allSeekers;
    public NeatNetwork[] allNetworks;

    //Prefabs
    public GameObject seekerPrefab;

    //Network
    public int inputNodes = 6;
    public int outputNodes = 2;
    public int hiddenNodes = 0;

    //Current Session
    [Header("Current Session")]
    public int currentGeneration = 0;
    public int startingPopulation;
    public int keepBest;
    public int dropWorst;
    public int currentAlive;

    void Start()
    {
        allSeekers = new GameObject[startingPopulation];
        allNetworks = new NeatNetwork[startingPopulation];

        InnovationTracker.Reset();

        SpawnPopulation();
    }

    void FixedUpdate()
    {
        if (currentAlive <= 0)
        {
            Repopulate();
        }
    }

    void SpawnPopulation()
    {
        for (int i = 0; i < startingPopulation; i++)
        {
            allNetworks[i] = new NeatNetwork(inputNodes, outputNodes, hiddenNodes);

            GameObject seeker = Instantiate(seekerPrefab, Utils.RandomPosition(), Quaternion.identity);
            SeekerController seekerController = seeker.GetComponent<SeekerController>();

            seekerController.ResetSeeker(i, allNetworks[i], inputNodes);

            allSeekers[i] = seeker;
        }

        GameObject.FindObjectOfType<Food>().SpawnFood(startingPopulation * 2);
        currentAlive = startingPopulation;
    }

    void Repopulate()
    {
        SortByFitness();
        SetNewPopulationNetworks();
        RespawnPopulation();
        currentGeneration++;
    }

    void SetNewPopulationNetworks()
    {
        NeatNetwork[] newNetworks = new NeatNetwork[startingPopulation];

        for (int i = 0; i < startingPopulation; i++)
        {
            if (i < keepBest)
            {
                newNetworks[i] = allNetworks[i];
            }
            else if (i < startingPopulation - dropWorst)
            {
                newNetworks[i] = allNetworks[i];
            }
            else
            {
                newNetworks[i] = new NeatNetwork(inputNodes, outputNodes, hiddenNodes);
            }
        }

        allNetworks = newNetworks;
    }

    void SortByFitness()
    {
        System.Array.Sort(allNetworks, (a, b) => b.fitness.CompareTo(a.fitness));
    }

    void RespawnPopulation()
    {
        for (int i = 0; i < startingPopulation; i++)
        {
            GameObject seeker = allSeekers[i];
            SeekerController seekerController = seeker.GetComponent<SeekerController>();

            seekerController.ResetSeeker(i, allNetworks[i], inputNodes);
        }

        currentAlive = startingPopulation;
    }

    public void seekerDeath(int brainIdx, float fitness)
    {
        allNetworks[brainIdx].fitness = fitness;
        currentAlive--;
    }
}

//Tracks historical evolution markers
public static class InnovationTracker
{
    static Dictionary<Tuple<int, int>, int> globalInnovations = new Dictionary<Tuple<int, int>, int>();
    static int innovationNum = 0;

    public static int GetInnovNum(int inNode, int outNode)
    {
        var con = Tuple.Create(inNode, outNode);

        if (globalInnovations.TryGetValue(con, out int innov))
        {
            return innov; //Already exists
        }
        else
        {
            innovationNum++;
            globalInnovations[con] = innovationNum;
            return innovationNum;
        }
    }

    public static void Reset()
    {
        globalInnovations.Clear();
        innovationNum = 0;
    }
}
