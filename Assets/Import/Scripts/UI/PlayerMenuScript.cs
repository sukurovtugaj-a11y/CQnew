using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenuScript : MonoBehaviour
{
    public GameObject MenuPanel;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        MenuPanel.SetActive(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
    public void ResumeGame()
    {
        isPaused = false;
        MenuPanel.SetActive(false);
        Time.timeScale = 1;
    }
}
