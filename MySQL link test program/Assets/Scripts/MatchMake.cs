using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MatchMake : MonoBehaviour
{
    private List<Player> FullPlayerList = new List<Player>();
    public List<Player> Team1 = new List<Player>();
    public List<Player> Team2 = new List<Player>();

    public void GetAndRandomise()
    {
        Team1.Clear();
        Team2.Clear();
        FullPlayerList = FindFirstObjectByType<EloRatingSystem>().PlayerList;

        while(Team1.Count < 6)
        {
            int PlayerIndex = Random.Range(0, FullPlayerList.Count);
            if (!Team1.Contains(FullPlayerList[PlayerIndex]))
            {
                Team1.Add(FullPlayerList[PlayerIndex]);
            }
            else continue;
        }


        while (Team2.Count < 6)
        {
            int PlayerIndex = Random.Range(0, FullPlayerList.Count);
            if (!Team2.Contains(FullPlayerList[PlayerIndex]) && !Team1.Contains(FullPlayerList[PlayerIndex])) //esure that players on team 1 cant also be on team 2
            {
                Team2.Add(FullPlayerList[PlayerIndex]);
            }
            else continue;
        }
    }

    private float CalculatePlayerStrength(Player player)
    {
        float PlayerStrength = (player.PlayerElo * 0.4f) + (player.KDR * 400f) + (player.level * 1.5f);
        return PlayerStrength;
    }

    private float CalculateTeamStrength(List<Player> Team)
    {
        float TeamPower = 0.0f;

        for (int i = 0; i < Team1.Count; i++)
        {
            TeamPower += CalculatePlayerStrength(Team[i]);
        }

        return TeamPower;
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
}
