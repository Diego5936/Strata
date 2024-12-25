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

        StartingNetworks();
        SpawnPopulation();
    }

    void StartingNetworks()
    {
        for (int i = 0; i < startingPopulation; i++)
        {
            allNetworks[i] = new NeatNetwork(inputNodes, outputNodes, hiddenNodes);
        }
    }

    void SpawnPopulation()
    {
        for (int i = 0; i < startingPopulation; i++)
        {
            Vector2 seekerPosition = Utils.RandomPosition();

            GameObject seekerObject = Instantiate(seekerPrefab, seekerPosition, Quaternion.identity);
            SeekerController seeker = seekerObject.GetComponent<SeekerController>();

            seeker.myBrainIdx = i;
            seeker.myNetwork = allNetworks[i];
            seeker.sensorsNum = inputNodes;

            allSeekers[i] = seekerObject; 
        }

        currentAlive = startingPopulation;
    }

    public void seekerDeath()
    {
        currentAlive--;
    }
}
