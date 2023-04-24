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
    public GameObject capturedScreen;
    public GameObject timeOutScreen;

    public Image sprintBar;
    public float sprintAmount = 3.0f;


    public GameObject loadingScreen;
    public Slider slider;
    public TextMeshProUGUI progressText;

    public TextMeshProUGUI timer;
    public TextMeshProUGUI score;

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

    public void UpdateScore(int sc)
    {
        score.SetText(sc.ToString());
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

    public void CapturedEnd(int score, int hiscore)
    {
        guiScreen.SetActive(false);
        capturedScreen.SetActive(true);
        capturedScreen.GetComponent<UIAnim_HighScoreScreen>().SetData(score, hiscore);
        capturedScreen.GetComponent<UIAnim_HighScoreScreen>().StartAnimations();

    }
    public void TimeOutEnd(int score, int hiscore)
    {
        guiScreen.SetActive(false);
        timeOutScreen.SetActive(true);
        timeOutScreen.GetComponent<UIAnim_HighScoreScreen>().SetData(score, hiscore);
        timeOutScreen.GetComponent<UIAnim_HighScoreScreen>().StartAnimations();
    }

    public void ChangeSprintBar(float amount)
    {
        sprintAmount = amount;
        sprintBar.fillAmount = sprintAmount / 3.0f;
    }
}
