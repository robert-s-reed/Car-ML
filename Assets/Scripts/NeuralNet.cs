using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class NeuralNet
{
    public class NNLayer
    {
        public class Neuron
        {
            public class Connection
            {
                public Neuron connectedNeuron;
                public float weight;

                //initialises connection and assigns a random weight:
                public Connection(Neuron _connectedNeuron)
                {
                    connectedNeuron = _connectedNeuron;
                    weight = Random.Range(-maxRandomWeight, maxRandomWeight);
                }
            }

            NNLayer prevLayer;
            public float bias;
            public Connection[] connections;
            public float value;

            public Neuron()
            {
                connections = new Connection[0];
            }

            public Neuron (int layerIndex, NNLayer[] layers)
            {
                prevLayer = layers[layerIndex - 1];
                bias = Random.Range(-maxRandomBias, maxRandomBias);

                //create connections (with random weights) to each of the neurons in the previous layer:
                connections = new Connection[prevLayer.neurons.Length];
                for (int i = 0; i < connections.Length; i++)
                {
                    connections[i] = new Connection(prevLayer.neurons[i]);
                }
            }

            public void UpdateValue()
            {
                //calculate value of neuron using previous neurons:
                value = 0;
                for (int i = 0; i < prevLayer.neurons.Length; i++)
                {
                    value += prevLayer.neurons[i].value * connections[i].weight;
                }
                value += bias;
                value = 1f / (1 + Mathf.Exp(-value)); //activation (sigmoid) function
            }
        }

        public Neuron[] neurons;

        //creates neurons array and calls each neuron's initialisation method:
        public NNLayer (int layerIndex, NNLayer[] layers)
        {
            neurons = new Neuron[layerSizes[layerIndex]];
            for (int i = 0; i < neurons.Length; i++)
            {
                if (layerIndex == 0)
                {
                    //doesn't create connections for input layer as there are no previous layers for neurons to be connected to:
                    neurons[i] = new Neuron();
                }
                else
                {
                    neurons[i] = new Neuron(layerIndex, layers);
                }
            }
        }

        //update the value of each neuron in the layer
        public void UpdateValues()
        {
            for (int i = 0; i < neurons.Length; i++)
            {
                neurons[i].UpdateValue();
            }
        }
    }

    public static int[] layerSizes;
    public static float maxRandomBias = 2f;
    public static float maxRandomWeight = 5f;

    public NNLayer[] layers;

    public float[] GetDNAArray()
    {
        //add all dna values to a DNA HashSet by iterating through each neuron
        HashSet<float> dna = new HashSet<float>();
        for (int i = 1; i < layers.Length; i++)
        {
            NNLayer layer = layers[i];
            for (int j = 0; j < layer.neurons.Length; j++)
            {
                NNLayer.Neuron neuron = layer.neurons[j];
                for (int k = 0; k < neuron.connections.Length; k++)
                {
                    dna.Add(neuron.connections[k].weight);
                }
                dna.Add(neuron.bias);
            }
        }

        //converts hashset to array and return:
        float[] dnaArray = new float[dna.Count];
        dna.CopyTo(dnaArray);
        return dnaArray;
    }

    //update neural network input values (which must be previously normalised between 0 and 1) and return the new outputs:
    public float[] Calculate(float[] inputs)
    {
        //set new values for input layer:
        for (int i = 0; i < inputs.Length; i++)
        {
            layers[0].neurons[i].value = inputs[i];
        }
        
        //update each of the remaining layers:
        for (int i = 1; i < layers.Length; i++)
        {
            layers[i].UpdateValues();
        }

        //get and return outputs:
        NNLayer outputLayer = layers[layers.Length - 1];
        float[] outputs = new float[outputLayer.neurons.Length];
        for (int i = 0; i < outputs.Length; i++)
        {
            outputs[i] = outputLayer.neurons[i].value;
        }
        return outputs;
    }

    //create neural network and calls the random initialisation method for each layer:
    public void Initialise()
    {
        //Load and initialise layer sizes from SimParams:
        layerSizes = new int[SimParams.layerSizes.Length + 2];
        layerSizes[0] = 5;
        for (int i = 1; i < layerSizes.Length - 1; i++)
        {
            layerSizes[i] = SimParams.layerSizes[i - 1];
        }
        layerSizes[layerSizes.Length - 1] = 2;

        layers = new NNLayer[layerSizes.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new NNLayer(i, layers);
        }
    }

    public void ImportDNA(float[] dna)
    {
        //Load DNA into class structure from DNA float array:
        int dnaIndex = 0;
        for (int i = 1; i < layers.Length; i++)
        {
            NNLayer layer = layers[i];
            for (int j = 0; j < layer.neurons.Length; j++)
            {
                NNLayer.Neuron neuron = layer.neurons[j];
                for (int k = 0; k < neuron.connections.Length; k++)
                {
                    neuron.connections[k].weight = dna[dnaIndex];
                    dnaIndex++;
                }
                neuron.bias = dna[dnaIndex];
                dnaIndex++;
            }
        }
    }

    public void Mutate(float[] parentDNA)
    {
        float[] newDNA = new float[parentDNA.Length];
        for (int i = 0; i < parentDNA.Length; i++)
        {
            newDNA[i] = parentDNA[i];
            //Mutate from parent DNA if a random number is within the probability:
            if (Random.Range(0f, 1f) <= SimParams.mutationProbability)
            {
                newDNA[i] += Random.Range(-SimParams.maxMutation, SimParams.maxMutation);
            }
        }
        ImportDNA(newDNA);
    }
}
