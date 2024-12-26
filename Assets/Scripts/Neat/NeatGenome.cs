using System;
using System.Collections.Generic;
using System.Linq;

public class NeatGenome
{
    public List<NodeGene> nodeGenes;
    public List<ConGene> conGenes;
    private HashSet<Tuple<int, int>> existingConnections;

    public NeatGenome()
    {
        nodeGenes = new List<NodeGene>();
        conGenes = new List<ConGene>();
    }

    public NeatGenome(List<NodeGene> nodeGenes, List<ConGene> conGenes)
    {
        this.nodeGenes = nodeGenes;
        this.conGenes = conGenes;
    }

    public void MutateGenome()
    {
        //Structural Mutations
        float newNodeProb = 3;
        float newConProb = 5;
        float sRoll = UnityEngine.Random.Range(0f, 100f);

        if (sRoll <= newNodeProb)
        {
            CreateNewNode();
        }
        if (sRoll <= newConProb)
        {
            CreateNewConnection();
        }

        //Weight Mutation
        foreach (ConGene con in conGenes)
        {
            float weightMutationProb = 80;
            float wRoll = UnityEngine.Random.Range(0f, 100f);

            if (wRoll <= weightMutationProb)
            {
                float perturbWeightProb = 90;
                float tRoll = UnityEngine.Random.Range(0f, 100f);

                if (tRoll <= perturbWeightProb)
                {
                    con.weight += UnityEngine.Random.Range(-0.1f, 0.1f);
                }
                else //10% chance to apply random weight
                {
                    con.weight = UnityEngine.Random.Range(-1, 1);
                }
            }
        }
    }

    private void CreateNewNode()
    {
        //If no connections exist, do nothing
        if (conGenes.Count == 0)
            return;

        //Choose random connection
        int randomConIdx = UnityEngine.Random.Range(0, conGenes.Count);
        ConGene randomCon = conGenes[randomConIdx];

        int inNode = randomCon.inputNode;
        int outNode = randomCon.outputNode;
        randomCon.isEnabled = false;

        //Create new node
        int nextNodeID = nodeGenes.Last().id + 1;
        NodeGene newNode = new NodeGene(nextNodeID, NodeGene.LAYER.Hidden);
        nodeGenes.Add(newNode);

        //Splits original connection in two
        int nextInnovNum = InnovationTracker.GetInnovNum(inNode, newNode.id);
        ConGene firstNewCon = new ConGene(inNode, newNode.id, 1, true, nextInnovNum);
        conGenes.Add(firstNewCon);

        nextInnovNum = InnovationTracker.GetInnovNum(newNode.id, outNode);
        ConGene secondNewCon = new ConGene(newNode.id, outNode, randomCon.weight, true, nextInnovNum);
        conGenes.Add(secondNewCon);
    }

    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^TO BE OPTIMIZED (recalculating hashset everytime can be expensive)
    private void CreateNewConnection()
    {
        //Separate node genes into layers
        List<NodeGene> inputNodes = nodeGenes.Where(node => node.layer == NodeGene.LAYER.Input).ToList();
        List<NodeGene> hiddenNodes = nodeGenes.Where(node => node.layer == NodeGene.LAYER.Hidden).ToList();
        List<NodeGene> outputNodes = nodeGenes.Where(node => node.layer == NodeGene.LAYER.Output).ToList();

        existingConnections = new HashSet<Tuple<int, int>>
                                (conGenes.Select(con => new Tuple<int, int>(con.inputNode, con.outputNode)));

        NodeGene inNode;
        NodeGene outNode;

        int maxAttempts = 50;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            //First node (either input or hidden layer)
            List<NodeGene> eligibleFirstNodes = inputNodes.Concat(hiddenNodes).ToList();
            inNode = eligibleFirstNodes[UnityEngine.Random.Range(0, eligibleFirstNodes.Count)];

            //Second node (subsequent layer(s) to first)
            List<NodeGene> eligibleSecondNodes = inNode.layer == NodeGene.LAYER.Input ?
                                                hiddenNodes.Concat(outputNodes).ToList() : outputNodes;
            outNode = eligibleSecondNodes[UnityEngine.Random.Range(0, eligibleSecondNodes.Count)];

            //Check if connection already exists
            var newCon = Tuple.Create(inNode.id, outNode.id);
            if (existingConnections.Contains(newCon))
            {
                attempts++;
                continue;
            }

            //Create valid connection
            float weight = UnityEngine.Random.Range(-1f,1f);
            int innov = InnovationTracker.GetInnovNum(inNode.id, outNode.id);

            ConGene newConnection = new ConGene(inNode.id, outNode.id, weight, true, innov);
            conGenes.Add(newConnection);               
            return;
        }
    }
}

public class NodeGene
{
    public int id;
    public enum LAYER {Input, Output, Hidden};
    public LAYER layer;
    public NodeGene(int id, LAYER layer)
    {
        this.id = id;
        this.layer = layer;
    }
}

public class ConGene
{
    public int inputNode;
    public int outputNode;
    public float weight;
    public bool isEnabled;
    public int innovNum;

    public ConGene(int inputNode, int outputNode, float weight, bool isEnabled, int innovNum)
    {
        this.inputNode = inputNode;
        this.outputNode = outputNode;
        this.weight = weight;
        this.isEnabled = isEnabled;
        this.innovNum = innovNum;
    } 
}
