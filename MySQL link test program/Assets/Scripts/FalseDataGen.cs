using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class FalseDataGen : MonoBehaviour
{
    public NetworkPrePrep PrePrep;
    private StreamWriter Writer;
    private StreamReader Reader;
    private string file;
    private float T1Power;
    private float T2Power;
    private int FileCount;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            CreateCSV();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            T1Power = PrePrep.CalculateTeamStrength(PrePrep.Team1);
            T2Power = PrePrep.CalculateTeamStrength(PrePrep.Team2);
            SaveMatchToCSV(PrePrep.Team1, PrePrep.Team2, CalculateMatchFairness(T1Power, T2Power), FileCount);
        }
    }

    private void CreateCSV()
    {
        FileCount = Directory.GetFiles(Application.dataPath + "/Resources/FalseData").Length;
        if (FileCount > 0) { FileCount = FileCount / 2; } //account for META files
        Writer = new StreamWriter(Application.dataPath + "/Resources/FalseData/MatchData" + (FileCount + 1) + ".csv", true);

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

    public void SaveMatchToCSV(List<Player> teamA, List<Player> teamB, float fairness, int filenum)
    {
        Writer = new StreamWriter(Application.dataPath + "/Resources/FalseData/MatchData" + filenum + ".csv", true);

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

    private float CalculateMatchFairness(float Team1Power, float Team2Power) //THIS WILL BE USED AS THE DATA USED TO TRAIN THE NETWORK
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

        for (int i = 0; i < 999; i++) //generate 1000 values
        {
            StartCoroutine(PrePrep.GetPoolFromDatabase("a", "http://localhost/Unity%20Scripts/GetPlayerPool.php")); //pull players from database
            yield return PrePrep.GetPoolFromDatabase("a", "http://localhost/Unity%20Scripts/GetPlayerPool.php");
            PrePrep.GetAndRandomise(); //randomise teams
            T1Power = PrePrep.CalculateTeamStrength(PrePrep.Team1); //calculate powers
            T2Power = PrePrep.CalculateTeamStrength(PrePrep.Team2);
            SaveMatchToCSV(PrePrep.Team1, PrePrep.Team2, CalculateMatchFairness(T1Power, T2Power), FileCount); //save results
        }
    }

    public void RunGnerator()
    {
        StartCoroutine(GenerateFalseDataLogic());
    }
}
