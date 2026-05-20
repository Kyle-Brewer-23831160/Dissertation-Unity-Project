using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Rendering.DebugUI.Table;

public class MatchStruct
{
    public float P1elo, P1level, P1kd,
    P2elo, P2level, P2kd,
    P3elo, P3level, P3kd,
    P4elo, P4level, P4kd,
    P5elo, P5level, P5kd,
    P6elo, P6level, P6kd,
    P7elo, P7level, P7kd,
    P8elo, P8level, P8kd,
    P9elo, P9level, P9kd,
    P10elo, P10level, P10kd,
    P11elo, P11level, P11kd,
    P12elo, P12level, P12kd,
    fairness = 0;
}

public class FalseDataGen : MonoBehaviour
{
    public NetworkPrePrep PrePrep;
    private StreamWriter Writer;
    private float T1Power;
    private float T2Power;
    private int FileCount;
    public List<Player> FullPlayerList = new List<Player>();
    public List<MatchStruct> MatchList = new List<MatchStruct>();

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

            NormalizedElo = Mathf.Round(NormalizedElo * 10000.0f) / 10000.0f;
            NormalizedLevel = Mathf.Round(NormalizedLevel * 10000.0f) / 10000.0f;
            NormalizedKD = Mathf.Round(NormalizedKD * 10000.0f) / 10000.0f;

