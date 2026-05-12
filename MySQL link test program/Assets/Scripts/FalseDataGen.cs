using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class FalseDataGen : MonoBehaviour
{
    public NetworkPrePrep PrePrep;
    private StreamWriter Writer;
    private float T1Power;
    private float T2Power;
    private int FileCount;
    public List<Player> FullPlayerList = new List<Player>();

    private void CreateCSV()
    {
        FileCount = Directory.GetFiles(Application.dataPath + "/Resources/FalseData").Length;
        if (FileCount > 0) { FileCount = FileCount / 2; } //account for META files
        Writer = new StreamWriter(Application.dataPath + "/Resources/FalseData/MatchData" + (FileCount) + ".csv", true);

        string headers = "";

        // Create headers for Team A (Players 1-6) and Team B (Players 1-6)
        string[] teams = { "TeamA", "TeamB" };
        foreach (string team in teams)
        {
            for (int i = 1; i <= 6; i++)
            {
                headers += $"{team}_P{i}_Elo,{team}_P{i}_Level,{team}_P{i}_K/D,";
            }
        }

        headers += "FairnessScore";

        Writer.WriteLine(headers);

        Writer.Close();
    }

    public void SaveMatchToCSV(List<Player> teamA, List<Player> teamB, float fairness)
    {
        Writer = new StreamWriter(Application.dataPath + "/Resources/FalseData/MatchData" + FileCount + ".csv", true);

        string row = "";

        // Add Team 1 stats
        foreach (Player p in teamA)
        {
            float NormalizedElo = Normalize(p.PlayerElo, 0, 5000);
            float NormalizedLevel = Normalize(p.level, 0, 500);
            float NormalizedKD = Mathf.Clamp01(p.KDR / 5); //rare for any player to have higher than this, so this is the soft cap

            row += $"{NormalizedElo},{NormalizedLevel},{NormalizedKD},";
        }

        // Add Team 2 stats
        foreach (Player p in teamB)
        {
            float NormalizedElo = Normalize(p.PlayerElo, 0, 5000);
            float NormalizedLevel = Normalize(p.level, 0, 500);
            float NormalizedKD = Mathf.Clamp01(p.KDR / 5);

            row += $"{NormalizedElo},{NormalizedLevel},{NormalizedKD},";
        }

        //fairness is the result which the network will use to learn
        row += fairness.ToString();

        Writer.WriteLine(row);

        Writer.Close();
    }

    public float CalculateMatchFairness(float Team1Power, float Team2Power) //THIS WILL BE USED AS THE DATA USED TO TRAIN THE NETWORK
    {
        float StrengthGap = Mathf.Abs(Team1Power - Team2Power);

        // Define Fairness (Target for Backprop)
        // If the gap is small (e.g., < 200), Fairness = 1.0 (Close match)
        // If the gap is huge (e.g., > 2000), Fairness = 0.0 (Stomp)
        float fairnessTarget = Mathf.Clamp01(1.0f - (StrengthGap / 2000f)); //make lower if all games are too perfect, make higher if games are constant stomps

        return fairnessTarget;
    }

    public float Normalize(float value, float min, float max)
    {
        return Mathf.Clamp01((value - min) / (max - min));
    }

    public IEnumerator GenerateFalseDataLogic()
    {
        CreateCSV(); //create CSV file

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

        int rand = Random.Range(0, FullPlayerList.Count);

        for (int i = 0; i < 3000; i++) //generate 1000 values
        {
            yield return PrePrep.GetPoolFromDatabase(FullPlayerList[rand].UserName, FullPlayerList);
            PrePrep.GetAndRandomise(); //randomise teams
            T1Power = PrePrep.CalculateTeamStrength(PrePrep.Team1); //calculate powers
            T2Power = PrePrep.CalculateTeamStrength(PrePrep.Team2);
            SaveMatchToCSV(PrePrep.Team1, PrePrep.Team2, CalculateMatchFairness(T1Power, T2Power)); //save results
        }
    }

    public void RunGnerator()
    {
        StartCoroutine(GenerateFalseDataLogic());
    }
}
