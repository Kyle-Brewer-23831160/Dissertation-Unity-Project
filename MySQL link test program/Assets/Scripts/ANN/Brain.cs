using System;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    [Serializable]
    public class TrainingData
    {
        public List<float> inputs; //inputs for training will be ray data
        public List<float> targets; //targets will be the trun and acceleration value

        public TrainingData(List<float> inputs, List<float> targets) //the hope is when inputs 1,2,3,4,5 are input, a match outcome is output
        {
            this.inputs = inputs;
            this.targets = targets;
        }
    }

    public NeuralNetwork nn;

    public int epochs;
    public float learningRate = 0.1f;


    //first element = input layer, last element = output layer, anything between =  hidden layers
    [SerializeField] private int[] NetworkStruct; //The number in those elements = number of neurons in that layer

    public void Test()
    {
        //for (int i = 0; i < Data.partsA.Count; i++)
        //{
        //    DrivingData.Add(new TrainingData(new List<float> { Data.partsA[i], Data.partsB[i], Data.partsC[i], Data.partsD[i], Data.partsE[i] }, new List<float> { Data.partsTurn[i], Data.partsAccel[i] }));
        //}

        Train();
    }

    public void Train()
    {
        if (nn == null) { nn = new NeuralNetwork(NetworkStruct, learningRate); }

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            double totalError = 0.0;

          //  for (int i = 0; i < DrivingData.Count; i++)
            {
                // Train on one sample
               // nn.BackPropagate(DrivingData[i].inputs, DrivingData[i].targets);

                // Get the network's current output for that same sample
              //  List<float> result = nn.FeedForward(DrivingData[i].inputs);

                // Add squared error for this sample
              //  for (int j = 0; j < result.Count; j++)
                {
               //     double error = DrivingData[i].targets[j] - result[j];
              //      totalError += error * error;
                }
            }

            Debug.Log("Epoch " + epoch + " | Total Error = " + totalError.ToString("F6"));
        }
    }

    void TestNetwork()
    {
      //  for (int i = 0; i < DrivingData.Count; i++)
        {
      //      List<float> result = nn.FeedForward(DrivingData[i].inputs);
        }
    }
}