using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewSim : MonoBehaviour
{
    [SerializeField] Slider genSizeSlider = null;
    [SerializeField] Slider numSpeciesSlider = null;
    [SerializeField] Slider gensBetweenForkSlider = null;
    [SerializeField] Slider trackNumSlider = null;
    [SerializeField] Dropdown weatherDropdown = null;
    [SerializeField] Slider mutationProbabilitySlider = null;
    [SerializeField] Slider maxMutationSlider = null;
    [SerializeField] Slider numLayersSlider = null;
    [SerializeField] Slider[] layerSliders = null;

    // Start is called before the first frame update
    void Start()
    {
        trackNumSlider.value = SimParams.trackNum;
        weatherDropdown.value = SimParams.trackCondition;
        genSizeSlider.value = SimParams.genSize;
        numSpeciesSlider.value = SimParams.numSpecies;
        gensBetweenForkSlider.value = SimParams.gensBetweenFork;

        mutationProbabilitySlider.value = SimParams.mutationProbability * 100f;
        maxMutationSlider.value = SimParams.maxMutation;
        numLayersSlider.value = SimParams.layerSizes.Length;
        for (int i = 0; i < SimParams.layerSizes.Length; i++)
        {
            layerSliders[i].value = SimParams.layerSizes[i];
        }

        UpdateNumLayers();
    }

    public void StartSim()
    {
        //Save parameters to SimParams class to later be loaded once the scene loads:
        SimParams.trackNum = (int)trackNumSlider.value;
        SimParams.trackCondition = weatherDropdown.value;
        SimParams.genSize = (int)genSizeSlider.value;
        SimParams.numSpecies = (int)numSpeciesSlider.value;
        SimParams.gensBetweenFork = (int)gensBetweenForkSlider.value;

        SimParams.mutationProbability = mutationProbabilitySlider.value / 100f;
        SimParams.maxMutation = maxMutationSlider.value;
        SimParams.layerSizes = new int[(int)numLayersSlider.value];
        for (int i = 0; i < SimParams.layerSizes.Length; i++)
        {
            SimParams.layerSizes[i] = (int)layerSliders[i].value;
        }

        //Load simulation environment:
        SceneManager.LoadScene("Simulation");
    }

    public void UpdateNumLayers()
    {
        for (int i = 0; i < layerSliders.Length; i++)
        {
            //Activate sliders for each layer used, deactivate others:
            if (i < (int)numLayersSlider.value)
            {
                layerSliders[i].transform.parent.gameObject.SetActive(true);
            }
            else
            {
                layerSliders[i].transform.parent.gameObject.SetActive(false);
            }
        }
    }
}
