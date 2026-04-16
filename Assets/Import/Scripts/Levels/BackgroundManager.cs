using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundManager : MonoBehaviour
{
    public enum BGType { Color, Texture, Sprite, Skybox }

    [Header("Настройки фона")]
    public BGType backgroundType = BGType.Color;

    [Header("Цвет (если Color)")]
    public Color backgroundColor = Color.black;

    [Header("Текстура (если Texture)")]
    public Texture2D backgroundTexture;
    public float textureScale = 10f;

    [Header("Спрайт (если Sprite)")]
    public Sprite backgroundSprite;

    [Header("Скайбокс (если Skybox)")]
    public Material skyboxMaterial;

    private Camera mainCamera;
    private GameObject bgObject;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();

        ApplyBackground();
    }

    public void ApplyBackground()
    {
        if (bgObject != null) Destroy(bgObject);

        if (mainCamera == null) return;

        switch (backgroundType)
        {
            case BGType.Color:
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = backgroundColor;
                RenderSettings.skybox = null;
                break;

            case BGType.Texture:
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = Color.black;
                if (backgroundTexture != null) CreateTextureBG();
                break;

            case BGType.Sprite:
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = Color.black;
                if (backgroundSprite != null) CreateSpriteBG();
                break;

            case BGType.Skybox:
                mainCamera.clearFlags = CameraClearFlags.Skybox;
                if (skyboxMaterial != null) RenderSettings.skybox = skyboxMaterial;
                break;
        }
    }

    void CreateTextureBG()
    {
        bgObject = new GameObject("Background");
        bgObject.transform.position = new Vector3(0, 0, 10);
        bgObject.transform.localScale = Vector3.one * textureScale;
        SpriteRenderer sr = bgObject.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(backgroundTexture, new Rect(0, 0, backgroundTexture.width, backgroundTexture.height), new Vector2(0.5f, 0.5f));
        sr.sortingOrder = -10;
    }

    void CreateSpriteBG()
    {
        bgObject = new GameObject("Background");
        bgObject.transform.position = new Vector3(0, 0, 10);
        bgObject.transform.localScale = Vector3.one;
        SpriteRenderer sr = bgObject.AddComponent<SpriteRenderer>();
        sr.sprite = backgroundSprite;
        sr.sortingOrder = -10;
    }
}