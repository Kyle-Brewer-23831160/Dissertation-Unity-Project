using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

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

    public List<Brain.TrainingData> Data;

    private string FileData;

    private void Start()
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
            int PlayerIndex = Random.Range(0, PlayerList.Count);
            if (!Team1.Contains(PlayerList[PlayerIndex]))
            {
                Team1.Add(PlayerList[PlayerIndex]);
            }
            else continue;
        }

        while (Team2.Count < 6)
        {
            int PlayerIndex = Random.Range(0, PlayerList.Count);
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
        int FileCount = Directory.GetFiles(Application.dataPath + "/Resources/FalseData").Length;
        if (FileCount > 0) { FileCount = FileCount / 2; } //account for META files
        Reader = new StreamReader(Application.dataPath + "/Resources/FalseData/MatchData0.csv", true);
        FileData = Reader.ReadToEnd();
        Reader.Close();

        string[] rows = FileData.Split("\n"[0]); //each row will have data from 2 teams as well as fairness result

        for (int i = 1; i < rows.Length - 1; i++) //for each row
        {
            string[] rowParts = rows[i].Split(","[0]);  //each part will be a piece of data from each player on either of 2 teams

            List<float> RowData = new List<float>(); //issue is here

            for (int j = 0; j < rowParts.Length; j++)
            {
                bool FormatGuard = float.TryParse(rowParts[j], out float result);
                if (FormatGuard)
                {
                    RowData.Add(result);
                }
                else continue;
            }


            if (RowData.Count >= 37)
            {
                List<float> inputs = RowData.GetRange(0, 36);
                float target = RowData[36];

                Data.Add(new Brain.TrainingData(inputs, target));
            }
        }

        TrainNetwork(); //train network once data has been read
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

    private void TrainNetwork()
    {
        if (nn == null) { nn = new NeuralNetwork(NetworkStruct, learningRate); }

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            // --- SHUFFLE START ---
            for (int i = 0; i < Data.Count; i++)
            {
                var temp = Data[i];
                int randomIndex = Random.Range(i, Data.Count);
                Data[i] = Data[randomIndex];
                Data[randomIndex] = temp;
            }

            double totalError = 0.0;

            for (int i = 0; i < Data.Count; i++) 
            {
                // Get the network's current output for that same sample
                List<float> result = nn.FeedForward(Data[i].inputs); //the outputs after all inputs have been through the newtwork
               
                double error = Data[i].target - (double)result[0];
                totalError += error * error;

                // Train on one sample
                nn.BackPropagate(Data[i].inputs, Data[i].target);
            }

            Debug.Log("Epoch " + epoch + " | Total Error = " + totalError.ToString("F6"));
        }
    }
}
