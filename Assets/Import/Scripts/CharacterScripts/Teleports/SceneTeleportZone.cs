using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleportZone : MonoBehaviour
{
    [Header("Настройки сцены")]
    public string sceneName;

    [Header("Точка появления")]
    public Vector3 spawnPosition = Vector3.zero;

    [Header("Направление игрока")]
    public bool changeDirection = false;
    public bool lookRight = true;

    [Header("Прокачка")]
    [Tooltip("Если true — переход на Upgrades после прохождения уровня")]
    public bool isFinishLevel = false;

    [Header("Блокировка управления")]
    [Tooltip("Блокировать управление персонажем после загрузки сцены")]
    public bool lockControlsOnEnter = false;
    [Tooltip("Длительность блокировки в секундах")]
    public float lockDuration = 2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Поддержка скольжения: спрайт SLIDERINGNAW 1 имеет свой коллайдер
        if (collision.name.StartsWith("SLIDERINGNAW 1"))
        {
            var slideChar = collision.GetComponentInParent<SecMainCharacter>();
            if (slideChar != null && !string.IsNullOrEmpty(sceneName))
            {
                HandleTeleport(slideChar, slideChar.currentHealth);
            }
            return;
        }

        var secChar = collision.GetComponent<SecMainCharacter>();
        MonoBehaviour player = secChar;

        if (player != null && !string.IsNullOrEmpty(sceneName))
        {
            int health = secChar.currentHealth;
            HandleTeleport(secChar, health);
        }
    }

    private void HandleTeleport(SecMainCharacter secChar, int health)
    {
        SpawnPointManager.LastHealth = health;

        var spawnManager = FindObjectOfType<SpawnPointManager>();
        if (spawnManager != null)
        {
            spawnManager.spawnPosition = spawnPosition;
            spawnManager.setDirection = changeDirection;
            spawnManager.lookRight = lookRight;
            spawnManager.hasSpawnData = true;
            spawnManager.lockControls = lockControlsOnEnter;
            spawnManager.lockDuration = lockDuration;
        }
        else
        {
            GameObject managerObj = new GameObject("SpawnPointManager");
            spawnManager = managerObj.AddComponent<SpawnPointManager>();
            DontDestroyOnLoad(managerObj);
            spawnManager.spawnPosition = spawnPosition;
            spawnManager.setDirection = changeDirection;
            spawnManager.lookRight = lookRight;
            spawnManager.hasSpawnData = true;
            spawnManager.lockControls = lockControlsOnEnter;
            spawnManager.lockDuration = lockDuration;
        }

        if (isFinishLevel)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            string level = currentScene switch
            {
                var s when s.StartsWith("L3") => "L3",
                var s when s.StartsWith("L1") => "L1",
                var s when s.StartsWith("L2") => "L2",
                var s when s.StartsWith("Train") => "TrainL",
                var s when s.StartsWith("DreamRunning") => "DreamRunning",
                _ => null
            };
            if (level != null && GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.OnLevelComplete(level);
                Debug.Log($"[SceneTeleportZone] Level completed: {level}");

                if (level == "L3")
                {
                    int? videoIndex = GameProgressManager.Instance.GetVideoIndexForLevel("L3");
                    Debug.Log($"[SceneTeleportZone] L3 videoIndex: {videoIndex}");
                    if (videoIndex.HasValue)
                    {
                        VideoController.videoToPlay = videoIndex.Value;
                        VideoController.autoReturn = true;
                        VideoController.currentLevelForVideo = "L3";
                        SceneManager.LoadScene("VideoScene");
                        return;
                    }
                }
            }

            SceneManager.LoadScene("Upgrades");
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
