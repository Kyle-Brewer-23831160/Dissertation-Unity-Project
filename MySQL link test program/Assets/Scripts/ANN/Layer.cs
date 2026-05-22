using System.Collections.Generic;

public class Layer
{
    public List<Neuron> neurons = new List<Neuron>();

    // neuronCount = how many neurons this layer should contain
    // inputCountPerNeuron = how many inputs each neuron in this layer receives
    public Layer(int neuronCount, int inputCountPerNeuron, bool isOutputLayer)
    {
        for (int i = 0; i < neuronCount; i++)
        {
            neurons.Add(new Neuron(inputCountPerNeuron, isOutputLayer));
        }
    }

    public List<float> FeedForward(List<float> inputs)
    {
        List<float> outputs = new List<float>();

        for (int i = 0; i < neurons.Count; i++)
        {
            outputs.Add(neurons[i].Activate(inputs)); //feed input list into each neuron in this layer
                                                      //& collect the outputs for the next layers inputs
        }

        return outputs;
    }
}