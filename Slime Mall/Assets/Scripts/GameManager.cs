using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Range(0,360)]
    public float sessionLenght = 120;
    public float score = 0;

    bool isGamePaused = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    void Start()
    {
        UI.instance.PauseActive(isGamePaused);
    }

    void FixedUpdate()
    {
        if(!isGamePaused /*And started*/)
        {
            sessionLenght -= Time.deltaTime;
            if(sessionLenght <= 0 ) 
            {
                EndGame();
            }
            UI.instance.UpdatedTimer(Mathf.FloorToInt(sessionLenght / 60), Mathf.FloorToInt(sessionLenght % 60));
        }
    }

    public void PauseGame()
    {
        if (isGamePaused) Time.timeScale = 1.0f;
        else Time.timeScale = 0.0f;

        isGamePaused = !isGamePaused;
        UI.instance.PauseActive(isGamePaused);
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    public void EndGame()
    {

    }

    public void LoadLevel(int index)
    {
        StartCoroutine(LoadLevelAsync(index));
    }

    IEnumerator LoadLevelAsync(int index)
    {
        //Loads in the background i.e. doesn't stop the game loop
        AsyncOperation op = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);

        UI.instance.LoadingActive(true);

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            //Percentage loaded            
            UI.instance.UpdateLoadingScreen(progress);

            yield return null;
        }

        UI.instance.LoadingActive(false);
    }

    public void Quit() { Application.Quit(); }
}
