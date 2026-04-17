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

    private bool isTeleportTransition;

    private void Start()
    {
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
                if (SpawnPointManager.forceLookRight && SceneManager.GetActiveScene().name != "MainScene")
                {
                    var scale = inst.transform.localScale;
                    scale.x = -Mathf.Abs(scale.x);
                    inst.transform.localScale = scale;
                    SpawnPointManager.forceLookRight = false;
                }
            }
            else
            {
                var main = inst.GetComponent<MainCharacter>();
                main.OnStart(this);
                if (SpawnPointManager.Instance != null)
                    SpawnPointManager.Instance.ApplySpawnPoint(main);
                if (SpawnPointManager.forceLookRight && SceneManager.GetActiveScene().name != "MainScene")
                {
                    var sr = main.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.flipX = false;
                    SpawnPointManager.forceLookRight = false;
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
        if (menuScript != null)
        {
            menuScript.CloseCurrentPanel();
        }
        else
        {
            var foundMenu = FindObjectOfType<PlayerMenuScript>();
            if (foundMenu != null)
                foundMenu.CloseCurrentPanel();
        }

        Time.timeScale = 1f;

        SpawnPointManager.forceLookRight = true;
        SceneManager.LoadScene(NameScene, LoadSceneMode.Single);
    }
}