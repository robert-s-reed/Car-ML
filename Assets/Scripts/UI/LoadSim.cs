using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSim : MonoBehaviour
{
    [SerializeField] InputField fileName = null;

    public void Load()
    {
        using (StreamReader streamReader = new StreamReader($"{Application.dataPath}/Output/Simulations/{fileName.text}.txt"))
        {
            //Load Sim Parameters:
            SimParams.genNum = int.Parse(streamReader.ReadLine());
            SimParams.genSize = int.Parse(streamReader.ReadLine());
            SimParams.numSpecies = int.Parse(streamReader.ReadLine());
            string[] speciesPopulation = streamReader.ReadLine().Split(',');
            SimParams.speciesPopulations = new int[speciesPopulation.Length];
            for (int i = 0; i < speciesPopulation.Length; i++)
            {
                SimParams.speciesPopulations[i] = int.Parse(speciesPopulation[i]);
            }
            SimParams.gensBetweenFork = int.Parse(streamReader.ReadLine());
            SimParams.mutationProbability = float.Parse(streamReader.ReadLine());
            SimParams.maxMutation = float.Parse(streamReader.ReadLine());
            SimParams.trackNum = int.Parse(streamReader.ReadLine());
            SimParams.trackCondition = int.Parse(streamReader.ReadLine());
            string[] layerSizes = streamReader.ReadLine().Split(',');
            SimParams.layerSizes = new int[layerSizes.Length];
            for (int i = 0; i < layerSizes.Length; i++)
            {
                SimParams.layerSizes[i] = int.Parse(layerSizes[i]);
            }

            //Load DNA:
            SimParams.dna = new float[SimParams.genSize][];
            for (int i = 0; i < SimParams.genSize; i++)
            {
                string[] dnaElements = streamReader.ReadLine().Split(',');
                SimParams.dna[i] = new float[dnaElements.Length];
                for (int j = 0; j < dnaElements.Length; j++)
                {
                    SimParams.dna[i][j] = float.Parse(dnaElements[j]);
                }
            }
        }

        SceneManager.LoadScene("Simulation");
    }
}
