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
    public Slider masterVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public Slider soundVolumeSlider;
    public TextMeshProUGUI soundVolumeText;
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeText;

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
        progressText.text = (progress * 100f).ToString();
    }

    public void UpdatedTimer(float minutes, float seconds)
    {
        timer.SetText(string.Format("{0:00}:{1:00}", minutes, seconds));
    }

    public void UpdateScore()
    {
        AudioManager.instance.PlaySound("SlimeConsume");
        int scoreToAdd = int.Parse(score.text) + KillScoreValue;
        score.SetText(scoreToAdd.ToString());
    }

    public void PlayButton(int index)
    {
        AudioManager.instance.PlaySound("UIClick");
        mainMenuScreen.SetActive(false);
        GameManager.Instance.LoadLevel(index);
        if(PlayerController.instance != null) PlayerController.instance.Cleanup();
    }

    public void SettingsButton()
    {
        AudioManager.instance.PlaySound("UIClick");
        settingsScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
    }

    public void MasterVolumeChanged()
    {
        AudioManager.instance.PlaySound("UIClick");
        AudioManager.instance.ChangeMasterVolume(masterVolumeSlider.value / 10);
        masterVolumeText.text = (masterVolumeSlider.value * 10).ToString();
    }

    public void SoundVolumeChanged()
    {
        AudioManager.instance.PlaySound("UIClick");
        AudioManager.instance.ChangeSoundVolume(soundVolumeSlider.value / 10);
        soundVolumeText.text = (soundVolumeSlider.value * 10).ToString();
    }

    public void MusicVolumeChanged()
    {
        AudioManager.instance.PlaySound("UIClick");
        AudioManager.instance.ChangeMusicVolume(musicVolumeSlider.value / 10);
        musicVolumeText.text = (musicVolumeSlider.value * 10).ToString();
    }

    public void CreditsButton()
    {
        AudioManager.instance.PlaySound("UIClick");
        creditsScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
    }

    public void BackButton()
    {
        AudioManager.instance.PlaySound("UIClick");
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
