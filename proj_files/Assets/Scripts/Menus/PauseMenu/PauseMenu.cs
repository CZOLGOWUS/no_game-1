using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject optionsMenuPanel;

    [HideInInspector]
    public static bool isGamePaused = false;


    public void Pause()
    {
        isGamePaused = true;

        pauseMenuPanel.SetActive( true );

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isGamePaused = false;

        optionsMenuPanel.SetActive( false );
        pauseMenuPanel.SetActive( false );

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
