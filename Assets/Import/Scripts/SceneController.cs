using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public SettingsCore settingsCore;
    public MainCharacter player;

    private void Start()
    {
        settingsCore = FindObjectOfType<SettingsCore>();
        if (settingsCore == null)
        {
            SceneManager.LoadScene("Prestart");
            return;
        }

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            player = Instantiate(player, Vector3.zero, Quaternion.Euler(0, 0, 0), null);
            player.OnStart(this);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LoadScene(string NameScene)
    {
        settingsCore.sceneChanger.LoadNewScene(NameScene, 0.5f);
    }

    public void SaveGame()
    {
        string nameFile = "SaveTest.txt";
        string path = settingsCore.gameSettings.pathData.SavePath + nameFile;
        string[] SavingData = new string[]
        {
        "[GENERAL]",
        $"   Name scene: {SceneManager.GetActiveScene().name}",
        "   Position: " + player.transform.position.ToString().Replace("(", "").Replace(")", ""),
        "   Rotation: " + player.transform.rotation.eulerAngles.ToString().Replace("(", "").Replace(")", ""),
        "   Flip: " + player.GetComponent<SpriteRenderer>().flipX,
        $"   Health: {0}",
        $"   Money: {0}",
        "[INVENTORY]",
        "[EFFECTS]"
        };
        settingsCore.fileSystem.WriteData(path, SavingData);
    }
}
