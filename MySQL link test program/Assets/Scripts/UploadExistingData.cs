using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class UploadExistingData : MonoBehaviour
{
    private StreamReader reader;

    public void StartUploadCoroutine()
    {
        StartCoroutine(TruncateTable());
    }

    private IEnumerator TruncateTable()
    {
        WWW Request = new WWW("http://localhost/Unity%20Scripts/Truncate.php");

        while (!Request.isDone)
        {
            yield return null;
        }

        StartCoroutine(SendNewUserData());
    }

    private IEnumerator SendNewUserData()
    {
        reader = new StreamReader(Application.dataPath + "/Resources/PlayerData/DatabaseStore.csv", true);

        string FileData = reader.ReadToEnd();
        reader.Close();

        string[] rows = FileData.Split("\n"[0]);

        for (int i = 1; i < rows.Length - 1; i++) //register all users in CSV to localhost database
        {
            string[] Parts = rows[i].Split(","[0]);

            string username = Parts[0];
            string password = Parts[1];
            string email = Parts[2];
            int level = int.Parse(Parts[3]);
            int kills = int.Parse(Parts[4]);
            int deaths = int.Parse(Parts[5]);
            int rank = int.Parse(Parts[6]);

            WWWForm form = new WWWForm();
            form.AddField("userlogin", username);
            form.AddField("userpassword", password);
            form.AddField("useremail", email);
            form.AddField("userlevel", level);
            form.AddField("userkills", kills);
            form.AddField("userdeaths", deaths);
            form.AddField("userrank", rank);

            using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/Unity%20Scripts/RegisterUser.php", form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log(www.downloadHandler.text);
                }
            }
        }
    }
}
