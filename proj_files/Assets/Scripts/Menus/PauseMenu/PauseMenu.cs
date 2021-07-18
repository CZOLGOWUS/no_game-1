using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;

    [HideInInspector]
    public static bool isGamePaused = false;


    public void Pause()
    {
        isGamePaused = !isGamePaused;
        pauseMenuPanel.SetActive( isGamePaused );
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isGamePaused = !isGamePaused;
        pauseMenuPanel.SetActive( isGamePaused );
        Time.timeScale = 1f;
    }

    public void OnPauseMenu()
    {
        if( isGamePaused )
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }
}
