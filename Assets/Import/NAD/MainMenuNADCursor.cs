using UnityEngine;

public class MainMenuNADCursor : MonoBehaviour
{
    [Header("Курсор")]
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
            Cursor.visible = true;
        }
    }

    private void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
