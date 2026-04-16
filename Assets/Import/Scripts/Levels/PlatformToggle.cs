using UnityEngine;
using System.Collections;

public class PlatformToggle : MonoBehaviour
{
    [Header("Настройки")]
    public float tangibleTime = 3f;
    public float intangibleTime = 2f;

    [Header("Лампочки")]
    public GameObject greenLight;
    public GameObject redLight;
    public GameObject platformLight1;
    public GameObject platformLight2;

    private Collider2D col;
    private SpriteRenderer sr;

    void Start()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(PlatformCycle());
    }

    IEnumerator PlatformCycle()
    {
        while (true)
        {
            SetPlatform(true);
            yield return new WaitForSeconds(tangibleTime);

            SetPlatform(false);
            yield return new WaitForSeconds(intangibleTime);
        }
    }

    void SetPlatform(bool isTangible)
    {
        col.enabled = isTangible;
        greenLight.SetActive(isTangible);
        redLight.SetActive(!isTangible);

        if (platformLight1 != null)
            platformLight1.SetActive(isTangible);

        if (platformLight2 != null)
            platformLight2.SetActive(isTangible);

        if (sr != null)
        {
            Color color = sr.color;
            color.a = isTangible ? 1f : 0.5f;
            sr.color = color;
        }
    }
}