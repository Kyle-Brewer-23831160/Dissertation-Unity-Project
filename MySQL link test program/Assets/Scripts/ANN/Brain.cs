using System;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    [Serializable]
    public class TrainingData
    {
        public List<float> inputs; //inputs for training player data
        public float target; //target will be the match fairness

        public TrainingData(List<float> inputs, float target) //the hope is when inputs 1,2,3,4,5 are input, a match outcome is output
        {
            this.inputs = inputs;
            this.target = target;
        }
    }

    public NeuralNetwork nn;

    public int epochs;
    public float learningRate = 0.1f;


    //first element = input layer, last element = output layer, anything between =  hidden layers
    [SerializeField] private int[] NetworkStruct; //The number in those elements = number of neurons in that layer
}