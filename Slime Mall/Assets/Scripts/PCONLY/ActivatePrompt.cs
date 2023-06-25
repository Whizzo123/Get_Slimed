using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatePrompt : MonoBehaviour
{
    public GameObject emotionResponse;
    public bool onlyEmotion;

    private float TimeToShowEmotion = 6.0f;
    private float CurrentTimeShowingEmotion = 0.0f;

    public void ShowEmotion()
    {
        emotionResponse.SetActive(true);
        CurrentTimeShowingEmotion = 0.0f;
    }

    public void HideEmotion()
    {
        emotionResponse.SetActive(false);
        CurrentTimeShowingEmotion = 0.0f;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        //if (emotionResponse != null)
        //{
            
        //}
    }

    void OnTriggerExit2D(Collider2D col)
    {
        //if (emotionResponse != null)
        //{
            
        //}
    }
}
