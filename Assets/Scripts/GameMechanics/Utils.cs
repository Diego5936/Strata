using System.IO;
using System.Text;
using Unity.Collections;
using UnityEngine;

public class Utils
{    
    public static Vector3 RandomPosition()
    {
        float offset = 2;
        float minX = -100 + offset;
        float maxX = 100 - offset;
        float minY = -40 + offset;
        float maxY = 40 - offset;

        Vector3 newPosition;

        float x = UnityEngine.Random.Range(minX, maxX);
        float y = UnityEngine.Random.Range(minY, maxY);

        newPosition = new Vector3(x, y);     

        return newPosition;
    }

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

    static string AppendIndents(int n)
    {
        StringBuilder indent = new StringBuilder();

        for (int i = 0; i < n; i++)
        {
            indent.Append("\t");
        }

        return indent.ToString();
    }

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
        SaveForChat(generation.ToString());
    }

    static StringBuilder allData = new StringBuilder();

    public static void SaveForChat(string generation)
    {
        allData.Append($"{generation}\n\n");
        
        File.WriteAllText(Application.dataPath + $"/AllData.txt", allData.ToString());
    }

    public static void SaveGenerationSeekers(NeatNetwork seeker, int startingPopulations, int currentGen)
    {
        StringBuilder generation = new StringBuilder();

        File.WriteAllText(Application.dataPath + $"/SeekersIn_{currentGen}.txt", generation.ToString());
    }
}
