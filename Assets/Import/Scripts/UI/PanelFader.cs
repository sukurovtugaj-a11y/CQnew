using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Постепенно делает Image прозрачным, затем отключает родительскую панель.
/// Скрипт добавляется на объект с Image.
/// </summary>
public class PanelFader : MonoBehaviour
{
    [Header("Настройки затухания")]
    [Tooltip("Время затухания в секундах")]
    public float fadeDuration = 2f;

    private Image targetImage;
    private GameObject parentPanel;

    void Start()
    {
        targetImage = GetComponent<Image>();
        if (targetImage == null)
        {
            Debug.LogError("[PanelFader] На объекте нет компонента Image!");
            enabled = false;
            return;
        }

        parentPanel = transform.parent != null ? transform.parent.gameObject : gameObject;
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color startColor = targetImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            targetImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        targetImage.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        parentPanel.SetActive(false);

        Debug.Log($"[PanelFader] Панель {parentPanel.name} отключена");
    }
}
