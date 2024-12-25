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
        //sort all networks by fitness
        //set new population networks

        RespawnPopulation();
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

    public void seekerDeath()
    {
        currentAlive--;
    }
}
