using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeuralNetDiagram : MonoBehaviour
{
    class Neuron
    {
        public struct Connection
        {
            public Image image;

            public Connection(GameObject prefab, Transform parent, Vector2 leftPos, Vector2 rightPos, float neuronDiameter)
            {
                GameObject go = Instantiate(prefab, parent);
                image = go.GetComponent<Image>();

                //calculate and set position, rotation and size:
                RectTransform rectTransform = go.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = (leftPos + rightPos) / 2f;
                rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(rightPos.y - leftPos.y, rightPos.x - leftPos.x) * Mathf.Rad2Deg);
                rectTransform.sizeDelta = new Vector2(Vector2.Distance(leftPos, rightPos) - neuronDiameter, rectTransform.sizeDelta.y);
            }
        }

        public Vector2 pos;
        public Text text;
        public Image image;
        public Connection[] connections;

        public Neuron(GameObject prefab, Transform parent, Vector2 _pos)
        {
            GameObject go = Instantiate(prefab, parent); //instantiate connection prefab
            pos = _pos;
            go.GetComponent<RectTransform>().anchoredPosition = pos;
            text = go.transform.GetChild(0).GetComponent<Text>();
            image = go.GetComponent<Image>();
        }
    }

    const float PADDING = 10f;

    static NeuralNet neuralNet = null;
    static Neuron[][] neurons; //jagged array: [layer][neuron in layer]

    [SerializeField] GameObject neuronPrefab = null;
    [SerializeField] GameObject connectionPrefab = null;

    Vector2 diagramSize;
    float neuronDiameter;
    float gapToEdge; //gap between centre of neuron and edge of diagram

    //calculate neuron relative position in a dimension to scale diagram to designated size:
    float GetNeuronCoord(bool isX, int index, int maxIndex)
    {
        float size;
        if (isX)
        {
            size = diagramSize.x;
        } else
        {
            size = diagramSize.y;
            index = maxIndex - index;
        }
        float usableSize = size - gapToEdge * 2f;
        return usableSize * ((float)index / maxIndex) + gapToEdge;
    }

    // Start is called before the first frame update
    void Start()
    {
        neuronDiameter = neuronPrefab.GetComponent<RectTransform>().sizeDelta.x;
        gapToEdge = neuronDiameter / 2f + PADDING;
        diagramSize = transform.GetComponent<RectTransform>().sizeDelta;

        //Initialise and populate neurons and connections arrays:
        int numLayers = NeuralNet.layerSizes.Length;
        neurons = new Neuron[numLayers][];
        for (int i = 0; i < neurons.Length; i++)
        {
            float xCoord = GetNeuronCoord(true, i, neurons.Length - 1);
            int numNeurons = NeuralNet.layerSizes[i];
            neurons[i] = new Neuron[numNeurons];
            for (int j = 0; j < numNeurons; j++)
            {
                //Add neuron:
                Vector2 neuronPos = new Vector2(xCoord, GetNeuronCoord(false, j, numNeurons - 1));
                Neuron neuron = new Neuron(neuronPrefab, transform, neuronPos);
                neurons[i][j] = neuron;

                //Add connection:
                if (i > 0) //no connections in input layer, so skip
                {
                    neuron.connections = new Neuron.Connection[neurons[i - 1].Length];
                    for (int k = 0; k < neuron.connections.Length; k++)
                    {
                        //Instantiate and save connection:
                        neuron.connections[k] = new Neuron.Connection(connectionPrefab, transform, neurons[i - 1][k].pos, neurons[i][j].pos, neuronDiameter);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (neuralNet != null)
        {
            for (int i = 0; i < neurons.Length; i++)
            {
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    //update neuron value and text:
                    NeuralNet.NNLayer.Neuron neuron = neuralNet.layers[i].neurons[j];
                    neurons[i][j].text.text = neuron.value.ToString(".00");
                }
            }
        }
    }

    public static void SetNeuralNet(NeuralNet net)
    {
        neuralNet = net;
        for (int i = 0; i < neurons.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                //update neuron colour:
                NeuralNet.NNLayer.Neuron neuron = neuralNet.layers[i].neurons[j];
                float normalisedBias = (neuron.bias + NeuralNet.maxRandomBias) / (NeuralNet.maxRandomBias * 2f);
                neurons[i][j].image.color = Color.Lerp(Color.blue, Color.red, normalisedBias);

                //update connections colour:
                for (int k = 0; k < neuron.connections.Length; k++)
                {
                    NeuralNet.NNLayer.Neuron.Connection connection = neuron.connections[k];
                    float normalisedWeight = (connection.weight + NeuralNet.maxRandomWeight) / (NeuralNet.maxRandomWeight * 2f);
                    neurons[i][j].connections[k].image.color = Color.Lerp(Color.blue, Color.red, normalisedWeight);
                }
            }
        }
    }
}
