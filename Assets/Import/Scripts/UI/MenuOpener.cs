using UnityEngine;

public class MenuOpener : MonoBehaviour
{
    public GameObject menuPanel;
    public PlayerMenuScript playerMenuScript;

    public void OpenMenu()
    {
        if (playerMenuScript != null && menuPanel != null)
            playerMenuScript.OpenPanel(menuPanel);
        else
            Debug.LogWarning($"MenuOpener на {gameObject.name}: не хватает ссылок!", this);
    }
}
