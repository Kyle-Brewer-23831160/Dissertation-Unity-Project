using System.Collections;
using UnityEngine;

public class CreateDatabase : MonoBehaviour
{
    public void InitiateCreation()
    {
        StartCoroutine(CreateDBAndTable("http://localhost/Unity%20Scripts/CreateDatabase.php"));
    }

    private IEnumerator CreateDBAndTable(string uri)
    {
        WWW Create = new WWW(uri);

        yield return Create;
    }
}
