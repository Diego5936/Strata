using System.Collections.Generic;
using System;

public class NeatNetwork
{
    public NeatGenome myGenome;
    List<Node> allNodes;
    List<Node> inputNodes;
    List<Node> outputNodes;
    List<Node> hiddenNodes;
    List<Connection> allConnections;

    public int id;
    public float fitness;
    public int foodEaten;
    public float selectionProb;
    
    public NeatNetwork(int id, int inp, int oup, int hid)
    {
        this.id = id;
        myGenome = CreateInitialGenome(inp, oup, hid);

        allNodes = new List<Node>();
        inputNodes = new List<Node>();
        outputNodes = new List<Node>();
        hiddenNodes = new List<Node>();
        allConnections = new List<Connection>();

        CreateNetwork();
    }

    public NeatNetwork(int id, NeatGenome genome)
    {
        this.id = id;
        myGenome = genome;

        allNodes = new List<Node>();
        inputNodes = new List<Node>();
        outputNodes = new List<Node>();
        hiddenNodes = new List<Node>();
        allConnections = new List<Connection>();

        CreateNetwork();
    }

    //Initializes initial genome with empty nodes
    private NeatGenome CreateInitialGenome(int inp, int oup, int hid)
    {
        int nodeID = 0;
        NeatGenome initGenome = new NeatGenome();

        for (int i = 0; i < inp; i++)
        {
            NodeGene newNodeGene = new NodeGene(nodeID, NodeGene.LAYER.Input);
            initGenome.nodeGenes.Add(newNodeGene);
            nodeID++;
        }

        for (int i = 0; i < oup; i++)
        {
            NodeGene newNodeGene = new NodeGene(nodeID, NodeGene.LAYER.Output);
            initGenome.nodeGenes.Add(newNodeGene);
            nodeID++;
        }

        for (int i = 0; i < hid; i++)
        {
            NodeGene newNodeGene = new NodeGene(nodeID, NodeGene.LAYER.Hidden);
            initGenome.nodeGenes.Add(newNodeGene);
            nodeID++;
        }

        return initGenome;
    }

    public void MutateNetwork()
    {
        myGenome.MutateGenome();
        CreateNetwork();
    }

    //Populates network attributes, transforming from genome
    private void CreateNetwork()
    {
        ResetNetwork();

        //Populates Nodes
        foreach (NodeGene nodeGene in myGenome.nodeGenes)
        {
            Node newNode = new Node(nodeGene.id);
            allNodes.Add(newNode);

            switch(nodeGene.layer)
            {
                case NodeGene.LAYER.Input:
                    inputNodes.Add(newNode);
                    break;
                case NodeGene.LAYER.Output:
                    outputNodes.Add(newNode);
                    break;
                case NodeGene.LAYER.Hidden:
                    hiddenNodes.Add(newNode);
                    break;
            }
        }

        //Populates Connections
        foreach (ConGene conGene in myGenome.conGenes)
        {
            Connection newCon = new Connection(conGene.inputNode, conGene.outputNode, 
                                                conGene.weight, conGene.isEnabled);
            allConnections.Add(newCon);
        }

        //Adds connections in the Node's lists
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^TO BE OPTIMIZED
        foreach (Node node in allNodes)
        {
            foreach (Connection con in allConnections)
            {
                if (con.inputNode == node.id)
                    node.outputConnections.Add(con);
                else if (con.outputNode == node.id)
                    node.inputConnections.Add(con);
            }
        }
    }

    private void ResetNetwork()
    {
        allNodes.Clear();
        inputNodes.Clear();
        outputNodes.Clear();
        hiddenNodes.Clear();
        allConnections.Clear();
    }

    //Passes information through network
    public float[] FeedForwardNetwork(float[] inputs)
    {
        float[] outputs = new float[outputNodes.Count];

        for (int i = 0; i < inputNodes.Count; i++)
        {
            inputNodes[i].SetInputNodeValue(inputs[i]);
            inputNodes[i].FeedForwardValue();

            inputNodes[i].value = 0;
        }

        for (int i = 0; i < hiddenNodes.Count; i++)
        {
            hiddenNodes[i].SetHiddenNodeValue();
            hiddenNodes[i].FeedForwardValue();

            hiddenNodes[i].value = 0;
        }

        //Specific setting for output nodes: Acceleration, Rotation
        for (int i = 0; i < outputNodes.Count; i++)
        {
            outputNodes[i].SetOutputNodeValue(i);
            outputs[i] = outputNodes[i].value;

            outputNodes[i].value = 0;
        }

        return outputs;
    }
}

public class Node
{
    public int id;
    public float value;
    public List<Connection> inputConnections;
    public List<Connection> outputConnections;

    public Node(int id)
    {
        this.id = id;
        inputConnections = new List<Connection>();
        outputConnections = new List<Connection>();
    }

    //Sets input layer to game sensors
    public void SetInputNodeValue(float val)
    {
        value = val;
    }

    //Sets hidden layer from input connections
    public void SetHiddenNodeValue()
    {
        float val = 0;

        foreach (Connection con in inputConnections)
        {
            if (con.isEnabled)
                val += con.weight * con.inputNodeValue;            
        }

        value = Tanh(val);
    }

    //Sets output later from input connections
    public void SetOutputNodeValue(int i)
    {
        float val = 0;
        
        foreach (Connection con in inputConnections)
        {
            if (con.isEnabled)
                val += con.weight * con.inputNodeValue;            
        }

        if (i == 0)
            value = Sigmoid(val); // Acceleration [0, 1]     
        else   
            value = Tanh(val); //Rotation [-1, 1]
    }

    //Passes value on to connection
    public void FeedForwardValue()
    {
        foreach (Connection con in outputConnections)
        {
            con.inputNodeValue = value;
        }
    }

    private float Tanh(float x)
    {
        return (float)Math.Tanh(x);
    }

    private float Sigmoid(float x)
    {
        return 1 / (1 + (float)Math.Exp(-x));
    }
}

public class Connection
{
    public int inputNode;
    public int outputNode;
    public float weight;
    public bool isEnabled;
    public float inputNodeValue;

    public Connection(int inputNode, int outputNode, float weight, bool isEnabled)
    {
        this.inputNode = inputNode;
        this.outputNode = outputNode;
        this.weight = weight;
        this.isEnabled = isEnabled;
    }     
}