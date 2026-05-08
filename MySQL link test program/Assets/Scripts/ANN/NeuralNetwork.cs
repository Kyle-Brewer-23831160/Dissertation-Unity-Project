using System.Collections.Generic;
using UnityEditor.PackageManager;

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

        // Start from index 1 because index 0 is the input size, not a real layer object
        for (int i = 1; i < layerSizes.Length; i++)
        {
            // Create each layer using:
            // current layer size = number of neurons in this layer
            // previous layer size = number of inputs each neuron receives
            layers.Add(new Layer(layerSizes[i], layerSizes[i - 1]));
        }
    }

    // Pass inputs forward through all layers
    public List<float> FeedForward(List<float> inputs) //a list in this case would be 5 inputs, 1 from each ray
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

            // Error = target - actual output
            float error = target - neuron.output; //this could be an issue;

            // Tanh derivative = 1 - out * out
            neuron.errorGradient = error * (1.0f - (neuron.output * neuron.output));
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

                // Add contribution from each neuron in the next layer
                for (int j = 0; j < nextLayer.neurons.Count; j++)
                {
                    sum += nextLayer.neurons[j].weights[i] * nextLayer.neurons[j].errorGradient;
                }

                neuron.errorGradient = sum * (1.0f - (neuron.output * neuron.output));
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

            // Update each neuron in this layer
            for (int neuronIndex = 0; neuronIndex < currentLayer.neurons.Count; neuronIndex++)
            {
                Neuron neuron = currentLayer.neurons[neuronIndex];

                // Update each weight
                for (int weightIndex = 0; weightIndex < neuron.weights.Count; weightIndex++)
                {
                    neuron.weights[weightIndex] += learningRate * neuron.errorGradient * layerInputs[weightIndex];
                }

                // Update bias
                neuron.bias += learningRate * neuron.errorGradient;
            }
        }
    }
}