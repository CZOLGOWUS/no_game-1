using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : Singleton<PauseMenuManager>
{
    //private PlayerInput uiInput;

    public GameObject pauseMenuPanel;
    public GameObject optionsMenuPanel;

    [HideInInspector]
    public static bool isGamePaused = false;

    private void Awake()
    {
        //uiInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        //uiInput.actions["MenuToggle"].performed += MenuToggle;
    }

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

    public void MenuToggle(InputAction.CallbackContext value)
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