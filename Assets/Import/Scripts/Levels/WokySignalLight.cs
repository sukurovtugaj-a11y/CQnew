using UnityEngine;

public class WokySignalLight : MonoBehaviour
{
    [Header("Light")]
    public UnityEngine.Rendering.Universal.Light2D targetLight;
    public float minRadius = 7f;
    public float maxRadius = 50f;
    public float blinkSpeed = 2f;

    [Header("Audio")]
    public AudioSource audioSource;

    private float defaultRadius;
    private bool isBlinking = false;

    void Start()
    {
        if (targetLight != null)
            defaultRadius = targetLight.pointLightOuterRadius;

        // Если INTRO уже смотрели - отключить сигнал
        if (PlayerPrefs.GetInt("IntroWatched", 0) == 1)
        {
            if (audioSource != null) audioSource.enabled = false;
            if (targetLight != null) targetLight.enabled = false;
            isBlinking = false;
        }
    }

    void Update()
    {
        if (!isBlinking || targetLight == null) return;

        float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        targetLight.pointLightOuterRadius = Mathf.Lerp(minRadius, maxRadius, t);
    }

    public void StartSignal()
    {
        isBlinking = true;
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
            audioSource.Play();
    }

    public void StopSignal()
    {
        isBlinking = false;
        if (targetLight != null)
            targetLight.pointLightOuterRadius = defaultRadius;
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}