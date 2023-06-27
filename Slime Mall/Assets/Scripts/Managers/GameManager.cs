using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/*
*AUTHOR: Unknown
*EDITORS: Tanapat Somrid
*DATEOFCREATION: dd/mm/yyyy
*DESCRIPTION: Description of file and usage
*/

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Range(0, 360)][SerializeField][Tooltip("Length of initial game session")] float SessionLength = 120;
    [Tooltip("Score of current session")]  int Score = 0;
    [Tooltip("")] int HighScore = 0;
    [Tooltip("How many points you gain per kill")] int KillScoreValue = 10;//SUGGESTION: Move it to the NPC's in case we have varying NPC's that offer different score
    [Tooltip("How much time you gain per kill")] int KillTimeValue = 1;//SUGGESTION: Move it to the NPC's in case we have varying NPC's that offer different time gain

    bool bIsGameFinished = false;
    public bool IsGameFinished
    {
        get
        {
            return bIsGameFinished;
        }
    }
    bool bIsGamePaused = false;
    public bool IsGamePaused
    {
        get
        {
            return bIsGamePaused;
        }
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        //TODO: Replace player prefs and use cloud scoreboard
        HighScore = PlayerPrefs.GetInt("HiScore");
    }

    void Start()
    {
        UI.instance.PauseActive(bIsGamePaused);
    }

    void FixedUpdate()
    {
        if(!bIsGamePaused /*And started*/)
        {
            SessionLength -= Time.deltaTime;
            if(SessionLength <= 0 ) 
            {
                bIsGamePaused = true;
                EndGame();
            }
            UI.instance.UpdatedTimer(Mathf.FloorToInt(SessionLength / 60), Mathf.FloorToInt(SessionLength % 60));
        }
    }

    public void UpdateScore()
    {
        Score += KillScoreValue;
        UI.instance.UpdateScore(Score);
    }

    public void PauseGame()
    {
        if (bIsGamePaused)
        {
            Time.timeScale = 1.0f;
        }
        else
        { 
            Time.timeScale = 0.0f;
        }

        bIsGamePaused = !bIsGamePaused;
        UI.instance.PauseActive(bIsGamePaused);
    }

 
    private void SetHighScore()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HiScore", HighScore);
        }
    }
    public void EndGame()
    {
        if (!bIsGameFinished)
        {
            bIsGameFinished = true;
            SetHighScore();
            FindObjectOfType<PlayerController>().FreezePlayer();
            //Load in end screen ui
            UI.instance.TimeOutEnd(Score, HighScore);
        }
    }

    public void CapturedEndGame()
    {
        if (!bIsGameFinished) 
        { 
            bIsGameFinished = true;
            SetHighScore();
            FindObjectOfType<PlayerController>().FreezePlayer();
            AudioManager.instance.PlaySound("Capture");
            //Load in end screen ui
            UI.instance.CapturedEnd(Score, HighScore);

        }

    }

    public void LoadLevel(int Index)
    {
        StartCoroutine(LoadLevelAsync(Index));
    }

    IEnumerator LoadLevelAsync(int Index)
    {
        //Loads in the background i.e. doesn't stop the game loop
        AsyncOperation Operation = SceneManager.LoadSceneAsync(Index, LoadSceneMode.Single);

        UI.instance.LoadingActive(true);

        while (!Operation.isDone)
        {
            float progress = Mathf.Clamp01(Operation.progress / 0.9f);

            //Percentage loaded            
            UI.instance.UpdateLoadingScreen(progress);

            yield return null;
        }

        UI.instance.LoadingActive(false);
    }

    public void Quit() 
    { 
        Application.Quit(); 
    }
}
