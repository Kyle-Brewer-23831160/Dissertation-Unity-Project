using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class UserLoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField UsernameText;
    [SerializeField] private TMP_InputField PasswordText;
    [SerializeField] private TMP_InputField EmailText;

    public void RegisterNewUser()
    {
        if (string.IsNullOrWhiteSpace(EmailText.text))
        {
            EmailText.text = UsernameText.text + "@gmail.com";
        }
        if (string.IsNullOrEmpty(PasswordText.text))
        {
            PasswordText.text = UsernameText.text + "Passowrd";
        }

        StartCoroutine(SendNewUserData());
    }

    private IEnumerator SendNewUserData()
    {
        int RandLevel = Random.Range(0, 500); //max level is 500
        int RandKills = Random.Range(RandLevel, RandLevel * 10);
        int RandDeaths = Random.Range(RandLevel / 2, RandLevel * 10);
        int baseElo = 1500;
        float KDR = (float)RandKills / Mathf.Max(1, RandDeaths);

        //each 0.1f away from 1.0f KDR shifts randomised elo by around 50 points
        float KDRmod = (KDR - 1.0f) * 500;

        //assuming that players continiously get better at the game with more levels
        float levelMod = RandLevel * 2f;

        //final calculation with some added noise for more variation
        float elo = baseElo + KDRmod + levelMod + Random.Range(-100, 100);

        //clamp to min/max
        int finalElo = Mathf.Clamp((int)elo, 0, 5000);

        WWWForm form = new WWWForm();
        form.AddField("userlogin", UsernameText.text);
        form.AddField("userpassword", PasswordText.text);
        form.AddField("useremail", EmailText.text);
        form.AddField("userlevel", RandLevel);
        form.AddField("userkills", RandKills);
        form.AddField("userdeaths", RandDeaths);
        form.AddField("userrank", finalElo);

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

        UsernameText.text = string.Empty;
        EmailText.text = string.Empty;
        PasswordText.text = string.Empty;
    }
}
