using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CodeLine {
    public string id;
    public UnityEngine.UI.InputField input;
}

[System.Serializable]
public class Answer {
    public string id;
    public string correctAnswer;
}

public class PuzzleUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string puzzleId;
    [SerializeField] private List<CodeLine> codeInputs = new();
    [SerializeField] private List<Answer> answers = new();

    [Header("UI Elements")]
    [SerializeField] private GameObject ERpanel;
    [SerializeField] private GameObject OKpanel;
    [SerializeField] private Sprite successBorder;

    [Header("Settings")]
    [SerializeField] private float successDelay = 3f;
    [SerializeField] private AudioSource errorSound;
    [SerializeField] private AudioSource successSound;

    [Header("On Success")]
    public UnityEvent onSuccess;

    private Dictionary<string, string> originalTexts = new();
    private bool solved;

    private void Awake()
    {
        if (OKpanel != null) OKpanel.SetActive(false);
    }

    public void Open()
    {
        if (solved) return;

        // ТВОЙ СПОСОБ: отключаем PlayerMenuScript при открытии головоломки
        var playerMenu = FindObjectOfType<PlayerMenuScript>();
        if (playerMenu != null) playerMenu.enabled = false;

        gameObject.SetActive(true);

        if (originalTexts.Count == 0)
            foreach (var line in codeInputs)
                if (line.input != null) originalTexts[line.id] = line.input.text;

        if (OKpanel != null) OKpanel.SetActive(false);
        if (ERpanel != null) ERpanel.SetActive(true);

        StartCoroutine(DelayPause());
    }

    private IEnumerator DelayPause()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z) && !solved)
            foreach (var kv in originalTexts)
            {
                var line = codeInputs.Find(l => l.id == kv.Key);
                if (line?.input != null) line.input.text = kv.Value;
            }
    }

    public void CheckAnswer()
    {
        if (solved) return;

        bool allCorrect = true;
        foreach (var line in codeInputs)
        {
            if (line.input == null) continue;
            string input = line.input.text.Replace("\n", "").Replace("\r", "").Trim();

            bool matched = false;
            foreach (var ans in answers)
            {
                if (ans.id != line.id) continue;
                if (input == ans.correctAnswer.Replace("\n", "").Replace("\r", "").Trim())
                {
                    matched = true;
                    break;
                }
            }

            if (matched)
            {
                if (successBorder != null)
                    line.input.GetComponent<UnityEngine.UI.Image>().sprite = successBorder;
            }
            else allCorrect = false;
        }

        if (allCorrect)
        {
            solved = true;
            if (ERpanel != null) ERpanel.SetActive(false);
            if (OKpanel != null) OKpanel.SetActive(true);

            if (successSound != null) successSound.Play();

            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.MarkPuzzleSolved(puzzleId);
                GameProgressManager.Instance.StartCoroutine(CloseRoutine());
            }
            else StartCoroutine(CloseRoutine());
        }
        else
        {
            if (ERpanel != null) ERpanel.SetActive(true);
            if (OKpanel != null) OKpanel.SetActive(false);
            if (errorSound != null) errorSound.Play();
        }
    }

    private IEnumerator CloseRoutine()
    {
        yield return new WaitForSecondsRealtime(successDelay);

        Time.timeScale = 1f;
        onSuccess?.Invoke();

        if (gameObject.activeSelf)
        {
            solved = false;
            gameObject.SetActive(false);
        }
    }

    public void Close()
    {
        if (!solved)
            foreach (var kv in originalTexts)
            {
                var line = codeInputs.Find(l => l.id == kv.Key);
                if (line?.input != null) line.input.text = kv.Value;
            }
        Time.timeScale = 1f;
        solved = false;
        gameObject.SetActive(false);

        // ТВОЙ СПОСОБ: включаем PlayerMenuScript обратно
        var playerMenu = FindObjectOfType<PlayerMenuScript>();
        if (playerMenu != null) playerMenu.enabled = true;
    }
}