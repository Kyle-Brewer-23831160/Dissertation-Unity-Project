using System.Collections.Generic;

//public class Layer
//{
//    // A list of all neurons in this layer
//    public List<Neuron> neurons = new List<Neuron>();

//    // neuronCount = how many neurons this layer should contain
//    // inputCountPerNeuron = how many inputs each neuron in this layer receives
//    public Layer(int neuronCount, int inputCountPerNeuron)
//    {
//        // Create the required number of neurons
//        for (int i = 0; i < neuronCount; i++)
//        {
//            neurons.Add(new Neuron(inputCountPerNeuron));
//        }
//    }

//    // Feed the same input list into every neuron in this layer
//    // and collect all neuron outputs into one list
//    public List<float> FeedForward(List<float> inputs)
//    {
//        List<float> outputs = new List<float>();

//        for (int i = 0; i < neurons.Count; i++)
//        {
//            outputs.Add(neurons[i].Activate(inputs));
//        }

//        return outputs;
//    }
//}

public class Layer
{
    public List<Neuron> neurons = new List<Neuron>();

    // Added isOutputLayer parameter
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
            outputs.Add(neurons[i].Activate(inputs));
        }

        return outputs;
    }
}