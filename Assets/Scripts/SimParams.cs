using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimParams
{
    public static int trackNum = 0;
    public static int trackCondition = 0;
    public static int genNum = 1;
    public static int genSize = 200;
    public static int numSpecies = 5;
    public static int[] speciesPopulations;
    public static float[][] dna;
    public static int gensBetweenFork = 10;
    public static float mutationProbability = 0.2f;
    public static float maxMutation = 2f;
    public static int[] layerSizes = new int[2] { 4, 3 };
}
