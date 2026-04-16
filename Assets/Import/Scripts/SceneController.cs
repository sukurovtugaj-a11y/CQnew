using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField]
    private Timer CurrentTimer;

    [Header("Settings")]
    public SettingsCore settingsCore;
    public GameObject playerPrefab;

    [Header("Menu Reference (обязательно назначь!)")]
    [Tooltip("Ссылка на объект с PlayerMenuScript для закрытия меню перед переходом")]
    public PlayerMenuScript menuScript;

    private void Start()
    {
        // Создаём EventSystem если нет (нужен для работы UI/кнопок)
        EnsureEventSystemExists();

        if (settingsCore == null)
            settingsCore = FindObjectOfType<SettingsCore>();

        if (settingsCore == null)
        {
            SceneManager.LoadScene("Prestart");
            return;
        }

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            var inst = Instantiate(playerPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0), null);

            var sec = inst.GetComponent<SecMainCharacter>();
            if (sec != null)
            {
                sec.OnStart(this);
                if (SpawnPointManager.Instance != null)
                    SpawnPointManager.Instance.ApplySpawnPointToSec(sec);
                if (SceneManager.GetActiveScene().name != "MainScene")
                {
                    var scale = inst.transform.localScale;
                    scale.x = -Mathf.Abs(scale.x);
                    inst.transform.localScale = scale;
                }
            }
            else
            {
                var main = inst.GetComponent<MainCharacter>();
                main.OnStart(this);
                if (SpawnPointManager.Instance != null)
                    SpawnPointManager.Instance.ApplySpawnPoint(main);
                if (SceneManager.GetActiveScene().name != "MainScene")
                {
                    var scale = inst.transform.localScale;
                    scale.x = -Mathf.Abs(scale.x);
                    inst.transform.localScale = scale;
                }
            }
        }
    }

    public void ChangeValue(int value)
    {
        if (CurrentTimer != null) { CurrentTimer.TimeValue += value; }
    }

    private void EnsureEventSystemExists()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();

            // Добавляем StandaloneInputModule для обработки ввода (мышь/клавиатура)
            var standaloneInput = eventSystem.AddComponent<StandaloneInputModule>();
            standaloneInput.forceModuleActive = true;

            Debug.Log("[SceneController] EventSystem создан автоматически");
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// 🔥 ЗАГРУЗКА СЦЕНЫ — работает даже если меню открыто и timeScale = 0
    /// </summary>
    public void LoadScene(string NameScene)
    {
        // 1. Закрываем меню, если есть ссылка
        if (menuScript != null)
        {
            menuScript.CloseCurrentPanel();
        }
        else
        {
            // Если ссылки нет — ищем в сцене (запасной вариант)
            var foundMenu = FindObjectOfType<PlayerMenuScript>();
            if (foundMenu != null)
                foundMenu.CloseCurrentPanel();
        }

        // 2. 🔥 ПРИНУДИТЕЛЬНО восстанавливаем время — гарантируем, что всё заработает
        Time.timeScale = 1f;

        // 3. Загружаем сцену напрямую через Unity (надёжно и быстро)
        SceneManager.LoadScene(NameScene, LoadSceneMode.Single);
    }
}