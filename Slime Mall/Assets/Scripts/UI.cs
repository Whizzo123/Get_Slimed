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

    public GameObject loadingScreen;
    public Slider slider;
    public TextMeshProUGUI progressText;

    public TextMeshProUGUI timer;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

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

    public void PlayButton(int index)
    {
        mainMenuScreen.SetActive(false);
        GameManager.instance.LoadLevel(index);
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
        GameManager.instance.Quit();
    }
}
