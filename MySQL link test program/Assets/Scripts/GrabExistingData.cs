using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GrabExistingData : MonoBehaviour
{
    [SerializeField] private List<Player> DatabaseStore;
    private StreamWriter writer;

    public void StartTheCoroutine()
    {
        StartCoroutine(GetExistingDatabase("http://localhost/Unity%20Scripts/GetExistingDatabase.php"));
    }

    private IEnumerator GetExistingDatabase(string uri)
    {
        DatabaseStore = new List<Player>();

        WWW download = new WWW(uri);

        yield return download;

        string rawResponse = download.text;
        string[] users = rawResponse.Split("/");

        for (int i = 0; i < users.Length; i++)
        {
            Player Current = new Player();

            if(users[i] == string.Empty) { continue; }

            Current.UserName = users[i];
            Current.Password = users[i+1];
            Current.Email = users[i+2];
            Current.level = int.Parse(users[i+3]);
            Current.Kills = int.Parse(users[i+4]);
            Current.Deaths = int.Parse(users[i+5]);
            Current.PlayerElo = int.Parse(users[i+6]);

            DatabaseStore.Add(Current);

            i += 6;
        }

        WriteDatabaseToCSV();
    }

    private void WriteDatabaseToCSV()
    {
        if(File.Exists(Application.dataPath + "/Resources/PlayerData/DatabaseStore.csv"))
        {
            File.Delete(Application.dataPath + "/Resources/PlayerData/DatabaseStore.csv");
        }

        writer = new StreamWriter(Application.dataPath + "/Resources/PlayerData/DatabaseStore.csv", true);

        string[] PlayerHeader = { "Username", "Password", "Email", "level", "kills", "Deaths", "Rank" };
        string header = string.Join(",", PlayerHeader);

        writer.WriteLine(header);


        foreach (Player Player in DatabaseStore)
        {
            string row = $"{Player.UserName},{Player.Password},{Player.Email},{Player.level},{Player.Kills},{Player.Deaths},{Player.PlayerElo}";
            writer.WriteLine(row);
        }

        writer.Close();
    }

}
