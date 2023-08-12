using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI instance;

    public GameObject mainMenuScreen;
    public GameObject pauseScreen;
    public GameObject settingsScreen;
    public GameObject creditsScreen;

    public GameObject guiScreen;
    public GameObject endScreen;

    public GameObject loadingScreen;
    public Slider slider;
    public TextMeshProUGUI progressText;

    public TextMeshProUGUI timer;
    public TextMeshProUGUI score;

    public string capturedMessage;
    public string endTimeMessage;

    [Tooltip("How many points you gain per kill")] int KillScoreValue = 10;//SUGGESTION: Move it to the NPC's in case we have varying NPC's that offer different score


    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        PlayerController.OnAddScore += UpdateScore;
    }

    private void OnDestroy() => PlayerController.OnAddScore -= UpdateScore;

    public void PauseActive(bool b)
    {
        pauseScreen.SetActive(b);
    }

    public void LoadingActive(bool b)
    {
        loadingScreen.SetActive(b);
        slider.value = 0;
        progressText.text = "";
    }

    public void UpdateLoadingScreen(float progress)
    {
        slider.value = progress;
        progressText.text = progress * 100f + "%";
    }

    public void UpdatedTimer(float minutes, float seconds)
    {
        timer.SetText(string.Format("{0:00}:{1:00}", minutes, seconds));
    }

    public void UpdateScore()
    {
        int scoreToAdd = int.Parse(score.text) + KillScoreValue;
        score.SetText(scoreToAdd.ToString());
    }

    public void PlayButton(int index)
    {
        mainMenuScreen.SetActive(false);
        GameManager.Instance.LoadLevel(index);
        if(PlayerController.instance != null) PlayerController.instance.Cleanup();
    }

    public void SettingsButton()
    {
        settingsScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
    }

    public void CreditsButton()
    {
        creditsScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
    }

    public void BackButton()
    {
        mainMenuScreen.SetActive(true);
        settingsScreen.SetActive(false);
        creditsScreen.SetActive(false);
    }

    public void QuitButton()
    {
        GameManager.Instance.Quit();
    }

    public void CapturedEnd(int score, int hiscore)
    {
        guiScreen.SetActive(false);
        endScreen.SetActive(true);
        endScreen.GetComponent<UIAnim_HighScoreScreen>().SetData(score, hiscore, capturedMessage);
        endScreen.GetComponent<UIAnim_HighScoreScreen>().StartAnimations();

    }
    public void TimeOutEnd(int score, int hiscore)
    {
        guiScreen.SetActive(false);
        endScreen.SetActive(true);
        endScreen.GetComponent<UIAnim_HighScoreScreen>().SetData(score, hiscore, endTimeMessage);
        endScreen.GetComponent<UIAnim_HighScoreScreen>().StartAnimations();
    }
}
