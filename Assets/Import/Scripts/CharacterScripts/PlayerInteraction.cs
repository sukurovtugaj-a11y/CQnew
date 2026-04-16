using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interact Icon")]
    public GameObject interactIcon;

    [Header("Settings")]
    public string interactKey = "r";

    [Header("UI Manager")]
    public PlayerMenuScript menuScript;

    [Header("Custom Cursor")]
    public Texture2D defaultCursorTexture;
    public Vector2 cursorHotspot = Vector2.zero;

    private InterativeController currentInteractable;
    private bool isCustomCursorActive = false;

    private void Awake()
    {
        if (interactIcon == null)
            interactIcon = GameObject.Find("InteractHint");

        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    private void Update()
    {
        if (menuScript != null && menuScript.IsAnyMenuOpen)
            return;

        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    public void ShowInteractIcon(InterativeController interactable)
    {
        currentInteractable = interactable;
        if (menuScript != null && menuScript.IsAnyMenuOpen) return;
        if (interactIcon != null) interactIcon.SetActive(true);
        SetCustomCursor(true);
    }

    public void HideInteractIcon()
    {
        currentInteractable = null;
        if (interactIcon != null) interactIcon.SetActive(false);
        SetCustomCursor(false);
    }

    public void RefreshInteractIcon()
    {
        bool canShow = currentInteractable != null &&
                      (menuScript == null || !menuScript.IsAnyMenuOpen);
        if (interactIcon != null) interactIcon.SetActive(canShow);
        SetCustomCursor(canShow);
    }

    private void SetCustomCursor(bool enable)
    {
        // 🔥 Меняем курсор ТОЛЬКО если меню НЕ открыто (чтобы не конфликтовать с PlayerMenuScript)
        if (menuScript != null && menuScript.IsAnyMenuOpen) return;

        if (enable && !isCustomCursorActive && defaultCursorTexture != null)
        {
            Cursor.SetCursor(defaultCursorTexture, cursorHotspot, CursorMode.Auto);
            isCustomCursorActive = true;
        }
        else if (!enable && isCustomCursorActive)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            isCustomCursorActive = false;
        }
    }

    private void Interact() => currentInteractable?.Do();
    public InterativeController GetCurrentInteractable() => currentInteractable;
}
