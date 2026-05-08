using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPrePrep : MonoBehaviour
{
    public List<Player> PlayerList = new List<Player>();
    public List<Player> Team1 = new List<Player>();
    public List<Player> Team2 = new List<Player>();

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
                        bool ValidElo = playerElo >= PlayerList[0].PlayerElo - 250 &&
                                        playerElo <= PlayerList[0].PlayerElo + 250; //if current checking player isnt too low or too high compared to first player

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
}
