using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

[Serializable]
    public class TrainingData
{
    public List<float> inputs; //inputs for training player data
    public float target; //target will be the match fairness

    public TrainingData(List<float> inputs, float target) //the hope is when inputs 1,2,3,4 etc are input, a match outcome is output
    {
        this.inputs = inputs;
        this.target = target;
    }
}

public class NetworkPrePrep : MonoBehaviour
{
    public List<Player> PlayerList = new List<Player>();
    public List<Player> Team1 = new List<Player>();
    public List<Player> Team2 = new List<Player>();
    private StreamReader Reader;

    //network stuff
    public NeuralNetwork nn;

    public int epochs;
    public float learningRate = 0.1f;

    //first element = input layer, last element = output layer, anything between =  hidden layers
    [SerializeField] private int[] NetworkStruct; //The number in those elements = number of neurons in that layer

    public List<TrainingData> Data;

    private string FileData;

    private void Start()
    {
        nn = new NeuralNetwork(NetworkStruct, learningRate);
    }

    public void ReadAndTrain()
    {
       ReadCSV();
    }
    //get the player pool
    public IEnumerator GetPoolFromDatabase(string QueuedPlayerName, List<Player> Players)
    {
        PlayerList = new List<Player>();

        foreach (Player p in Players)
        {
            if(p.UserName == QueuedPlayerName)
            {
                PlayerList.Add(p);
            }
        }

        for (int i = 0; i < 30; i++) //6v6 so we need 11 more players
        {
            foreach (Player p in Players) //search through all users
            {
                bool ValidElo = p.PlayerElo >= PlayerList[0].PlayerElo - 400 &&
                                p.PlayerElo <= PlayerList[0].PlayerElo + 400; //if current checking player isnt too low or too high compared to first player

                if (ValidElo) //if current checking player isnt too low or too high compared to first player
                {
                    bool playerExistsinList = false;

                    for (int a = 0; a < PlayerList.Count; a++)
                    {
                        if (PlayerList[a].UserName == p.UserName) //check if we are adding a player that is already in the list
                        {
                            playerExistsinList = true;
                            break;
                        }
                    }

                    if (!playerExistsinList)
                    {
                        PlayerList.Add(p);
                        break;
                    }
                }
            }
        }

        yield return null;  
    }

    //pick 11 other players
    public void GetAndRandomise()
    {
        Team1.Clear();
        Team2.Clear();

        while (Team1.Count < 6)
        {
            int PlayerIndex = UnityEngine.Random.Range(0, PlayerList.Count);
            if (!Team1.Contains(PlayerList[PlayerIndex]))
            {
                Team1.Add(PlayerList[PlayerIndex]);
            }
            else continue;
        }

        while (Team2.Count < 6)
        {
            int PlayerIndex = UnityEngine.Random.Range(0, PlayerList.Count);
            if (!Team2.Contains(PlayerList[PlayerIndex]) && !Team1.Contains(PlayerList[PlayerIndex])) //esure that players on team 1 cant also be on team 2
            {
                Team2.Add(PlayerList[PlayerIndex]);
            }
            else continue;
        }

        //sort teams by highest to lowest elo
        Team1.Sort(SortByElo);
        Team2.Sort(SortByElo);
    }

    static int SortByElo(Player player1, Player player2)
    {
        return player2.PlayerElo.CompareTo(player1.PlayerElo);
    }

    private void ReadCSV()
    {
        string folderPath = Application.dataPath + "/Resources/FalseData/";
        if (!Directory.Exists(folderPath)) return;

        string[] files = Directory.GetFiles(folderPath, "*.csv");

        foreach (string filePath in files)
        {
            using (StreamReader reader = new StreamReader(filePath, true))
            {
                FileData = reader.ReadToEnd();
            }

            string[] rows = FileData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries); //each row will have data from 2 teams as well as fairness result


            for (int i = 1; i < rows.Length - 1; i++) //for each row after header
            {
                string[] rowParts = rows[i].Split(","[0]);  //each part will be a piece of data from each player on either of 2 teams

                List<float> RowData = new List<float>(); //issue is here

                for (int j = 0; j < rowParts.Length; j++)
                {
                    if (float.TryParse(rowParts[j], out float result))
                    {
                        RowData.Add(result);
                    }
                }

                if (RowData.Count == 37) //correct size
                {
                    List<float> inputs = RowData.GetRange(0, 36);
                    float target = RowData[36];

                    if (target > 0.0f)
                    {
                        Data.Add(new TrainingData(inputs, target));
                    }
                }
            }
        }

        StartCoroutine(TrainNetwork()); //train network once data has been read
    }

    //calculate player power and team power
    private float CalculatePlayerStrength(Player player)
    {
        float PlayerStrength = (player.PlayerElo * 0.4f) + (player.KDR * 400f) + (player.level * 1.5f);
        return PlayerStrength;
    }

    public float CalculateTeamStrength(List<Player> Team)
    {
        float TeamPower = 0.0f;

        for (int i = 0; i < Team1.Count; i++)
        {
            TeamPower += CalculatePlayerStrength(Team[i]);
        }

        return TeamPower;
    }

    private IEnumerator TrainNetwork()
    {
        if (File.Exists(Application.dataPath + "/Resources/NetworkOutput/ErrorOverEpoch.csv"))
        {
            File.Delete(Application.dataPath + "/Resources/NetworkOutput/ErrorOverEpoch.csv"); //delete old data
        }

        using (StreamWriter sw = new StreamWriter(Application.dataPath + "/Resources/NetworkOutput/ErrorOverEpoch.csv", true))
        {
            string headers = "Epoch number, Total Error";

            sw.WriteLine(headers);
            sw.Close();
        }

        if (nn == null) { nn = new NeuralNetwork(NetworkStruct, learningRate); }

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            // --- SHUFFLE START ---
            for(int i = 0; i < Data.Count; i++)
            {
               var temp = Data[i];
               int rand = UnityEngine.Random.Range(i, Data.Count);
               Data[i] = Data[rand];
               Data[rand] = temp;
            }

            double totalError = 0.0;
            float totalAccuracy = 0.0f;
            int samplesAnalysed = 0;

            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i].target != 0)
                {
                    samplesAnalysed++;

                    // Get the network's current output for that same sample
                    List<float> result = nn.FeedForward(Data[i].inputs); //the outputs after all inputs have been through the newtwork

                    double error = Data[i].target - (double)result[0];
                    totalError += error * error;

                    float SampleAbsError = Mathf.Abs((float)error);
                    float SampleAccuracy  =  100.0f - ((SampleAbsError / Data[i].target) * 100.0f);

                    totalAccuracy += SampleAccuracy;

                    // Train on one sample
                    nn.BackPropagate(Data[i].inputs, Data[i].target);
                }
            }

            float OverallPercentage = 0.0f;
            OverallPercentage = totalAccuracy / samplesAnalysed;

            Debug.Log("Epoch " + epoch + " | Total Error = " + totalError.ToString("F6") + "| Accuracy: " + OverallPercentage.ToString("F2") + "%");

            using (StreamWriter sw = new StreamWriter(Application.dataPath + "/Resources/NetworkOutput/ErrorOverEpoch.csv", true))
            {
                string row = $"{epoch},{totalError}";
                sw.WriteLine(row);
                sw.Close();
                yield return null;
            }
        }
    }
}
