using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class RandomiseAndJudge : MonoBehaviour
{
    [SerializeField] private NetworkPrePrep PrePrep;
    [SerializeField]
    private TextMeshProUGUI Team1Player1, Team1Player2, Team1Player3, Team1Player4, Team1Player5, Team1Player6,
                            Team2Player1, Team2Player2, Team2Player3, Team2Player4, Team2Player5, Team2Player6;

    List<float> playerdata = new List<float>();

    int index = 0;

    public List<Player> FullPlayerList = new List<Player>();

    private void Start()
    {
        if (File.Exists("Application.dataPath + \"/Resources/NetworkOutput/NetworkData.csv\""))
        {
            File.Delete(Application.dataPath + "/Resources/NetworkOutput/NetworkData.csv");
        }

        using (StreamWriter sw = new StreamWriter(Application.dataPath + "/Resources/NetworkOutput/NetworkData.csv", true))
        {

            string[] NetworkHeader = { "Attempt", "NetworkOutput", "Real Output", "Accuracy" };
            string header = string.Join(",", NetworkHeader);

            sw.WriteLine(header);

            sw.Close();
        }

        StartCoroutine(GetPlayers());
    }

    private IEnumerator GetPlayers() //locally save player data on start, reduces web requests needed later
    {
        //get all players from database
        using (UnityWebRequest www = UnityWebRequest.Get("http://localhost/Unity%20Scripts/GetPlayerPool.php"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string result = www.downloadHandler.text;
                string[] parts = result.Split("/");

                for (int f = 0; f < parts.Length; f += 5)
                {
                    if (parts[f] != string.Empty)
                    {
                        Player player = new Player();
                        player.UserName = parts[f];

                        player.level = int.Parse(parts[f + 1]);
                        player.Kills = int.Parse(parts[f + 2]);
                        player.Deaths = int.Parse(parts[f + 3]);
                        player.KDR = (float)player.Kills / Mathf.Max(1, player.Deaths);
                        string Elo = parts[f + 4];
                        int EloValue;
                        int.TryParse(Elo, out EloValue);
                        player.PlayerElo = EloValue;
                        FullPlayerList.Add(player);
                    }
                }
            }
        }

        FindFirstObjectByType<NetworkPrePrep>().PlayerList = FullPlayerList;
    }

    public void StartMatchGeneration()
    {
        if (File.Exists(Application.dataPath + "/Resources/NetworkOutput/NetworkData.csv"))
        {
            File.Delete(Application.dataPath + "/Resources/NetworkOutput/NetworkData.csv");

            using (StreamWriter sw = new StreamWriter(Application.dataPath + "/Resources/NetworkOutput/NetworkData.csv", true))
            {
                string[] NetworkHeader = { "Attempt", "NetworkOutput", "Real Output", "Accuracy" };
                string header = string.Join(",", NetworkHeader);

                sw.WriteLine(header);

                sw.Close();
            }
        }

        for (int i = 0; i < 500; i++)
        {
          StartCoroutine(GenerateMatch());
           index = 0;
        }
    }

    private IEnumerator GenerateMatch()
    {
        int rand = Random.Range(0, FullPlayerList.Count);

        yield return PrePrep.GetPoolFromDatabase(FullPlayerList[rand].UserName, FullPlayerList);

        PrePrep.GetAndRandomise(); //assign players from the pools to teams

        playerdata.Clear(); //clear Data list

        foreach(Player user in PrePrep.Team1)
        {
            playerdata.Add(Normalize(user.PlayerElo, 0, 5000));
            playerdata.Add(Normalize(user.level, 0, 500));
            playerdata.Add(Mathf.Clamp01(user.KDR / 5));
        }

        foreach (Player user in PrePrep.Team2)
        {
            playerdata.Add(Normalize(user.PlayerElo, 0, 5000));
            playerdata.Add(Normalize(user.level, 0, 500));
            playerdata.Add(Mathf.Clamp01(user.KDR / 5));
        }


        Team1Player1.text = PrePrep.Team1[0].UserName;
        Team1Player2.text = PrePrep.Team1[1].UserName;
        Team1Player3.text = PrePrep.Team1[2].UserName;
        Team1Player4.text = PrePrep.Team1[3].UserName;
        Team1Player5.text = PrePrep.Team1[4].UserName;
        Team1Player6.text = PrePrep.Team1[5].UserName;

        Team2Player1.text = PrePrep.Team2[0].UserName;
        Team2Player2.text = PrePrep.Team2[1].UserName;
        Team2Player3.text = PrePrep.Team2[2].UserName;
        Team2Player4.text = PrePrep.Team2[3].UserName;
        Team2Player5.text = PrePrep.Team2[4].UserName;
        Team2Player6.text = PrePrep.Team2[5].UserName;

        List<float> MatchFairness = PrePrep.nn.FeedForward(playerdata);

        float Team1power = PrePrep.CalculateTeamStrength(PrePrep.Team1);
        float Team2Power = PrePrep.CalculateTeamStrength(PrePrep.Team2);

        float realfariness = FindFirstObjectByType<FalseDataGen>().CalculateMatchFairness(Team1power, Team2Power);

        float Accuracy = (1f - (Mathf.Abs(realfariness - MatchFairness[0]) / Mathf.Abs(realfariness))) * 100;

        Accuracy = Mathf.Round(Accuracy * 100.0f) / 100.0f;
        
        if(Accuracy < 0) Accuracy = 0;

        using (StreamWriter sw = new StreamWriter(Application.dataPath + "/Resources/NetworkOutput/NetworkData.csv", true))
        {
            string row = $"{index},{MatchFairness[0]},{realfariness},{Accuracy}";

            sw.WriteLine(row);

            sw.Close();
        }

        index += 1;

        yield return null;
    }

    public float Normalize(float value, float min, float max) //normalise before passing to the network
    {
        return Mathf.Clamp01((value - min) / (max - min));
    }
}