            row += $"{NormalizedElo},{NormalizedLevel},{NormalizedKD},";
        }

        // Add Team 2 stats
        foreach (Player p in teamB)
        {
            float NormalizedElo = Normalize(p.PlayerElo, 0, 5000);
            float NormalizedLevel = Normalize(p.level, 0, 500);
            float NormalizedKD = Mathf.Clamp01(p.KDR / 5);

            NormalizedElo = Mathf.Round(NormalizedElo * 10000.0f) / 10000.0f;
            NormalizedLevel = Mathf.Round(NormalizedLevel * 10000.0f) / 10000.0f;
            NormalizedKD = Mathf.Round(NormalizedKD * 10000.0f) / 10000.0f;

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
        float fairnessTarget = Mathf.Clamp01(1.0f - (StrengthGap / 2800f)); //make lower if all games are too perfect, make higher if games are constant stomps

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

        for (int i = 0; i < 3600; i++) //generate 3000 values
        {
            yield return PrePrep.GetPoolFromDatabase(FullPlayerList[rand].UserName, FullPlayerList);
            PrePrep.GetAndRandomise(); //randomise teams
            T1Power = PrePrep.CalculateTeamStrength(PrePrep.Team1); //calculate powers
            T2Power = PrePrep.CalculateTeamStrength(PrePrep.Team2);
            float fairness = CalculateMatchFairness(T1Power, T2Power);


            if (i < 1000)
            {
                if(fairness > 0 && fairness < 0.33)
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
                if (fairness >= 0.66 && fairness <= 0.90)
                {
                    SaveMatchToCSV(PrePrep.Team1, PrePrep.Team2, CalculateMatchFairness(T1Power, T2Power)); //save results
                }
                else
                {
                    i--;
                }
            }
            else if (i >= 3000 && i < 3600)
            {
                if (fairness >= 0.90 && fairness <= 1.0)
                {
                    SaveMatchToCSV(PrePrep.Team1, PrePrep.Team2, CalculateMatchFairness(T1Power, T2Power)); //save results
                }
                else
                {
                    i--;
                }
            }

            if (i == 2999)
            {
                //////////////////////////////////////
                //// REMOVE NEAR DUPLICATE VALUES ////
                //////////////////////////////////////

                StreamReader read = new StreamReader(Application.dataPath + "/Resources/FalseData/MatchData" + FileCount + ".csv");//reads most recent entry
                string file = read.ReadToEnd();
                read.Close();

                string[] Lines = file.Split("\n"[0]);

                for (int k = 1; k < Lines.Length; k++)
                {
                    string[] SinglePart = Lines[k].Split(","[0]);

                    if (SinglePart.Length != 37) continue; //filter entires where colums are missing

                    //Parse the current line data into temporary variables
                    MatchStruct match = new MatchStruct();
                    match.P1elo = float.Parse(SinglePart[0]);
                    match.P1level = float.Parse(SinglePart[1]);
                    match.P1kd = float.Parse(SinglePart[2]);
                    match.P2elo = float.Parse(SinglePart[3]);
                    match.P2level = float.Parse(SinglePart[4]);
                    match.P2kd = float.Parse(SinglePart[5]);
                    match.P3elo = float.Parse(SinglePart[6]);
                    match.P3level = float.Parse(SinglePart[7]);
                    match.P3kd = float.Parse(SinglePart[8]);
                    match.P4elo = float.Parse(SinglePart[9]);
                    match.P4level = float.Parse(SinglePart[10]);
                    match.P4kd = float.Parse(SinglePart[11]);
                    match.P5elo = float.Parse(SinglePart[12]);
                    match.P5level = float.Parse(SinglePart[13]);
                    match.P5kd = float.Parse(SinglePart[14]);
                    match.P6elo = float.Parse(SinglePart[15]);
                    match.P6level = float.Parse(SinglePart[16]);
                    match.P6kd = float.Parse(SinglePart[17]);
                    match.P7elo = float.Parse(SinglePart[18]);
                    match.P7level = float.Parse(SinglePart[19]);
                    match.P7kd = float.Parse(SinglePart[20]);
                    match.P8elo = float.Parse(SinglePart[21]);
                    match.P8level = float.Parse(SinglePart[22]);
                    match.P8kd = float.Parse(SinglePart[23]);
                    match.P9elo = float.Parse(SinglePart[24]);
                    match.P9level = float.Parse(SinglePart[25]);
                    match.P9kd = float.Parse(SinglePart[26]);
                    match.P10elo = float.Parse(SinglePart[27]);
                    match.P10level = float.Parse(SinglePart[28]);
                    match.P10kd = float.Parse(SinglePart[29]);
                    match.P11elo = float.Parse(SinglePart[30]);
                    match.P11level = float.Parse(SinglePart[31]);
                    match.P11kd = float.Parse(SinglePart[32]);
                    match.P12elo = float.Parse(SinglePart[33]);
                    match.P12level = float.Parse(SinglePart[34]);
                    match.P12kd = float.Parse(SinglePart[35]);
                    match.fairness = float.Parse(SinglePart[36]);

                    // 2. Check against existing data
                    bool isDuplicate = false;
                    float threshold = 0.22f; // Adjust this value to define "too similar"

                    for (int j = 0; j < MatchList.Count; j++)
                    {
                        // Compare every column
                        if (Mathf.Abs(MatchList[j].P1elo - match.P1elo) < threshold &&
                            Mathf.Abs(MatchList[j].P1level - match.P1level) < threshold &&
                            Mathf.Abs(MatchList[j].P1kd - match.P1kd) < threshold &&

                            Mathf.Abs(MatchList[j].P2elo - match.P2elo) < threshold &&
                            Mathf.Abs(MatchList[j].P2level - match.P2level) < threshold &&
                            Mathf.Abs(MatchList[j].P2kd - match.P2kd) < threshold &&

                            Mathf.Abs(MatchList[j].P3elo - match.P3elo) < threshold &&
                            Mathf.Abs(MatchList[j].P3level - match.P3level) < threshold &&
                            Mathf.Abs(MatchList[j].P3kd - match.P3kd) < threshold &&

                            Mathf.Abs(MatchList[j].P4elo - match.P4elo) < threshold &&
                            Mathf.Abs(MatchList[j].P4level - match.P4level) < threshold &&
                            Mathf.Abs(MatchList[j].P4kd - match.P4kd) < threshold &&

                            Mathf.Abs(MatchList[j].P5elo - match.P5elo) < threshold &&
                            Mathf.Abs(MatchList[j].P5level - match.P5level) < threshold &&
                            Mathf.Abs(MatchList[j].P5kd - match.P5kd) < threshold &&

                            Mathf.Abs(MatchList[j].P6elo - match.P6elo) < threshold &&
                            Mathf.Abs(MatchList[j].P6level - match.P6level) < threshold &&
                            Mathf.Abs(MatchList[j].P6kd - match.P6kd) < threshold &&

                            Mathf.Abs(MatchList[j].P7elo - match.P7elo) < threshold &&
                            Mathf.Abs(MatchList[j].P7level - match.P7level) < threshold &&
                            Mathf.Abs(MatchList[j].P7kd - match.P7kd) < threshold &&

                            Mathf.Abs(MatchList[j].P8elo - match.P8elo) < threshold &&
                            Mathf.Abs(MatchList[j].P8level - match.P8level) < threshold &&
                            Mathf.Abs(MatchList[j].P8kd - match.P8kd) < threshold &&

                            Mathf.Abs(MatchList[j].P9elo - match.P9elo) < threshold &&
                            Mathf.Abs(MatchList[j].P9level - match.P9level) < threshold &&
                            Mathf.Abs(MatchList[j].P9kd - match.P9kd) < threshold &&

                            Mathf.Abs(MatchList[j].P10elo - match.P10elo) < threshold &&
                            Mathf.Abs(MatchList[j].P10level - match.P10level) < threshold &&
                            Mathf.Abs(MatchList[j].P10kd - match.P10kd) < threshold &&

                            Mathf.Abs(MatchList[j].P11elo - match.P11elo) < threshold &&
                            Mathf.Abs(MatchList[j].P11level - match.P11level) < threshold &&
                            Mathf.Abs(MatchList[j].P11kd - match.P11kd) < threshold &&

                            Mathf.Abs(MatchList[j].P12elo - match.P12elo) < threshold &&
                            Mathf.Abs(MatchList[j].P12level - match.P12level) < threshold &&
                            Mathf.Abs(MatchList[j].P12kd - match.P12kd) < threshold &&

                            Mathf.Abs(MatchList[j].fairness - match.fairness) < threshold)

                        {
                            isDuplicate = true;
                            break;
                        }
                    }

                    //Only add if no similar match was found
                    if (!isDuplicate)
                    {
                        print("didnt find dupe");
                        MatchList.Add(match);
                    }
                    else 
                    {
                        print("found dupe ");
                    }
                }

                File.Delete(Application.dataPath + "/Resources/FalseData/MatchData" + FileCount + ".csv"); //delete old CSV (could have duplicates)

                using (StreamWriter sw = new StreamWriter(Application.dataPath + "/Resources/FalseData/MatchData" + FileCount + ".csv"))
                {
                    string headers = "";

                    string[] teams = { "TeamA", "TeamB" };
                    foreach (string team in teams)
                    {
                        for (int j = 1; j <= 6; j++)
                        {
                            headers += $"{team}_P{j}_Elo,{team}_P{j}_Level,{team}_P{j}_K/D,";
                        }
                    }

                    headers += "FairnessScore";

                    sw.WriteLine(headers);

                    string row = "";

                    foreach (MatchStruct match in MatchList)
                    {
                        row = $"{match.P1elo},{match.P1level},{match.P1kd},{match.P2elo},{match.P2level},{match.P2kd},{match.P3elo},{match.P3level},{match.P3kd},{match.P4elo},{match.P4level},{match.P4kd}, {match.P5elo},{match.P5level},{match.P5kd}, {match.P6elo},{match.P6level},{match.P6kd}, {match.P7elo},{match.P7level},{match.P7kd},{match.P8elo},{match.P8level},{match.P8kd},{match.P9elo},{match.P9level},{match.P9kd}, {match.P10elo},{match.P10level},{match.P10kd},{match.P11elo},{match.P11level},{match.P11kd},{match.P12elo},{match.P12level},{match.P12kd},{match.fairness},";
                        sw.WriteLine(row);
                    }

                    print("Trimmed similar Data");
                }
                yield return null;
            }
            yield return null;
        }
    }

    public void RunGnerator() //link data gen to button
    {
        StartCoroutine(GenerateFalseDataLogic());
    }
}
