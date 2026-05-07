using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class EloManager : MonoBehaviour
{
    [SerializeField] private string Username1;
    [SerializeField] private string Username2;
    [SerializeField] private List<PlayerInfo> PlayerInformation;

    public void FindRanks()
    {
        StartCoroutine(GetRanks("http://localhost/Unity%20Scripts/GrabData.php/"));
    }

    IEnumerator GetRanks(string uri)
    {
        WWWForm form = new WWWForm();
        form.AddField("username1", Username1);
        form.AddField("username2", Username2);
        WWW download = new WWW(uri, form);

        yield return download;

        string rawResponse = download.text;

        string[] users = rawResponse.Split("/");

        for (int i = 0; i < users.Length; i++)
        {
            if (users[i] == Username1 || users[i] == Username2)
            {
                PlayerInfo info = new PlayerInfo();
                info.Username = users[i];
                string Elo = users[i + 1];
                int EloValue;
                int.TryParse(Elo, out EloValue);
                info.Elo = EloValue;
                PlayerInformation.Add(info);
            }
        }
    }
}

[System.Serializable]
public class PlayerInfo
{
    public string Username;
    public float Elo;
}
