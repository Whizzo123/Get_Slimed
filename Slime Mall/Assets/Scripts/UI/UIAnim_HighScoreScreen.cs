using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;

public class UIAnim_HighScoreScreen : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text Score;
    [SerializeField] private TMPro.TMP_Text HighScore;
    [SerializeField] private GameObject[] BounceObjects;

    [SerializeField] private float ScaleTime;
    public void SetData(int ScoreNumber, int HighScoreNumber)
    {
        
        Score.text = ScoreNumber.ToString();
        HighScore.text = HighScoreNumber.ToString();
    }
    public void StartAnimations()
    {
        Vector3 SavedScale;
        for (int i = 0; i < BounceObjects.Length; i++)
        {
            SavedScale = BounceObjects[i].transform.localScale;
            BounceObjects[i].transform.localScale = new Vector3(0, 0);
            LeanTween.scale(BounceObjects[i].gameObject, SavedScale, ScaleTime).setDelay(0.1f).setEase(LeanTweenType.easeOutElastic);
        }
        Invoke("PlayLose", 1.0f);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }
    void PlayLose()
    {
        AudioManager.instance.PlaySoundFull("Lose");
    }
}
