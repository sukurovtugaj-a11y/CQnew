using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("Настройки курсора")]
    [Tooltip("Спрайт кастомного курсора")]
    public Sprite cursorSprite;

    [Tooltip("Размер курсора")]
    public float cursorSize = 1f;

    [Tooltip("Смещение курсора относительно мыши")]
    public Vector2 offset = new Vector2(0, 0);

    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private bool isVisible = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;

        GameObject cursorObj = new GameObject("CursorSprite");
        cursorObj.transform.SetParent(transform);
        spriteRenderer = cursorObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = cursorSprite;
        spriteRenderer.sortingOrder = 9999;
        cursorObj.transform.localScale = Vector3.one * cursorSize;

        mainCamera = Camera.main;

        DontDestroyOnLoad(gameObject);
    }

    private void LateUpdate()
    {
        if (isVisible && mainCamera != null)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -mainCamera.transform.position.z;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            transform.position = worldPos + (Vector3)offset;
        }
    }

    public static void Show()
    {
        if (Instance != null) Instance.SetVisible(true);
    }

    public static void Hide()
    {
        if (Instance != null) Instance.SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
    }

    public void SetCursorSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null)
            spriteRenderer.sprite = newSprite;
    }
}