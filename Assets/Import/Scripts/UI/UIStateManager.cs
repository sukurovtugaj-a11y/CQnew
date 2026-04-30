using UnityEngine;

/// <summary>
/// Единый менеджер состояний UI. Координирует все меню в игре.
/// </summary>
public class UIStateManager : MonoBehaviour
{
    public static UIStateManager Instance { get; private set; }

    // Ссылки на меню
    public PlayerMenuScript playerMenu;
    public PlayerInteraction playerInteraction;

    // Текущее состояние
    private enum UIMode
    {
        Gameplay,
        PauseMenu,
        InteractionMenu,
        DeathMenu
    }

    private UIMode currentState = UIMode.Gameplay;

    public bool IsAnyMenuOpen => currentState != UIMode.Gameplay;
    public bool IsPaused => currentState == UIMode.PauseMenu;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapePress();
        }
    }

    private void HandleEscapePress()
    {
        switch (currentState)
        {
            case UIMode.Gameplay:
                OpenPauseMenu();
                break;
            case UIMode.InteractionMenu:
                CloseInteractionMenu();
                // НЕ открываем паузу — просто возвращаемся в игру
                break;
            case UIMode.DeathMenu:
                // В панели смерти ESC может быть обработан отдельно, игнорируем здесь
                break;
            case UIMode.PauseMenu:
                ClosePauseMenu();
                break;
        }
    }

    public void RequestInteractionMenu()
    {
        if (currentState == UIMode.PauseMenu) return;
        currentState = UIMode.InteractionMenu;
        Time.timeScale = 0f;
        Cursor.visible = true;
        // Не закрываем панель здесь, это делает PlayerMenuScript.OpenPanel()
    }

    public void CloseInteractionMenu()
    {
        // ПРИНУДИТЕЛЬНО включаем PlayerMenuScript, если он выключен (для головоломок)
        if (playerMenu != null && !playerMenu.enabled)
        {
            playerMenu.enabled = true;
        }

        currentState = UIMode.Gameplay;
        Time.timeScale = 1f;
        Cursor.visible = false;
        
        if (playerMenu != null)
        {
            playerMenu.CloseCurrentPanel();  // Реально закрываем панель интерактивного объекта
        }
    }

    public void OpenPauseMenu()
    {
        currentState = UIMode.PauseMenu;
        Time.timeScale = 0f;
        Cursor.visible = true;
        playerInteraction?.HideInteractIcon();
    }

    public void ClosePauseMenu()
    {
        currentState = UIMode.Gameplay;
        Time.timeScale = 1f;
        Cursor.visible = false;
    }

    public bool CanInteract() => currentState == UIMode.Gameplay;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
