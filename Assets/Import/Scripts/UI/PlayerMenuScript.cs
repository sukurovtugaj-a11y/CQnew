using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMenuScript : MonoBehaviour
{
    public GameObject MenuPanel;
    public PlayerInteraction playerInteraction;

    // === НОВОЕ: Панель смерти ===
    [Header("Death Panel")]
    public GameObject deathPanel;

    // 🔥 Кнопки навигации
    [Header("Navigation Buttons")]
    public GameObject buttonHL;
    public GameObject buttonMN;

    // 🔥 Курсор
    [Header("Custom Cursor for Pause Menu")]
    public Texture2D pauseCursorTexture;
    public Vector2 pauseCursorHotspot = Vector2.zero;

    private bool isPaused = false;
    private GameObject currentOtherPanel;

    public bool IsAnyMenuOpen => isPaused || currentOtherPanel != null;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentOtherPanel != null)
            {
                CloseOtherPanel();
                return;
            }

            if (isPaused)
            {
                ResumeGame();
                return;
            }

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
            Cursor.visible = true;
            ApplyCustomCursor(true);
            if (currentOtherPanel != null)
                CloseOtherPanel();
            UpdateNavigationButtons();
        }
        else
        {
            Time.timeScale = 1;
            Cursor.visible = false;
            ApplyCustomCursor(false);
        }

        if (playerInteraction != null)
            playerInteraction.RefreshInteractIcon();
    }

    // === НОВОЕ: Показать меню смерти ===
    public void ShowDeathPanel()
    {
        Debug.Log("[PlayerMenuScript] ShowDeathPanel вызван!");

        // Закрываем обычную паузу, если открыта
        if (isPaused)
        {
            isPaused = false;
            MenuPanel.SetActive(false);
        }
        if (currentOtherPanel != null)
            CloseOtherPanel();

        // Показываем панель смерти
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            Debug.Log($"[PlayerMenuScript] Панель смерти активирована: {deathPanel.name}");
        }
        else
        {
            Debug.LogError("[PlayerMenuScript] ОШИБКА! Поле deathPanel пустое в Инспекторе!");
        }

        Time.timeScale = 0f;
        Cursor.visible = true;
        ApplyCustomCursor(true);

        if (playerInteraction != null)
            playerInteraction.RefreshInteractIcon();
    }

    // === НОВОЕ: Перезапустить текущую сцену ===
    public void RestartLevel()
    {
        Time.timeScale = 1f;

        // === Сбрасываем здоровье на максимум ===
        SpawnPointManager.LastHealth = 100; // Или player.maxHealth, если нужна ссылка
        Debug.Log($"[PlayerMenuScript] Здоровье сброшено: {SpawnPointManager.LastHealth}");

        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    private void UpdateNavigationButtons()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool isOnHub = currentScene == "MainScene";

        if (buttonHL != null)
            buttonHL.SetActive(!isOnHub);
        if (buttonMN != null)
            buttonMN.SetActive(isOnHub);
    }

    public void GoToHub()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainScene");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenPanel(GameObject panel)
    {
        if (panel == null) return;

        if (panel == MenuPanel)
        {
            if (!isPaused) TogglePause();
            return;
        }

        if (currentOtherPanel != null)
            CloseOtherPanel();

        if (isPaused)
        {
            isPaused = false;
            MenuPanel.SetActive(false);
        }

        currentOtherPanel = panel;
        currentOtherPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.visible = true;
        ApplyCustomCursor(true);

        if (playerInteraction != null)
            playerInteraction.RefreshInteractIcon();
    }

    private void CloseOtherPanel()
    {
        if (currentOtherPanel != null)
        {
            currentOtherPanel.SetActive(false);
            currentOtherPanel = null;

            Time.timeScale = 1f;
            Cursor.visible = false;
            ApplyCustomCursor(false);

            if (playerInteraction != null)
                playerInteraction.RefreshInteractIcon();
        }
    }

    public void CloseCurrentPanel()
    {
        if (currentOtherPanel != null)
            CloseOtherPanel();
        else if (isPaused)
            ResumeGame();
    }

    public void ResumeGame()
    {
        isPaused = false;
        MenuPanel.SetActive(false);
        Time.timeScale = 1;
        Cursor.visible = false;
        ApplyCustomCursor(false);

        if (playerInteraction != null)
            playerInteraction.RefreshInteractIcon();
    }

    private void ApplyCustomCursor(bool enable)
    {
        Texture2D cursorTex = pauseCursorTexture != null ? pauseCursorTexture :
                             (playerInteraction != null ? playerInteraction.defaultCursorTexture : null);
        Vector2 hotspot = pauseCursorTexture != null ? pauseCursorHotspot :
                         (playerInteraction != null ? playerInteraction.cursorHotspot : Vector2.zero);

        if (enable && cursorTex != null)
        {
            Cursor.SetCursor(cursorTex, hotspot, CursorMode.Auto);
        }
        else if (!enable)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}