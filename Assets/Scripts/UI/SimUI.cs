using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SimUI : MonoBehaviour
{
    [SerializeField] Transform playerCar = null;
    [SerializeField] GameObject pauseMenu = null;
    [SerializeField] GameObject telemetry = null;
    [SerializeField] GameObject dashcam = null;
    [SerializeField] GameObject neuralNet = null;
    [SerializeField] GameObject graphs = null;
    [SerializeField] InputField fileName = null;

    CarHandling playerCarHandling;

    public static bool IsPaused { get; private set; } = false;

    string DNAString {
        get {
            //generate output string header (with hidden layer sizes) at the start:
            string output = NeuralNet.layerSizes[1].ToString();
            for (int i = 2; i < NeuralNet.layerSizes.Length - 1; i++)
            {
                output += $",{NeuralNet.layerSizes[i]}";
            }

            for (int i = 0; i < CarsManager.PrevDNA.Count; i++)
            {
                output += "\n";

                //Add each dna value to the output string:
                float[] dna = CarsManager.PrevDNA[i];
                output += dna[0]; //initialise output to add just the first DNA value to prevent additional comma
                for (int j = 1; j < dna.Length; j++)
                {
                    output += $",{dna[j]}";
                }
            }
            return output;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        playerCarHandling = playerCar.GetComponent<CarHandling>();
    }

    // Update is called once per frame
    void Update()
    {
        //Pause simulation and open pause menu:
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        //Disable most keybindings if paused:
        if (!IsPaused)
        {
            //Toggle telemetry:
            if (Input.GetKeyDown(KeyCode.T))
            {
                telemetry.SetActive(!telemetry.activeSelf);
            }

            //Toggle dashcam:
            if (Input.GetKeyDown(KeyCode.C))
            {
                dashcam.SetActive(!dashcam.activeSelf);
            }

            //Toggle neural net diagram:
            if (Input.GetKeyDown(KeyCode.N))
            {
                neuralNet.SetActive(!neuralNet.activeSelf);
            }

            //Toggle graphs:
            if (Input.GetKeyDown(KeyCode.G))
            {
                graphs.SetActive(!graphs.activeSelf);
            }

            //Toggle whether or not only parents are displayed:
            if (Input.GetKeyDown(KeyCode.P))
            {
                CarsManager.ToggleParentsOnly();
            }

            //Toggle 'Race AI' mode:
            if (Input.GetKeyDown(KeyCode.R))
            {
                CarsManager.evolutionInhibit = !CarsManager.evolutionInhibit;

                //Reset player car if exiting race mode:
                if (!CarsManager.evolutionInhibit)
                {
                    playerCar.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    playerCarHandling.Velocity = 0;
                }
            }
        }
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        pauseMenu.SetActive(IsPaused);
    }

    public void SaveSim()
    {
        //Create and populate simulation save file string using previous generation:
        string output = $"{SimParams.genNum - 1}\n{SimParams.genSize}\n{SimParams.numSpecies}\n{CarsManager.PrevSpeciesSizes[0]}";
        for (int i = 1; i < SimParams.numSpecies; i++) //Starts from 1 to avoid extra comma
        {
            output += $",{CarsManager.PrevSpeciesSizes[i]}";
        }
        output += $"\n{SimParams.gensBetweenFork}\n{SimParams.mutationProbability}\n{SimParams.maxMutation}\n{SimParams.trackNum}\n{SimParams.trackCondition}\n{DNAString}";

        //Initialise StreamWriter and write simulation info to it:
        string path = Application.dataPath + "/Output/Simulations";
        Directory.CreateDirectory(path);
        using (StreamWriter streamWriter = new StreamWriter($"{path}/{fileName.text}.txt"))
        {
            streamWriter.Write(output);
        }
    }
}
