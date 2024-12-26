using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GeneManager : MonoBehaviour
{
    public GameObject[] allSeekers;
    public NeatNetwork[] allNetworks;

    //Prefabs
    public GameObject seekerPrefab;

    //Networks
    public int inputNodes = 6;
    public int outputNodes = 2;
    public int hiddenNodes = 0;

    //Species
    public List<NeatSpecies> allSpecies;
    public Dictionary<NeatNetwork, NeatSpecies> speciesDic;
    public float compThreshold = 3;

    //Current Session
    [Header("Session Settings")]
    public int startingPopulation;
    public float cullPercent = 0.5f;
    float justMutatePercent = 0.25f;

    [Header("Current Session")]
    public int currentGeneration = 0;
    public int currentAlive;
    public int speciesCount;

    float populationFitness;
    List<float> allAvgPopFitness = new List<float>();

    [Header("Generational Stats")]
    public float avgPopulationFitness;
    public float lastAvgPopFitness;
    public float fitnessDifference;

    [Header("CHAMPION")]
    public int champGeneration;
    public float champFitness = 0;
    public int foodEaten;


    void Start()
    {
        allSeekers = new GameObject[startingPopulation];
        allNetworks = new NeatNetwork[startingPopulation];
        allSpecies = new List<NeatSpecies>();

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

    //Initialize first mutated networks and match them to bodies
    void SpawnPopulation()
    {
        for (int i = 0; i < startingPopulation; i++)
        {
            allNetworks[i] = new NeatNetwork(inputNodes, outputNodes, hiddenNodes);
            allNetworks[i].MutateNetwork();

            GameObject seeker = Instantiate(seekerPrefab, Utils.RandomPosition(), Quaternion.identity);
            SeekerController seekerController = seeker.GetComponent<SeekerController>();

            seekerController.ResetSeeker(i, allNetworks[i], inputNodes, Color.magenta);

            allSeekers[i] = seeker;
        }

        GameObject.FindObjectOfType<Food>().SpawnFood(startingPopulation * 2);
        currentAlive = startingPopulation;
    }

    void Repopulate()
    {
        //Fitness calculation
        Array.Sort(allNetworks, (a, b) => b.fitness.CompareTo(a.fitness));
        populationFitness = allNetworks.Sum(network => network.fitness);

        //Population stats
        avgPopulationFitness = populationFitness / startingPopulation;
        allAvgPopFitness.Add((float)Math.Round(avgPopulationFitness, 2));

        int lastGenIdx = currentGeneration - 1;
        lastAvgPopFitness = lastGenIdx >= 0 ? allAvgPopFitness[lastGenIdx] : 0;

        fitnessDifference = (float)Math.Round(avgPopulationFitness - lastAvgPopFitness, 2);

        //Champion stats
        NeatNetwork populationBest = allNetworks.First();
        if (populationBest.fitness > champFitness)
        {
            champFitness = populationBest.fitness;
            champGeneration = currentGeneration;
            foodEaten = populationBest.foodEaten;
        }

        //Speciation and Respawn
        SpeciatePopulation();
        CalculateFitnessSharing();
        SetNewPopulationNetworks();
        RespawnPopulation();

        currentGeneration++;
    }

    void SpeciatePopulation()
    {
        //Resets existing 
        speciesCount = 0;
        allSpecies.Clear();
        if (speciesDic == null)
            speciesDic = new Dictionary<NeatNetwork, NeatSpecies>();
        else
            speciesDic.Clear();

        //Matches networks to species or creates new species
        foreach (NeatNetwork seeker in allNetworks)
        {
            bool found = false;

            foreach (NeatSpecies species in allSpecies)
            {
                float compDist = NeatUtils.GetCompatabilityDistance(seeker.myGenome, species.mascot.myGenome);

                if (compDist < compThreshold)
                {
                    species.members.Add(seeker);
                    speciesDic[seeker] = species;

                    species.mascot = seeker.fitness > species.mascot.fitness ? seeker : species.mascot;
                    species.speciesFitness += seeker.fitness;

                    found = true;
                    break;
                }
            }

            if (!found)
            {
                NeatSpecies newSpecies = new NeatSpecies(seeker);
                allSpecies.Add(newSpecies);
                speciesDic.Add(seeker, newSpecies);
                speciesCount++;
            }
        }
    }

    void CalculateFitnessSharing()
    {
        foreach (NeatNetwork seeker in allNetworks)
        {
            NeatSpecies thisSpecies = speciesDic[seeker];
            seeker.fitness /= thisSpecies.members.Count;

            //Probabilty of being selected as a parent for crossover
            seeker.selectionProb = thisSpecies.speciesFitness > 0 ?
                                    seeker.fitness / thisSpecies.speciesFitness : 0;
        }
    }

    void SetNewPopulationNetworks()
    {
        List<NeatNetwork> newNetworks = new List<NeatNetwork>();
        int addedMembers = 0;

        foreach (NeatSpecies species in allSpecies)
        {
            //Remove weakest organisms
            species.members.Sort((a, b) => b.fitness.CompareTo(a.fitness));

            int speciesCount = species.members.Count;

            if (speciesCount > 3)
            {
                //Remove as long as species is big enough
                int numToRemove = Mathf.FloorToInt(speciesCount * cullPercent); 
                int removeIdx = speciesCount - numToRemove;

                species.members.RemoveRange(removeIdx, numToRemove);
            }

            //Leave species champion untouched
            if (species.members.Count > 5)
            {
                newNetworks.Add(species.members.First());
                addedMembers++;
            }

            //Crossover
            int offspringNum = Mathf.FloorToInt(species.speciesFitness / populationFitness * startingPopulation);
            int justMutateNum = Mathf.FloorToInt(offspringNum * justMutatePercent);

            for (int i = 0; i < offspringNum; i++)
            {
                NeatNetwork offspring;

                //Offspring that are a result of single parent mutation
                if (i < justMutateNum)
                {
                    offspring = FitnessProportionateSelect(species);                    
                }
                else
                {
                    NeatNetwork parent1 = FitnessProportionateSelect(species);
                    NeatNetwork parent2 = FitnessProportionateSelect(species);

                    NeatGenome offspringGenome = NeatUtils.Crossover(parent1.myGenome, parent2.myGenome,
                                                                    parent1.fitness, parent2.fitness);
                    offspring = new NeatNetwork(offspringGenome);
                }

                offspring.MutateNetwork();
                newNetworks.Add(offspring);
                addedMembers++;
            }
        }

        allSpecies.Sort((a, b) => b.speciesFitness.CompareTo(a.speciesFitness));
        while (addedMembers < startingPopulation)
        {
            NeatSpecies bestSpecies = allSpecies.First();

            NeatNetwork parent1 = FitnessProportionateSelect(bestSpecies);
            NeatNetwork parent2 = FitnessProportionateSelect(bestSpecies);

            NeatGenome offspringGenome = NeatUtils.Crossover(parent1.myGenome, parent2.myGenome,
                                                            parent1.fitness, parent2.fitness);
            NeatNetwork offspring = new NeatNetwork(offspringGenome);

            offspring.MutateNetwork();
            newNetworks.Add(offspring);
            addedMembers++;
        }

        allNetworks = newNetworks.ToArray();
    }

    //Chooses a parent from a species in a roulette-wheel approach
    NeatNetwork FitnessProportionateSelect(NeatSpecies species)
    {
        float roll = UnityEngine.Random.value; // Random number [0, 1]
        float cumulativeProbability = 0;

        foreach (NeatNetwork seeker in species.members)
        {
            cumulativeProbability += seeker.selectionProb;
            if (roll <= cumulativeProbability)
            {
                return seeker; // Selected parent
            }
        }

        return species.members.First();
    }

    void RespawnPopulation()
    {
        for (int i = 0; i < startingPopulation; i++)
        {
            GameObject seeker = allSeekers[i];
            SeekerController seekerController = seeker.GetComponent<SeekerController>();

            Color color = speciesDic[seekerController.myNetwork].speciesColor;
            seekerController.ResetSeeker(i, allNetworks[i], inputNodes, color);
        }

        currentAlive = startingPopulation;
    }

    public void seekerDeath(int brainIdx, float fitness, int foodEaten)
    {
        allNetworks[brainIdx].fitness = fitness;
        allNetworks[brainIdx].foodEaten = foodEaten;
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

public class NeatSpecies
{
    public NeatNetwork mascot;
    public List<NeatNetwork> members;
    public float speciesFitness;
    public Color speciesColor;

    public NeatSpecies(NeatNetwork startingMember)
    {
        mascot = startingMember;
        members = new List<NeatNetwork>{startingMember};

        speciesFitness = startingMember.fitness;
        speciesColor = new Color(UnityEngine.Random.Range(0f,255f)/255f, 
                                UnityEngine.Random.Range(0f,255f)/255f, 
                                UnityEngine.Random.Range(0f,255f)/255f);
    }
}
