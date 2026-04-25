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

    public void StartGameCheckVideo()
    {
        if (GameProgressManager.Instance != null && !GameProgressManager.Instance.IsIntroWatched())
        {
            VideoController.videoToPlay = 5;
            VideoController.autoReturn = true;
            VideoController.spawnAtWokyZone = true;
            VideoController.currentLevelForVideo = "Woky";
            LoadScene("VideoScene");
        }
        else
        {
            LoadScene("MainScene");
        }
    }

    public void PlayVideoIntro() { GoToVideo(0, true); }
    public void PlayVideoIntroFromPanel() { GoToVideo(0, false); }
    public void PlayVideoPL() { GoToVideo(1, false); }
    public void PlayVideoTDUP() { GoToVideo(2, false); }
    public void PlayVideoSOS() { GoToVideo(3, false); }
    public void PlayVideoOOP() { GoToVideo(4, false); }

    private void GoToVideo(int index, bool spawnAtIntro)
    {
        VideoController.videoToPlay = index;
        VideoController.autoReturn = true;
        VideoController.spawnAtIntroZone = spawnAtIntro;
        VideoController.currentLevelForVideo = index == 0 ? "Intro" : null;
        LoadScene("VideoScene");
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