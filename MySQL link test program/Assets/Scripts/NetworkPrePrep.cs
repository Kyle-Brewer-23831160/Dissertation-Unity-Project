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

    private bool Line0Skipped;

    public List<Brain.TrainingData> Data;

    private string FileData;

    private void Start()
    {
       ReadCSV();
    }

    //get the player pool
    public IEnumerator GetPoolFromDatabase(string QueuedPlayerName, string uri)
    {
        PlayerList = new List<Player>();

        WWWForm form = new WWWForm();
        form.AddField("username1", QueuedPlayerName);
        WWW download = new WWW(uri, form);

        yield return download;

        string rawResponse = download.text;
        string[] users = rawResponse.Split("/");

        for (int f = 0; f < users.Length; f++)
        {
            if (users[f] == QueuedPlayerName) //find target player and elo
            {
                //print("what");
                Player player = new Player();
                player.UserName = users[f];
                player.level = int.Parse(users[f + 1]);
                player.Kills = int.Parse(users[f + 2]);
                player.Deaths = int.Parse(users[f + 3]);
                player.KDR = (float)player.Kills / Mathf.Max(1, player.Deaths);
                string Elo = users[f + 4];
                int EloValue;
                int.TryParse(Elo, out EloValue);
                player.PlayerElo = EloValue;
                PlayerList.Add(player);
                break;
            }
        }

        for (int i = 0; i < 30; i++) //6v6 so we need 11 more players
        {
            for (int k = 0; k < users.Length; k++) //search through all users
            {
                if (int.TryParse(users[k], out int playerLevl)) //if sucessful, user level is located, their elo is 3 spaces past this
                {
                    k += 3; //adding 3 to index will put us at that users rank value

                    if (int.TryParse(users[k], out int playerElo))
                    {
                        bool ValidElo = playerElo >= PlayerList[0].PlayerElo - 400 &&
                                        playerElo <= PlayerList[0].PlayerElo + 400; //if current checking player isnt too low or too high compared to first player

                        if (ValidElo) //if current checking player isnt too low or too high compared to first player
                        {
                            string Name = users[k - 4];

                            bool playerExistsinList = false;

                            for (int a = 0; a < PlayerList.Count; a++)
                            {
                                if (PlayerList[a].UserName == users[k - 4]) //check if we are adding a player that is already in the list
                                {
                                    playerExistsinList = true;
                                    break;
                                }
                            }

                            if (!playerExistsinList)
                            {
                                Player player = new Player();
                                player.level = int.Parse(users[k - 3]);
                                player.Kills = int.Parse(users[k - 2]);
                                player.Deaths = int.Parse(users[k - 1]);
                                player.KDR = (float)player.Kills / Mathf.Max(1, player.Deaths);
                                player.UserName = users[k - 4];
                                string Elo = users[k];
                                int EloValue;
                                int.TryParse(Elo, out EloValue);
                                player.PlayerElo = EloValue;
                                PlayerList.Add(player);
                                break;
                            }
                        }
                    }
                }
            }
        }
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
        Reader = new StreamReader(Application.dataPath + "/Resources/FalseData/MatchData" + FileCount + ".csv", true);
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
                    RowData.Add(float.Parse(rowParts[j]));
                }
                else continue;
            }

            print (RowData.Count);

            Data.Add(new Brain.TrainingData(new List<float> { RowData[0], RowData[1], RowData[2], //p1 elo, level, kd
                                                              RowData[3], RowData[4], RowData[5], //p2 elo, level, kd
                                                              RowData[6], RowData[7], RowData[8], //p3 elo, level, kd
                                                              RowData[9], RowData[10], RowData[11], //p4 elo, level, kd
                                                              RowData[12], RowData[13], RowData[14], //p5 elo, level, kd
                                                              RowData[15], RowData[16], RowData[17], //p6 elo, level, kd
                                                              RowData[18], RowData[19], RowData[20], //p7 elo, level, kd
                                                              RowData[21], RowData[22], RowData[23], //p8 elo, level, kd
                                                              RowData[24], RowData[25], RowData[26], //p9 elo, level, kd
                                                              RowData[27], RowData[28], RowData[29], //p10 elo, level, kd
                                                              RowData[30], RowData[31], RowData[32], //p11 elo, level, kd
                                                              RowData[33], RowData[34], RowData[35]}, //p12 elo, level, kd
                                                              RowData[36]));
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
            double totalError = 0.0;

            for (int i = 0; i < Data.Count; i++) 
            {
                // Train on one sample
                nn.BackPropagate(Data[i].inputs, Data[i].target);

                // Get the network's current output for that same sample
                List<float> result = nn.FeedForward(Data[i].inputs); //the outputs after all inputs have been through the newtwork

                // Add squared error for this sample
                for (int j = 0; j < result.Count; j++)
                {
                    print(result[j] + " " + i);
                    double error = Data[i].target - result[j];
                    totalError += error * error;
                }
            }

            Debug.Log("Epoch " + epoch + " | Total Error = " + totalError.ToString("F6"));
        }
    }
}
