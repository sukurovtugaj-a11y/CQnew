using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Handles the Upgrades button in the Hub UI
/// </summary>
public class TerminalMenu : MonoBehaviour
{
    public GameObject textObject;
    public float displayTime = 3f;
    private Coroutine hideRoutine;

    public void OpenUpgrades()
    {
        var pending = GameProgressManager.Instance?.GetAllPendingUpgradePanels() ?? new List<string>();
        if (pending.Count == 0)
        {
            ShowText();
            return;
        }
        SceneManager.LoadScene("Upgrades");
    }

    public void ShowText()
    {
        if (textObject == null) return;
        if (hideRoutine != null) StopCoroutine(hideRoutine);

        textObject.SetActive(true);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(displayTime);
        if (textObject != null) textObject.SetActive(false);
        hideRoutine = null;
    }
}
