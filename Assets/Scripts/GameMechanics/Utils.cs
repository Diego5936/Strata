using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class Utils
{    
    public static Vector3 RandomPosition()
    {
        float offset = 2;
        float minX = -200 + offset;
        float maxX = 200 - offset;
        float minY = -80 + offset;
        float maxY = 80 - offset;

        Vector3 newPosition;

        float x = UnityEngine.Random.Range(minX, maxX);
        float y = UnityEngine.Random.Range(minY, maxY);

        newPosition = new Vector3(x, y);     

        return newPosition;
    }

    //Saves the best genome in all simulations
    public static void SaveGenome(NeatNetwork network, int iter, int currentGen)
    {
        //Genome processing
        NeatGenome genome = network.myGenome;
        NeatGenomeJson genomeJson = new NeatGenomeJson();

        foreach (NodeGene node in genome.nodeGenes)
        {
            NodeGeneJson nodeJson = new NodeGeneJson();

            nodeJson.id = node.id;
            nodeJson.layer = (NodeGeneJson.LAYER)node.layer;

            genomeJson.nodeGenes.Add(nodeJson);
        }

        foreach (ConGene con in genome.conGenes)
        {
            ConGeneJson conJson = new ConGeneJson();

            conJson.inputNode = con.inputNode;
            conJson.outputNode = con.outputNode;
            conJson.weight = con.weight;
            conJson.isEnabled = con.isEnabled;
            conJson.innovNum = con.innovNum;

            genomeJson.conGenes.Add(conJson);
        }

        string json = JsonUtility.ToJson(genomeJson);
        Debug.Log(json);

        //Include stats in save for analysis
        StringBuilder save = new StringBuilder();

        save.Append($"Fitness: {network.fitness} \nGeneration: {currentGen}\n\n");
        save.Append(json);

        File.WriteAllText(Application.dataPath + $"/ChosenOne_Iter{iter}.txt", save.ToString());
    }

    //Loads best genome saved
    public static NeatGenome LoadGenome()
    {
        string genomeString = File.ReadAllText(Application.dataPath + "/save.txt");

        NeatGenomeJson savedGenome = JsonUtility.FromJson<NeatGenomeJson>(genomeString);
        NeatGenome loadedGenome = new NeatGenome();

        foreach(NodeGeneJson savedNode in savedGenome.nodeGenes)
        {
            NodeGene newNode = new NodeGene(savedNode.id, (NodeGene.LAYER)savedNode.layer);
            loadedGenome.nodeGenes.Add(newNode);
        }
        foreach(ConGeneJson savedCon in savedGenome.conGenes)
        {
            ConGene newCon = new ConGene(savedCon.inputNode, savedCon.outputNode, savedCon.weight, savedCon.isEnabled, savedCon.innovNum);
            loadedGenome.conGenes.Add(newCon);
        }

        return loadedGenome;
    }

    //________________________________________________VISUALIZATION USE
    //Creates text files with population information per generation
    public static void SaveGenerationSpecies(NeatSpecies[] allSpecies, int currentGen)
    {
        StringBuilder generation = new StringBuilder();

        generation.Append($"Generation {currentGen}\n");
        generation.Append($"All Species Count {allSpecies.Length}:\n\n");
        foreach (NeatSpecies species in allSpecies)
        {
            generation.Append($"Species {species.id}, Gen Created: {species.generationEmergence}, Members#: {species.members.Count}, MaxFit: {species.maxFitness}, Stagnation {species.stagnationCount} \n");
            generation.Append($"\tMascot:\n");
            generation.Append($"{PrintNetwork(species.mascot.myGenome, 1)}\n");

            foreach (NeatNetwork seeker in species.members)
            {
                generation.Append($"\tSeeker {seeker.id}, Fitness: {seeker.fitness}\n");
                generation.Append($"\t{PrintNetwork(seeker.myGenome, 1)}\n");
            }
        }

        File.WriteAllText(Application.dataPath + $"/SpeciesIn_{currentGen}.txt", generation.ToString());
    }

    //Returns a network's hidden nodes and connections as a string
    public static string PrintNetwork(NeatGenome genome, int indent)
    {
        StringBuilder networkStr = new StringBuilder();
        
        networkStr.Append(AppendIndents(indent+1));
        networkStr.Append("Hiddden Nodes: [");
        foreach (NodeGene node in genome.nodeGenes)
        {
            if (node.layer == NodeGene.LAYER.Hidden)
            {
                networkStr.Append($"{node.id},");
            }
        }
        networkStr.Append("]\n");
        
        networkStr.Append(AppendIndents(indent+1));
        networkStr.Append($"{genome.conGenes.Count} Connections:\n");
        foreach (ConGene con in genome.conGenes)
        {
            networkStr.Append(AppendIndents(indent+2));
            networkStr.Append($"\t{con.inputNode} -> {con.outputNode}, Weight: {con.weight}, isEnabled? {con.isEnabled}, Innovation: {con.innovNum}\n");
        }

        return networkStr.ToString();
    }

    //Adds n amount of indents to a string
    static string AppendIndents(int n)
    {
        StringBuilder indent = new StringBuilder();

        for (int i = 0; i < n; i++)
        {
            indent.Append("\t");
        }

        return indent.ToString();
    }
}

//Json nodes and cons for IO
[System.Serializable]
public class NeatGenomeJson
{
    public List<NodeGeneJson> nodeGenes = new List<NodeGeneJson>();
    public List<ConGeneJson> conGenes = new List<ConGeneJson>();
}

[System.Serializable]
public class NodeGeneJson
{
    public int id;
    public enum LAYER {Input, Output, Hidden};
    public LAYER layer;
}

[System.Serializable]
public class ConGeneJson
{
    public int inputNode;
    public int outputNode;
    public float weight;
    public bool isEnabled;
    public int innovNum;
}