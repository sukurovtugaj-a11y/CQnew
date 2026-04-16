using UnityEngine;

/// <summary>
/// Скрипт для управления кастомным курсором в главном меню
/// </summary>
public class MainMenuCursor : MonoBehaviour
{
    [Header("Курсор")]
    [Tooltip("Текстура курсора (можно взять из PlayerInteraction.defaultCursorTexture)")]
    public Texture2D cursorTexture;
    
    [Tooltip("Точка привязки курсора (обычно левый нижний угол)")]
    public Vector2 hotspot = Vector2.zero;

    private void OnEnable()
    {
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
            Cursor.visible = true;
        }
    }

    private void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        // Сбрасываем курсор при уничтожении объекта
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = false;
    }
}
