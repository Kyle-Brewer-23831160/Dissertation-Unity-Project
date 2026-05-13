using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class RandomiseAndJudge : MonoBehaviour
{
    public string PlayerName;
    [SerializeField] private NetworkPrePrep PrePrep;
    [SerializeField]
    private TextMeshProUGUI Team1Player1, Team1Player2, Team1Player3, Team1Player4, Team1Player5, Team1Player6,
                            Team2Player1, Team2Player2, Team2Player3, Team2Player4, Team2Player5, Team2Player6;

    List<float> playerdata = new List<float>();

    StreamWriter writer;

    int i = 0;

    private void Start()
    {
        writer = new StreamWriter(Application.dataPath + "/Resources/NetworkOutput/NetworkData0.csv", true);

        string[] NetworkHeader = { "Attempt" , "NetworkOutput", "Real Output" };
        string header = string.Join(",", NetworkHeader);

        writer.WriteLine(header);

        writer.Close();
    }

    public void StartMatchGeneration()
    {
        for (int i = 0; i < 500; i++)
        {
            GenerateMatch();
        }
    }

    private void GenerateMatch()
    {
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

        writer = new StreamWriter(Application.dataPath + "/Resources/NetworkOutput/NetworkData0.csv", true);

        string row = $"{i},{MatchFairness[0]},{realfariness}";

        i += 1;

        writer.WriteLine(row);

        writer.Close();
    }

    public float Normalize(float value, float min, float max) //normalise before passing to the network
    {
        return Mathf.Clamp01((value - min) / (max - min));
    }
}
