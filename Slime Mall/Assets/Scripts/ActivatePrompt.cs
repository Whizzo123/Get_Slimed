using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatePrompt : MonoBehaviour
{
    public GameObject prompt;
    public GameObject emotionResponse;
    public bool onlyEmotion;

    private float TimeToShowEmotion = 6.0f;
    private float CurrentTimeShowingEmotion = 0.0f;

    void Start()
    {
        if (!prompt) prompt = transform.GetChild(0).gameObject;

        prompt.SetActive(false);
    }

    public void ShowEmotion()
    {
        emotionResponse.SetActive(true);
        prompt.SetActive(false);
        CurrentTimeShowingEmotion = 0.0f;
    }

    public void HideEmotion()
    {
        emotionResponse.SetActive(false);
        prompt.SetActive(false);
        CurrentTimeShowingEmotion = 0.0f;
    }

    private void Update()
    {
        //if(emotionResponse.activeSelf == true)
        //{
        //    if(CurrentTimeShowingEmotion > TimeToShowEmotion)
        //    {
        //        emotionResponse.SetActive(false);
        //    }
        //    else
        //    {
        //        CurrentTimeShowingEmotion += Time.deltaTime;
        //    }
        //}
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (emotionResponse != null)
        {
            if (emotionResponse.activeSelf == false && onlyEmotion == false)
            {
                if (col.CompareTag("PlayerArea"))
                {
                    prompt.SetActive(true);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (emotionResponse != null)
        {
            if (emotionResponse.activeSelf == false && onlyEmotion == false)
            {
                if (col.CompareTag("PlayerArea"))
                {
                    prompt.SetActive(false);
                }
            }
        }
    }
}
