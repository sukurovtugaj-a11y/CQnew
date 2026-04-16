using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SaveSlotManager : MonoBehaviour
{
    public InputField fieldName;
    public GameObject playButton;
    public GameObject applyButton;
    public GameObject editButton;
    public GameObject deleteButton;

    private string savePath => Application.dataPath + "/Save/slot.json";

    void Start()
    {
        bool exists = File.Exists(savePath);
        applyButton.SetActive(false);

        if (exists)
            fieldName.text = JsonUtility.FromJson<SlotData>(File.ReadAllText(savePath)).slotName;

        editButton.SetActive(exists);
        deleteButton.SetActive(exists);

        playButton.GetComponent<MyButton>().AddClick(() => PlayGame());
        applyButton.GetComponent<MyButton>().AddClick(() => SaveName());
        editButton.GetComponent<MyButton>().AddClick(() => StartEdit());
        deleteButton.GetComponent<MyButton>().AddClick(() => DeleteSave());
        fieldName.onValueChanged.AddListener((val) => applyButton.SetActive(true));
    }

    void StartEdit() { applyButton.SetActive(true); fieldName.Select(); }

    void SaveName()
    {
        string name = fieldName.text.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            var data = File.Exists(savePath) ? JsonUtility.FromJson<SlotData>(File.ReadAllText(savePath)) : new SlotData();
            data.slotName = name;
            File.WriteAllText(savePath, JsonUtility.ToJson(data, true));

            if (GameProgressManager.Instance != null)
                GameProgressManager.Instance.SetSlotName(name);

            editButton.SetActive(true);
            deleteButton.SetActive(true);
        }
        applyButton.SetActive(false);
    }

    void DeleteSave()
    {
        if (File.Exists(savePath)) File.Delete(savePath);
        fieldName.text = "";
        editButton.SetActive(false);
        deleteButton.SetActive(false);
    }

    void PlayGame()
    {
        string name = fieldName.text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            fieldName.onValueChanged.RemoveAllListeners();
            name = "КрутойУЛЬТРАнеВвёлИмя";
            fieldName.text = name;
            var data = new SlotData { slotName = name };
            File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
            editButton.SetActive(true);
            deleteButton.SetActive(true);
            fieldName.onValueChanged.AddListener((val) => applyButton.SetActive(true));
        }
        if (GameProgressManager.Instance != null)
            GameProgressManager.Instance.SetSlotName(name);
        Debug.Log($"Загрузка: {name}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && applyButton.activeSelf) SaveName();
        else if (Input.GetKeyDown(KeyCode.Escape) && applyButton.activeSelf) applyButton.SetActive(false);
    }
}
