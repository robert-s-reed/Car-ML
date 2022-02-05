using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CarsManager : MonoBehaviour
{
    [System.Serializable] public struct SpeciesInfo
    {
        public string name;
        public Color colour;
    }

    const int MIN_CARS_PER_SPECIES = 5;

    public SpeciesInfo[] speciesInfo;

    [HideInInspector] public static bool evolutionInhibit = false;

    static int carsRemaining;
    static List<CarInfo> cars = new List<CarInfo>();
    static int[] speciesSizes;
    static List<CarInfo>[] speciesCars; //array of species each containing an array of cars of that species
    static int[] speciesCarsRemaining;
    static bool[] speciesFinished;
    static bool parentsOnly = false;

    [SerializeField] GameObject carPrefab = null;
    [SerializeField] Text genInfoText = null;
    [SerializeField] Graph distanceGraph = null;
    [SerializeField] Graph speedGraph = null;

    Transform fittestCar = null;
    List<CarInfo> carsSorted = new List<CarInfo>();

    public static CarsManager This { get; private set; } //reference to only instance to enable static methods to access non-static fields

    public static List<float[]> PrevDNA { get; private set; }

    public static int[] PrevSpeciesSizes { get; private set; }

    void Awake()
    {
        This = this; //assign static class reference as this (and only) instance

        //Load parameters from SimParams:
        PrevDNA = new List<float[]>();
        speciesSizes = new int[SimParams.numSpecies];
        PrevSpeciesSizes = new int[SimParams.numSpecies];
        speciesCars = new List<CarInfo>[SimParams.numSpecies];
        speciesCarsRemaining = new int[SimParams.numSpecies];
        speciesFinished = new bool[SimParams.numSpecies];

        if (SimParams.speciesPopulations != null)
        {
            //Load species populations from SimParams:
            speciesSizes = SimParams.speciesPopulations;
        }
        else
        {
            //Calculate species populations (as per algorithm in design):
            int minCarsPerSpecies = Mathf.FloorToInt(SimParams.genSize / SimParams.numSpecies);
            int numLargerSpecies = SimParams.genSize % SimParams.numSpecies;
            for (int i = 0; i < SimParams.numSpecies; i++)
            {
                if (i < numLargerSpecies)
                {
                    speciesSizes[i] = minCarsPerSpecies + 1;
                }
                else
                {
                    speciesSizes[i] = minCarsPerSpecies;
                }
                speciesCarsRemaining[i] = speciesSizes[i];
            }
        }

        //initialise species cars remaining to species population (since no cars have died):
        for (int i = 0; i < speciesSizes.Length; i++)
        {
            speciesCarsRemaining[i] = speciesSizes[i];
        }

        //Instantiate AI cars:
        int dnaIndex = 0; //Keeps track of index of entire population so that DNA could be imported if necessary
        for (int i = 0; i < SimParams.numSpecies; i++)
        {
            speciesCars[i] = new List<CarInfo>();
            for (int j = 0; j < speciesSizes[i]; j++)
            {
                CarInfo newCar = Instantiate(carPrefab).GetComponent<CarInfo>();
                newCar.ResetCar(i);
                cars.Add(newCar);
                speciesCars[i].Add(newCar);

                //Load DNA if applicable:
                if (SimParams.dna != null)
                {
                    newCar.ai.NN.ImportDNA(SimParams.dna[dnaIndex]);
                    dnaIndex++;
                }
            }
        }

        //Initialise gen. info:
        carsRemaining = SimParams.genSize;
        UpdateGenInfo();
    }

    // Update is called once per frame
    void Update()
    {
        //Sort cars by fitness and alert CamManager if the lead car (that is still alive) has changed
        carsSorted = cars.OrderByDescending(x => x.Fitness).ToList();
        for (int i = 0; i < SimParams.genSize; i++)
        {
            if (carsSorted[i].IsAlive && (!parentsOnly || carsSorted[i].IsParent))
            {
                if (fittestCar != carsSorted[i].transform)
                {
                    //Make exterior lights only visible on new car and update fittestCar:
                    if (fittestCar != null)
                    {
                        fittestCar.GetComponent<CarInfo>().ToggleLights();
                    }
                    fittestCar = carsSorted[i].transform;
                    carsSorted[i].ToggleLights();

                    //Update camera and UI:
                    CamManager.NewFittest(fittestCar);
                    NeuralNetDiagram.SetNeuralNet(fittestCar.GetComponent<AICar>().NN);
                }
                break;
            }
        }
    }

    public static void ToggleParentsOnly()
    {
        parentsOnly = !parentsOnly;
        for (int i = 0; i < SimParams.genSize; i++)
        {
            if (!parentsOnly || cars[i].IsParent)
            {
                cars[i].rend.enabled = true;
            }
            else
            {
                cars[i].rend.enabled = false;
            }
        }
    }

    static void UpdateGenInfo()
    {
        //Create gen info text:
        string genInfo = $"Generation: {SimParams.genNum}\nTotal: {carsRemaining}/{SimParams.genSize}";
        for (int i = 0; i < SimParams.numSpecies; i++)
        {
            genInfo += $"\n{This.speciesInfo[i].name}: {speciesCarsRemaining[i]}/{speciesSizes[i]}";
        }
        This.genInfoText.text = genInfo;
    }

    public static void KillCar(int speciesIndex, bool isFinished)
    {
        carsRemaining--;
        speciesCarsRemaining[speciesIndex]--;
        if (isFinished || speciesCarsRemaining[speciesIndex] <= 0)
        {
            speciesFinished[speciesIndex] = true;
        }
        bool isGenComplete = true;
        for (int i = 0; i < SimParams.numSpecies; i++)
        {
            if (!speciesFinished[i])
            {
                isGenComplete = false;
                break;
            }
        }

        //End generation if no cars remain:
        if (carsRemaining <= 0 || isGenComplete)
        {
            //Calculate total fitness from best of each species and identify best species:
            float[] bestFitnesses = new float[SimParams.numSpecies];
            float[][] parentDNAs = new float[SimParams.numSpecies][];
            float totFitness = 0;
            int fittestIndex = 0;
            int weakestIndex = 0;
            float fittestFitness = -1f;
            float weakestFitness = Mathf.Infinity; //To ensure there will always be a generation weaker than the default

            //Evolve only if not inhibitted:
            if (!evolutionInhibit)
            {
                //Determine fittest and weakes species and save DNA:
                PrevDNA.Clear();
                for (int i = 0; i < SimParams.numSpecies; i++)
                {
                    CarInfo bestCar = speciesCars[i].OrderByDescending(x => x.Fitness).First();

                    //Save DNA:
                    for (int j = 0; j < speciesCars[i].Count; j++)
                    {
                        PrevDNA.Add(speciesCars[i][j].ai.NN.GetDNAArray());
                    }

                    bestFitnesses[i] = Mathf.Sqrt(bestCar.Fitness);
                    parentDNAs[i] = bestCar.ai.NN.GetDNAArray();
                    PrevSpeciesSizes[i] = speciesSizes[i];
                    speciesCars[i].Clear();
                    totFitness += bestFitnesses[i];
                    This.distanceGraph.AddData(i, bestCar.Distance);
                    This.speedGraph.AddData(i, bestCar.AvgV * CarHandling.MPS2MPH);

                    if (bestFitnesses[i] > fittestFitness)
                    {
                        fittestIndex = i;
                        fittestFitness = bestFitnesses[i];
                    }
                    if (bestFitnesses[i] < weakestFitness)
                    {
                        weakestIndex = i;
                        weakestFitness = bestFitnesses[i];
                    }
                }

                //Give DNA from fittest to weakest if fork generation
                if (SimParams.genNum % SimParams.gensBetweenFork == 0)
                {
                    parentDNAs[weakestIndex] = parentDNAs[fittestIndex];
                    totFitness += fittestFitness - weakestFitness;
                    bestFitnesses[weakestIndex] = fittestFitness;
                }

                //Calcualte population of each species based on their performance
                int carsAssigned = 0;
                for (int i = 0; i < SimParams.numSpecies; i++)
                {
                    if (i != fittestIndex)
                    {
                        speciesSizes[i] = Mathf.Max(Mathf.FloorToInt((bestFitnesses[i] / totFitness) * SimParams.genSize), MIN_CARS_PER_SPECIES);
                        carsAssigned += speciesSizes[i];
                    }
                }

                //Assign final species to have a size of the rest of the population:
                speciesSizes[fittestIndex] = SimParams.genSize - carsAssigned;

                SimParams.genNum++;
            }

            speciesFinished = new bool[SimParams.numSpecies];

            //Mutate cars and populate species:
            int popIndex = 0;
            for (int i = 0; i < SimParams.numSpecies; i++)
            {
                for (int j = 0; j < speciesSizes[i] - 1; j++)
                {
                    cars[popIndex].ai.ResetCar(parentDNAs[i], evolutionInhibit, true);
                    cars[popIndex].ResetCar(i);
                    cars[popIndex].rend.enabled = true;
                    speciesCars[i].Add(cars[popIndex]);
                    popIndex++;
                }

                //Leave one car unmutated as the parent:
                cars[popIndex].ai.ResetCar(parentDNAs[i], evolutionInhibit);
                cars[popIndex].ResetCar(i, true);
                cars[popIndex].rend.enabled = true;
                speciesCars[i].Add(cars[popIndex]);
                popIndex++;

                speciesCarsRemaining[i] = speciesSizes[i];
            }

            //Recalculate parents to show if necessary:
            if (parentsOnly)
            {
                parentsOnly = false;
                ToggleParentsOnly();
            }

            carsRemaining = SimParams.genSize;
        }
        UpdateGenInfo();
    }
}
