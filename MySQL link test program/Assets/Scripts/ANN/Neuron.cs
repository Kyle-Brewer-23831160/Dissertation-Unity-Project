using System;
using System.Collections.Generic;
using UnityEngine;

//public class Neuron
//{
//    // Each input going into the neuron has one matching weight.
//    public List<float> weights = new List<float>();

//    // The bias to be added to the weighted sum.
//    public float bias;

//    // Stores the final output value produced by this neuron
//    public float output;

//    // how much this neuron contributed to the error.
//    public float errorGradient;

//    // When we create a neuron, we tell it how many inputs it will receive.
//    public Neuron(int inputCount)
//    {
//        // Start the bias with a random value between -1 and 1.
//        //bias =UnityEngine. Random.Range(-1f, 1f);

//        bias = 0.0f;

//        float range = Mathf.Sqrt(6.0f / (inputCount + 1)); // Assuming 1 output neuron per layer concept
//        for (int i = 0; i < inputCount; i++)
//        {
//            weights.Add(UnityEngine.Random.Range(-range, range));
//        }
//    }

//    // This function calculates the neuron's output.
//    // It takes the input values, multiplies each one by its weight,
//    // adds them together with the bias, then passes the result through sigmoid.
//    public float Activate(List<float> inputs)
//    {
//        // Start the total from the bias.
//        float sum = bias;

//        // Add input * weight for every input.
//        for (int i = 0; i < weights.Count; i++)
//        {
//            sum += inputs[i] * weights[i];
//        }

//        // Pass the final sum through the sigmoid activation function.
//        // Save the result in output and return it.
//        output = 1.0f / (1.0f + Mathf.Exp(-sum));
//        return output;
//    }
//}

public class Neuron
{
    public List<float> weights = new List<float>();
    public float bias;
    public float output;
    public float errorGradient;

    // Track the type of activation this specific neuron uses
    private bool isOutputNeuron;

    public Neuron(int inputCount, bool isOutputNeuron)
    {
        this.isOutputNeuron = isOutputNeuron;

        // Biases for ReLU are standardly initialized to a small positive value (e.g., 0.01) 
        // to prevent "dead neurons" at start, or simply 0.0f.
        bias = isOutputNeuron ? 0.0f : 0.01f;

        // He/Kaiming Initialization for ReLU layers
        // Glorot/Xavier Initialization for the Sigmoid Output layer
        float standardDeviation = isOutputNeuron
            ? Mathf.Sqrt(2.0f / (inputCount + 1))
            : Mathf.Sqrt(2.0f / inputCount);

        for (int i = 0; i < inputCount; i++)
        {
            // Box-Muller transform to generate random Gaussian/Normal distribution numbers
            float u1 = 1.0f - UnityEngine.Random.value;
            float u2 = 1.0f - UnityEngine.Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);

            weights.Add(randStdNormal * standardDeviation);
        }
    }

    public float Activate(List<float> inputs)
    {
        float sum = bias;

        for (int i = 0; i < weights.Count; i++)
        {
            sum += inputs[i] * weights[i];
        }

        // Conditional activation based on layer position
        if (isOutputNeuron)
        {
            // Sigmoid for final match fairness output (bounds between 0 and 1)
            output = 1.0f / (1.0f + Mathf.Exp(-sum));
        }
        else
        {
            // ReLU for all hidden feature processing layers
            output = (sum > 0.0f) ? sum : sum * 0.01f;
        }

        return output;
    }
}