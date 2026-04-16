using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenuManager : MonoBehaviour
{
    public static DeathMenuManager Instance { get; private set; }

    [Header("UI Setup")]
    public string panelName = "DeathPanel"; // Имя панели для поиска

    private GameObject cachedPanel;
    private bool initialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Initialize()
    {
        if (initialized) return;

        // Ищем панель по имени в дочерних объектах
        cachedPanel = transform.Find(panelName)?.gameObject;

        // Если не нашли — ищем на всей сцене
        if (cachedPanel == null)
            cachedPanel = GameObject.Find(panelName);

        if (cachedPanel != null)
        {
            cachedPanel.SetActive(false);
            Debug.Log($"[DeathMenu] Инициализировано. Панель: {cachedPanel.name}");
        }
        else
        {
            Debug.LogError($"[DeathMenu] ОШИБКА: панель '{panelName}' не найдена!");
        }

        initialized = true;
    }

    public void Show()
    {
        Initialize(); // Инициализация при первом вызове

        if (cachedPanel == null) return;

        cachedPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log($"[DeathMenu] Показано");
    }

    public void Hide()
    {
        if (cachedPanel == null)
        {
            Debug.LogError("[DeathMenu] Hide() отменён: панель не инициализирована!");
            return;
        }

        cachedPanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log($"[DeathMenu] Скрыто");
    }

    public void Respawn()
    {
        Debug.Log("[DeathMenu] Respawn() вызван");

        //if (MainCharacter.Instance == null)
        {
            Debug.LogError("[DeathMenu] MainCharacter.Instance == null!");
            ReturnToMainMenu();
            return;
        }

        Time.timeScale = 1f;
        //MainCharacter.Instance.RespawnImmediately();
        Hide();

        Debug.Log($"[DeathMenu] Респаун завершён");
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
} 