using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class GPGSAuthenticator : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);       

    }

    private void ProcessAuthentication(SignInStatus Status)
    {
        if(Status == SignInStatus.Success)
        {
            // Continue with Google play
            Debug.Log("Successfully Signed-In");
        }
        else
        {
            // Failed sign-in attempt manual sign-in
            Debug.Log("Failed Sign-In");
            PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
