using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class NeuralNetwork
{
    public List<Layer> layers = new List<Layer>();

    public float learningRate; //keep between 0 and 1, controls what percentage of error gradient gets used e.g 1 = 100 percent

    // layerSizes is the network shape
    //{2, 2, 1} means:
    // 2 inputs, 2 neurons in hidden layer, 1 output neuron
    public NeuralNetwork(int[] layerSizes, float learningRate)
    {
        this.learningRate = learningRate;

        for (int i = 1; i < layerSizes.Length; i++)
        {
            // Identify if this iteration represents the final output layer
            bool isOutputLayer = (i == layerSizes.Length - 1);
            layers.Add(new Layer(layerSizes[i], layerSizes[i - 1], isOutputLayer));
        }
    }

    // Pass inputs forward through all layers
    public List<float> FeedForward(List<float> inputs) //a list in this case would be 37 inputs
    {
        List<float> currentOutputs = inputs;

        for (int i = 0; i < layers.Count; i++)
        {
            currentOutputs = layers[i].FeedForward(currentOutputs);
        }

        return currentOutputs;
    }

    public void BackPropagate(List<float> inputs, float target)
    {
        // First do a full forward pass so every neuron has its latest output
        List<float> outputs = FeedForward(inputs);

        // -----------------------------
        // 1. Output layer error gradient
        // -----------------------------
        Layer outputLayer = layers[layers.Count - 1];

        for (int i = 0; i < outputLayer.neurons.Count; i++)
        {
            Neuron neuron = outputLayer.neurons[i];

            // Rectified Error: Output - Target combined with -= weight updates fixes the sign error
            float error = neuron.output - target;

            // Sigmoid derivative: out * (1 - out)
            neuron.errorGradient = error * (neuron.output * (1.0f - neuron.output));
        }


        // -----------------------------
        // 2. Hidden layer error gradients
        // -----------------------------
        for (int layerIndex = layers.Count - 2; layerIndex >= 0; layerIndex--)
        {
            Layer currentLayer = layers[layerIndex];
            Layer nextLayer = layers[layerIndex + 1];

            for (int i = 0; i < currentLayer.neurons.Count; i++)
            {
                Neuron neuron = currentLayer.neurons[i];
                float sum = 0.0f;

                for (int j = 0; j < nextLayer.neurons.Count; j++)
                {
                    sum += nextLayer.neurons[j].weights[i] * nextLayer.neurons[j].errorGradient;
                }

                // ReLU derivative: 1 if output > 0, otherwise 0
                float reluDerivative = (neuron.output > 0.0f) ? 1.0f : 0.01f;
                neuron.errorGradient = sum * reluDerivative;
    
            }
        }

        // -----------------------------
        // 3. Update weights and biases
        // -----------------------------
        for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
        {
            Layer currentLayer = layers[layerIndex];

            // Inputs for this layer:
            // first hidden layer uses the original network inputs
            // later layers use outputs from previous layer
            List<float> layerInputs;

            if (layerIndex == 0)
            {
                layerInputs = inputs;
            }
            else
            {
                layerInputs = new List<float>();

                foreach (Neuron prevNeuron in layers[layerIndex - 1].neurons)
                {
                    layerInputs.Add(prevNeuron.output);
                }
            }

            float weightDecay = 0.001f;

            // Update each neuron in this layer
            for (int neuronIndex = 0; neuronIndex < currentLayer.neurons.Count; neuronIndex++)
            {
                Neuron neuron = currentLayer.neurons[neuronIndex];

                // Update each weight
                for (int weightIndex = 0; weightIndex < neuron.weights.Count; weightIndex++)
                {
                    neuron.weights[weightIndex] *= (1.0f - weightDecay * learningRate);

                    neuron.weights[weightIndex] -= learningRate * neuron.errorGradient * layerInputs[weightIndex];
                }

                // Update bias
                neuron.bias += learningRate * neuron.errorGradient;
            }
        }
    }
}