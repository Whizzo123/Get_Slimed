using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    bool isGamePaused = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void PauseGame()
    {
        if (isGamePaused) Time.timeScale = 1.0f;
        else Time.timeScale = 0.0f;

        isGamePaused = !isGamePaused;
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }
}
