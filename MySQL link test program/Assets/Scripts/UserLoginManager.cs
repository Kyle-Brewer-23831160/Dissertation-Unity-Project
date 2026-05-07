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
        WWWForm form = new WWWForm();
        form.AddField("userlogin", UsernameText.text);
        form.AddField("userpassword", PasswordText.text);
        form.AddField("useremail", EmailText.text);

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
