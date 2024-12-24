using System.Collections.Generic;

public class NeatGenome
{
    public List<NodeGene> nodeGenes;
    public List<ConGene> conGenes;

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
