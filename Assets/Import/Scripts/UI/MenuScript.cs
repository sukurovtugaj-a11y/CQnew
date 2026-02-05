using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject PanelButton;
    public GameObject PanelButtonContinue;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        PanelButton.SetActive(true);
        PanelButtonContinue.SetActive(false);
    }

    public void ShowGameMenu()
    {
        PanelButton.SetActive(false);
        PanelButtonContinue.SetActive(true);
    }
    
    public void NewGame()
    {
        Debug.Log("Starting new game...");
    }

    public void ContinueGame()
    {
        Debug.Log("Continuing game...");
    }

    public void ShowSettings()
    {
        Debug.Log("Opening settings...");
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
