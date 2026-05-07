using System;
using System.Collections.Generic;
using UnityEngine;

public class Neuron
{
    // Each input going into the neuron has one matching weight.
    public List<float> weights = new List<float>();

    // The bias to be added to the weighted sum.
    public float bias;

    // Stores the final output value produced by this neuron
    public float output;

    // how much this neuron contributed to the error.
    public float errorGradient;

    // When we create a neuron, we tell it how many inputs it will receive.
    public Neuron(int inputCount)
    {
        // Start the bias with a random value between -1 and 1.
        bias =UnityEngine. Random.Range(-1f, 1f);

        // Create one random weight for each input.
        for (int i = 0; i < inputCount; i++)
        {
            weights.Add(UnityEngine.Random.Range(-1f, 1f));
        }
    }

    // This function calculates the neuron's output.
    // It takes the input values, multiplies each one by its weight,
    // adds them together with the bias, then passes the result through sigmoid.
    public float Activate(List<float> inputs)
    {
        // Start the total from the bias.
        float sum = bias;

        // Add input * weight for every input.
        for (int i = 0; i < weights.Count; i++)
        {
            sum += inputs[i] * weights[i];
        }

        // Pass the final sum through the sigmoid activation function.
        // Save the result in output and return it.
        output = (float)Math.Tanh(sum);
        return output;
    }

    // Sigmoid activation function.
    private double Sigmoid(float x)
    {
        return 1.0 / (1.0 + System.Math.Exp(-x));
    }
}