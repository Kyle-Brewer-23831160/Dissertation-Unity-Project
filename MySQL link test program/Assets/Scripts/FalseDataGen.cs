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

    private void Start()
    {
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
            float NormalizedElo = Normalize(p.PlayerElo, 0, 5000); //normalise data to improve network digestability
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
        float fairnessTarget = Mathf.Clamp01(1.0f - (StrengthGap / 2500f)); //make lower if all games are too perfect, make higher if games are constant stomps

        //round to get rid of near meaningless decimal places

        fairnessTarget = Mathf.Round(fairnessTarget * 10000.0f) / 10000.0f;

        return fairnessTarget;
    }

    public float Normalize(float value, float min, float max)
    {
        return Mathf.Clamp01((value - min) / (max - min));
    }

    public IEnumerator GenerateFalseDataLogic() //generates "true" match data using a static algorithm to calculate match fairness
    {
        CreateCSV(); //create CSV file

        int rand = Random.Range(0, FullPlayerList.Count);

        for (int i = 0; i < 3000; i++) //generate 1000 values
        {
            yield return PrePrep.GetPoolFromDatabase(FullPlayerList[rand].UserName, FullPlayerList);
            PrePrep.GetAndRandomise(); //randomise teams
            T1Power = PrePrep.CalculateTeamStrength(PrePrep.Team1); //calculate powers
            T2Power = PrePrep.CalculateTeamStrength(PrePrep.Team2);
            float fairness = CalculateMatchFairness(T1Power, T2Power);

            if(i < 1000)
            {
                if(fairness < 0.33)
                {
                    SaveMatchToCSV(PrePrep.Team1, PrePrep.Team2, CalculateMatchFairness(T1Power, T2Power)); //save results
                }
                else
                {
                    i--;
                }
            }
            else if(i >= 1000 && i < 2000)
            {
                if (fairness >= 0.33 && fairness < 0.66)
                {
                    SaveMatchToCSV(PrePrep.Team1, PrePrep.Team2, CalculateMatchFairness(T1Power, T2Power)); //save results
                }
                else
                {
                    i--;
                }
            }
            else if (i >= 2000 && i < 3000)
            {
                if (fairness >= 0.66 && fairness <= 1)
                {
                    SaveMatchToCSV(PrePrep.Team1, PrePrep.Team2, CalculateMatchFairness(T1Power, T2Power)); //save results
                }
                else
                {
                    i--;
                }
            }
        }
    }

    public void RunGnerator() //link data gen to button
    {
        StartCoroutine(GenerateFalseDataLogic());
    }
}
