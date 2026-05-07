using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class EloRatingSystem : MonoBehaviour
{
    [SerializeField] float PlayerA_Elo;
    [SerializeField] float PlayerB_Elo;
    private const int Kfactor = 32;
    [SerializeField] private float PlayerA_Chance;
    [SerializeField] private float PlayerB_Chance;
    [SerializeField] private float Multiplier;
    public List<Player> PlayerList = new List<Player>();
    private float rand;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerA_Chance = GetChance(PlayerB_Elo, PlayerA_Elo);
        PlayerB_Chance = GetChance(PlayerA_Elo, PlayerB_Elo);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayerA_Chance = GetChance(PlayerB_Elo, PlayerA_Elo);
            PlayerB_Chance = GetChance(PlayerA_Elo, PlayerB_Elo);

            Randomiser();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
           StartCoroutine(GetPoolFromDatabase("HeyYa", "http://localhost/Unity%20Scripts/GetPlayerPool.php/"));
        }
    }

    private void Randomiser() //a player with a lower chance may still win
    {
        rand = UnityEngine.Random.Range(0, 1);

        if (PlayerA_Chance > PlayerB_Chance) 
        {
            if (rand > PlayerA_Chance)
            {
                UpdatePlayerElo(true); //player 1 win
            }
            else
            {
                UpdatePlayerElo(false);
            }
        }
        else
        {
            if (rand > PlayerB_Chance)
            {
                UpdatePlayerElo(true); //player 1 win
            }
            else
            {
                UpdatePlayerElo(false);
            }
        }
    }

    private float GetChance(float EnemyElo, float SelfElo)
    {
        float Chance = 1.0f / (1.0f + Mathf.Pow(10.0f, (EnemyElo - SelfElo) / 400.0f)); //elo formula
        return Chance;
    }

    public void UpdatePlayerElo(bool isPlayer1Winner)
    {
        if (isPlayer1Winner)
        {
            PlayerA_Elo += Multiplier * (1 - PlayerA_Chance);
            PlayerB_Elo += Multiplier * (0 - PlayerB_Chance);
        }
        else
        {
            PlayerA_Elo += Multiplier * (0 - PlayerA_Chance);
            PlayerB_Elo += Multiplier * (1 - PlayerB_Chance);
        }
    }

    public void MakeMatch()
    {
        //List<Player> queuePool = GetPoolFromDatabase( ,"http://localhost/Unity%20Scripts/GrabData.php/");
        //define inputs for both teams
        List<float> Inputs = new List<float>();

        for (int i = 0; i < 12; i++) //6v6 so 12 players
        {

        }
    }

    private IEnumerator GetPoolFromDatabase(string QueuedPlayerName, string uri)
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
                string Elo = users[f + 1];
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
                if (int.TryParse(users[k], out int playerElo)) //find target player and elo
                {
                    bool ValidElo = playerElo >= PlayerList[0].PlayerElo - 300 && 
                                    playerElo <= PlayerList[0].PlayerElo + 300; //if current checking player isnt too low or too high compared to first player

                    if (ValidElo) //if current checking player isnt too low or too high compared to first player
                    {
                        string Name = users[k - 1];

                        bool playerExistsinList = false;

                        for (int a = 0; a < PlayerList.Count; a++)
                        {
                            if (PlayerList[a].UserName == users[k - 1]) //check if we are adding a player that is already in the list
                            {
                                playerExistsinList = true;
                                break;
                            }
                        }

                        if (!playerExistsinList)
                        {
                            Player player = new Player();
                            player.UserName = users[k - 1];
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


