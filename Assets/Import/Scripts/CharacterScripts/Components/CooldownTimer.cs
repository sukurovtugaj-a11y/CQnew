using UnityEngine;
using UnityEngine.UI;

public class CooldownTimer : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 15f;       // 15 seconds
    public string prefix = "Q готов через: ";

    private Text timerText;
    private float timeLeft;
    private bool isRunning = false;

    void Awake()
    {
        timerText = GetComponentInChildren<Text>(true);
        if (timerText != null) timerText.text = "";
    }

    void Update()
    {
        if (!isRunning) return;

        timeLeft -= Time.deltaTime;

        if (timerText != null)
            timerText.text = $"{prefix}{timeLeft:F1}";

        if (timeLeft <= 0f)
        {
            isRunning = false;
            timeLeft = 0f;
            gameObject.SetActive(false);
        }
    }

    public void StartTimer()
    {
        timeLeft = duration;
        isRunning = true;
        gameObject.SetActive(true);
    }
}
