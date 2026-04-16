using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    private string savePath => Application.dataPath + "/Save/slot.json";
    private SlotData data;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    void Load()
    {
        data = File.Exists(savePath)
            ? JsonUtility.FromJson<SlotData>(File.ReadAllText(savePath))
            : new SlotData();
    }

    void Save() => File.WriteAllText(savePath, JsonUtility.ToJson(data, true));

    public void SetSlotName(string name)
    {
        data.slotName = name;
        Save();
    }

    public bool IsGameStarted() => data.gameStarted;

    public void OnFirstMove()
    {
        if (!data.gameStarted) { data.gameStarted = true; Save(); }
    }

    public bool IsLevelCompleted(string level)
    {
        return level switch
        {
            "TrainL" => data.trainCompleted,
            "L1" => data.l1Completed,
            "L2" => data.l2Completed,
            "L3" => data.l3Completed,
            _ => false
        };
    }

    public void OnLevelComplete(string level)
    {
        switch (level)
        {
            case "TrainL": data.trainCompleted = true; break;
            case "L1": data.l1Completed = true; break;
            case "L2": data.l2Completed = true; break;
            case "L3": data.l3Completed = true; break;
        }

        // Повторное прохождение: если первый выбор уже есть — выдаём второй
        string branch = level switch { "TrainL" => "train", "L1" => "firstLevel", "L2" => "secondLevel", _ => null };
        if (branch != null)
        {
            string first = GetUpgrade(branch, false);
            string second = GetUpgrade(branch, true);
            if (!string.IsNullOrEmpty(first) && string.IsNullOrEmpty(second))
            {
                string remaining = GetRemainingUpgrade(branch, first);
                if (!string.IsNullOrEmpty(remaining))
                {
                    OnUpgradeChosen(branch, remaining, true);
                    data.pendingAchievement = remaining;
                    Save();
                    return;
                }
            }
        }

        Save();
    }

    private string GetRemainingUpgrade(string branch, string chosen)
    {
        return branch switch
        {
            "train" => chosen == "health" ? "upDamage" : null,
            "firstLevel" => chosen == "doubleJump" ? "dash" : (chosen == "dash" ? "doubleJump" : null),
            "secondLevel" => chosen == "checkpoint" ? "invincible" : (chosen == "invincible" ? "checkpoint" : null),
            _ => null
        };
    }

    public string GetUpgrade(string branch, bool secondChoice)
    {
        return branch switch
        {
            "train" => secondChoice ? data.trainUpgrade2 : data.trainUpgrade,
            "firstLevel" => secondChoice ? data.firstLevelUpgrade2 : data.firstLevelUpgrade,
            "secondLevel" => secondChoice ? data.secondLevelUpgrade2 : data.secondLevelUpgrade,
            _ => null
        };
    }

    public string GetPendingUpgradePanel()
    {
        if (data.trainCompleted && string.IsNullOrEmpty(data.trainUpgrade)) return "AfterTrain";
        if (data.l1Completed && string.IsNullOrEmpty(data.firstLevelUpgrade)) return "AfterFirstLevel";
        if (data.l2Completed && string.IsNullOrEmpty(data.secondLevelUpgrade)) return "AfterSecLevel";
        return null;
    }

    public List<string> GetAllPendingUpgradePanels()
    {
        var list = new List<string>();
        if (data.trainCompleted && string.IsNullOrEmpty(data.trainUpgrade)) list.Add("AfterTrain");
        if (data.l1Completed && string.IsNullOrEmpty(data.firstLevelUpgrade)) list.Add("AfterFirstLevel");
        if (data.l2Completed && string.IsNullOrEmpty(data.secondLevelUpgrade)) list.Add("AfterSecLevel");
        return list;
    }

    public void OnUpgradeChosen(string branch, string choice, bool secondChoice)
    {
        switch (branch)
        {
            case "train":
                if (secondChoice) data.trainUpgrade2 = choice; else data.trainUpgrade = choice;
                break;
            case "firstLevel":
                if (secondChoice) data.firstLevelUpgrade2 = choice; else data.firstLevelUpgrade = choice;
                break;
            case "secondLevel":
                if (secondChoice) data.secondLevelUpgrade2 = choice; else data.secondLevelUpgrade = choice;
                break;
        }
        Save();
    }

    public string GetAndClearPendingAchievement()
    {
        if (string.IsNullOrEmpty(data.pendingAchievement)) return null;
        string a = data.pendingAchievement;
        data.pendingAchievement = null;
        Save();
        return a;
    }

    // Puzzle progress
    public bool IsPuzzleSolved(string id) => data != null && data.solvedPuzzles != null && data.solvedPuzzles.Contains(id);

    public void MarkPuzzleSolved(string id)
    {
        if (data == null || data.solvedPuzzles == null) return;
        if (!data.solvedPuzzles.Contains(id))
        {
            data.solvedPuzzles.Add(id);
            Save();
        }
    }
}

[System.Serializable]
public class SlotData
{
    public string slotName;
    public bool gameStarted;
    public bool trainCompleted, l1Completed, l2Completed, l3Completed;
    public string trainUpgrade, trainUpgrade2;
    public string firstLevelUpgrade, firstLevelUpgrade2;
    public string secondLevelUpgrade, secondLevelUpgrade2;
    public string pendingAchievement;
    public List<string> solvedPuzzles = new List<string>();
}
