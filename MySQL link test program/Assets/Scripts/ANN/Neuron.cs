using System;
using System.Collections.Generic;
using UnityEngine;


public class Neuron
{
    public List<float> weights = new List<float>();
    public float bias;
    public float output;
    public float errorGradient;

    private bool isOutputNeuron;

    public Neuron(int inputCount, bool isOutputNeuron)
    {
        this.isOutputNeuron = isOutputNeuron;

        // Biases for ReLU are standardly initialized to a small positive value (e.g., 0.01) 
        // to prevent "dead neurons" at start, or simply 0.0f.
        bias = isOutputNeuron ? 0.0f : 0.01f; //if is output, 0.0f, else 0.01f

        // adjusted He for leaky ReLU layers (prevents exploding weight)
        // Xavier Initialization for the Sigmoid Output layer
        float standardDeviation = isOutputNeuron ? Mathf.Sqrt(2.0f / (inputCount + 1)) : Mathf.Sqrt(2.0f / ((1.0f + 0.01f * 0.01f) * inputCount));

        for (int i = 0; i < inputCount; i++)
        {
            // Box-Muller transform
            float rand1 = 1.0f - UnityEngine.Random.value;
            while (rand1 == 0.0f)
            {
                rand1 = UnityEngine.Random.value;
            }
            float rand2 = 1.0f - UnityEngine.Random.value;
            while (rand2 == 0.0f)
            {
                rand2 = UnityEngine.Random.value;
            }
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Sin(2.0f * Mathf.PI * rand2);

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
            //Leaky ReLU for all hidden feature processing layers
            output = (sum > 0.0f) ? sum : sum * 0.01f;
        }

        return output;
    }
}