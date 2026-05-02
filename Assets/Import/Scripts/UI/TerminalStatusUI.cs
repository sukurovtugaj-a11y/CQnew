using UnityEngine;
using UnityEngine.UI;

public class TerminalStatusUI : MonoBehaviour
{
    [Header("Текст имени")]
    public Text nameText;

    [Header("Текст прогресса")]
    public Text progressText;

    [Header("Иконки прокачки (сами выключите в инспекторе)")]
    public GameObject healthIcon;
    public GameObject upDamageIcon;
    public GameObject doubleJumpIcon;
    public GameObject dashIcon;
    public GameObject checkpointIcon;
    public GameObject invincibleIcon;

    void Start()
    {
        var gpm = GameProgressManager.Instance;
        if (gpm == null)
        {
            Debug.LogWarning("[TerminalStatusUI] GameProgressManager не найден!");
            return;
        }

        int completed = CountCompleted(gpm);
        int total = 20;
        float percent = (float)completed / total * 100f;

        if (progressText != null)
            progressText.text = $"{(int)percent}%";

        if (nameText != null)
            nameText.text = gpm.GetSlotName();

        ActivateUpgradeIcons(gpm);
    }

    private int CountCompleted(GameProgressManager gpm)
    {
        int count = 0;

        if (gpm.IsGameStarted()) count++;
        if (!string.IsNullOrEmpty(gpm.GetSlotName())) count++;
        if (gpm.IsVoiceoverPlayed()) count++;

        if (gpm.IsLevelCompleted("TrainL")) count++;
        if (gpm.IsLevelCompleted("L1")) count++;
        if (gpm.IsLevelCompleted("L2")) count++;
        if (gpm.IsLevelCompleted("L3")) count++;
        if (gpm.IsLevelCompleted("DreamRunning")) count++;

        if (gpm.IsVideoWatched("Intro")) count++;
        if (gpm.IsVideoWatched("UDontDrUCrIt")) count++;
        if (gpm.IsVideoWatched("TrainL")) count++;
        if (gpm.IsVideoWatched("L1")) count++;
        if (gpm.IsVideoWatched("L2")) count++;
        if (gpm.IsVideoWatched("L3")) count++;

        if (HasUpgrade("train")) count++;
        if (HasUpgrade("train2")) count++;
        if (HasUpgrade("firstLevel")) count++;
        if (HasUpgrade("firstLevel2")) count++;
        if (HasUpgrade("secondLevel")) count++;
        if (HasUpgrade("secondLevel2")) count++;

        return count;
    }

    private bool HasUpgrade(string branch)
    {
        var gpm = GameProgressManager.Instance;
        string upgrade = branch switch
        {
            "train" => gpm.GetUpgrade("train", false),
            "train2" => gpm.GetUpgrade("train", true),
            "firstLevel" => gpm.GetUpgrade("firstLevel", false),
            "firstLevel2" => gpm.GetUpgrade("firstLevel", true),
            "secondLevel" => gpm.GetUpgrade("secondLevel", false),
            "secondLevel2" => gpm.GetUpgrade("secondLevel", true),
            _ => null
        };
        return !string.IsNullOrEmpty(upgrade);
    }

    private void ActivateUpgradeIcons(GameProgressManager gpm)
    {
        SetIconIfUpgraded(healthIcon, gpm, "train", "health");
        SetIconIfUpgraded(upDamageIcon, gpm, "train", "upDamage");
        SetIconIfUpgraded(doubleJumpIcon, gpm, "firstLevel", "doubleJump");
        SetIconIfUpgraded(dashIcon, gpm, "firstLevel", "dash");
        SetIconIfUpgraded(checkpointIcon, gpm, "secondLevel", "checkpoint");
        SetIconIfUpgraded(invincibleIcon, gpm, "secondLevel", "invincible");
    }

    private void SetIconIfUpgraded(GameObject icon, GameProgressManager gpm, string branch, string upgradeName)
    {
        if (icon == null) return;

        bool first = gpm.GetUpgrade(branch, false) == upgradeName;
        bool second = gpm.GetUpgrade(branch, true) == upgradeName;

        if (first || second)
            icon.SetActive(true);
    }
}
