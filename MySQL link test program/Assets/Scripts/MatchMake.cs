using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MatchMake : MonoBehaviour
{
    private List<Player> FullPlayerList = new List<Player>();
    public List<Player> Team1 = new List<Player>();
    public List<Player> Team2 = new List<Player>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void GetAndRandomise()
    {
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
}
