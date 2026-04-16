using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    [Header("Панели выбора")]
    public GameObject afterTrainPanel;
    public GameObject afterFirstLevelPanel;
    public GameObject afterSecLevelPanel;

    private Queue<string> pendingPanels = new Queue<string>();

    void Start()
    {
        pendingPanels = new Queue<string>(GameProgressManager.Instance?.GetAllPendingUpgradePanels() ?? new List<string>());
        ShowNextPanel();
    }

    void ShowNextPanel()
    {
        afterTrainPanel.SetActive(false);
        afterFirstLevelPanel.SetActive(false);
        afterSecLevelPanel.SetActive(false);

        if (pendingPanels.Count == 0)
        {
            SceneManager.LoadScene("MainScene");
            return;
        }

        string name = pendingPanels.Dequeue();
        if (name == "AfterTrain") afterTrainPanel.SetActive(true);
        else if (name == "AfterFirstLevel") afterFirstLevelPanel.SetActive(true);
        else if (name == "AfterSecLevel") afterSecLevelPanel.SetActive(true);
    }

    void AfterPick()
    {
        StartCoroutine(ShowNextPanelDelayed());
    }

    IEnumerator ShowNextPanelDelayed()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        ShowNextPanel();
    }

    // === Wrappers for MyButton.m_OnClick ===

    public void PickHealth()
    {
        GameProgressManager.Instance.OnUpgradeChosen("train", "health", false);
        AfterPick();
    }

    public void PickDoubleJump()
    {
        GameProgressManager.Instance.OnUpgradeChosen("firstLevel", "doubleJump", false);
        AfterPick();
    }

    public void PickDash()
    {
        GameProgressManager.Instance.OnUpgradeChosen("firstLevel", "dash", false);
        AfterPick();
    }

    public void PickCheckpoint()
    {
        GameProgressManager.Instance.OnUpgradeChosen("secondLevel", "checkpoint", false);
        AfterPick();
    }

    public void PickInvincible()
    {
        GameProgressManager.Instance.OnUpgradeChosen("secondLevel", "invincible", false);
        AfterPick();
    }
}
