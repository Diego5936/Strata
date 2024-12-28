using System;
using System.Collections.Generic;
using System.Linq;

public class NeatUtils
{
    static int keepDisabledChance = 75;

    //Returns offspring off of two parent genomes
    public static NeatGenome Crossover(NeatGenome parent1, NeatGenome parent2, float fitness1, float fitness2)
    {
        if (fitness1 < fitness2)
        {
            (parent2, parent1) = (parent1, parent2);
        }

        List<NodeGene> childNodes = new List<NodeGene>();
        List<ConGene> childCons = new List<ConGene>();   

        //Nodes of the fittest parent remain
        foreach (NodeGene node in parent1.nodeGenes)
        {
            childNodes.Add(new NodeGene(node.id, node.layer));
        }     

        CompareConnections
        (
            parent1.conGenes, parent2.conGenes,
            (con1, con2) =>
            {
                //On match, weight is of random parent
                float childWeight = UnityEngine.Random.Range(0, 100) < 50 ? con1.weight : con2.weight;

                //Chance that a disabled gene stays disabled
                bool switchToEnable = con1.isEnabled && con2.isEnabled;
                if (!switchToEnable)
                    switchToEnable = UnityEngine.Random.Range(0, 100) >= keepDisabledChance; 
                
                childCons.Add(new ConGene(con1.inputNode, con1.outputNode, childWeight, switchToEnable, con1.innovNum));
            },
            (con1) =>
            {
                //Disjoint and Excess from fitter parent stay
                childCons.Add(new ConGene(con1.inputNode, con1.outputNode, con1.weight, con1.isEnabled, con1.innovNum));
            },
            (con2) => {}
        );

        NeatGenome childGenome = new NeatGenome(childNodes, childCons);
        
        return childGenome;
    }

    //Calculate the compatibility of two genomes
    public static float GetCompatabilityDistance(NeatGenome firstGenome, NeatGenome secondGenome, float c1, float c2, float c3)
    {
        (int disjoint, int excess, float aWd) = GetDisjointExcessAWD(firstGenome.conGenes, secondGenome.conGenes); 
        
        int N = Math.Max(firstGenome.conGenes.Count, secondGenome.conGenes.Count);

        float compDis = c1*excess/N + c2*disjoint/N + c3*aWd;
        // UnityEngine.Debug.Log($"Excess: {excess}, Disjoint: {disjoint}, AWD: {aWd}, CompDist: {compDis}");
        return compDis;
    }

    //Get # of disjoint nodes, # of excess nodes, and average weight difference of two genomes
    public static (int, int, float) GetDisjointExcessAWD(List<ConGene> firstCons, List<ConGene> secondCons)
    {
        int disjoint = 0, excess = 0, matchingCount = 0;
        float aWd = 0;

        int maxInnov1 = firstCons.Count > 0 ? firstCons.Last().innovNum : 0;
        int maxInnov2 = secondCons.Count > 0 ? secondCons.Last().innovNum : 0;

        //Calculates the point in which one list innovs ends as the other continues (for excess vs disjoint)
        int innovRange = Math.Min(maxInnov1, maxInnov2);

        CompareConnections
        (
            firstCons, secondCons, 
            (con1, con2) => 
            {
                aWd += Math.Abs(con1.weight - con2.weight);
                matchingCount++;
            },
            (con1) => 
            {
                if (con1.innovNum > innovRange)
                    excess++;
                else
                    disjoint++;
            },
            (con2) => 
            {
                if (con2.innovNum > innovRange)
                    excess++;
                else
                    disjoint++;
            }
        );

        aWd = matchingCount > 0 ? aWd / matchingCount : 0;
        
        return (disjoint, excess, aWd);
    }

    //Compares two connection gene lists and returns callbacks depending on matches
    public static void CompareConnections(List<ConGene> firstCons, List<ConGene> secondCons,
                                        Action<ConGene, ConGene> onMatch, Action<ConGene> firstOnly, Action<ConGene> secondOnly)
    {
        firstCons.Sort((a, b) => a.innovNum.CompareTo(b.innovNum));
        secondCons.Sort((a, b) => a.innovNum.CompareTo(b.innovNum));

        int i = 0, j = 0;

        while (i < firstCons.Count || j < secondCons.Count)
        {
            //if within first range AND (no more connections on second list OR second list innov skips to higher number)
            if (i < firstCons.Count && (j >= secondCons.Count || firstCons[i].innovNum < secondCons[j].innovNum))
            {
                //Then innov only appears in first
                firstOnly(firstCons[i]);
                i++;
            }
            //if within second range AND (no more connections on first list OR first list innov skips to higher number)
            else if (j < secondCons.Count && (i >= firstCons.Count || secondCons[j].innovNum < firstCons[i].innovNum))
            {
                //Then innov only appears in second
                secondOnly(secondCons[j]);
                j++;
            }
            else
            {
                //Innovs match
                onMatch(firstCons[i], secondCons[j]);
                i++;
                j++;
            }
        }
    }
}
