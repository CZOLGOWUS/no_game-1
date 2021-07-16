using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;

    [HideInInspector]
    public static bool isGamePaused = false;


    void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {

            if(isGamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

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
}
