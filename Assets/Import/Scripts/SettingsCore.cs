using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SettingsCore;

public class SettingsCore : MonoBehaviour
{
    public GameSettings gameSettings;
    public FileSystem fileSystem;
    public SceneChanger sceneChanger;

    private void Awake()
    {
        // Инициализируем все вложенные объекты
        if (gameSettings.screenSettings == null)
            gameSettings.screenSettings = new ScreenSettings();
        if (gameSettings.audioSettings == null)
            gameSettings.audioSettings = new AudioSettings();
        if (gameSettings.keyControll == null)
            gameSettings.keyControll = new KeyControll();
    }

    private void Start()
    {
        DontDestroyOnLoad(this);

        fileSystem = gameObject.AddComponent<FileSystem>();
        sceneChanger = gameObject.AddComponent<SceneChanger>();
        fileSystem.OnStart(this);

        sceneChanger.LoadNewScene("MainMenu", 1f);
    }

    public void SetNewSettings(string[] Data)
    {
        Vector2Int res = Convertor.ReadVector2INT(Data, "Resolution", new Vector2Int(1024, 768));
        if (res.x < 640 || res.y < 480)
        {
            res = new Vector2Int(640, 480);
        }
        gameSettings.screenSettings.resolution = res;
        gameSettings.audioSettings.MusicVolume = Convertor.ReadINT(Data, "Music Volume", 50);
        gameSettings.audioSettings.EffectVolume = Convertor.ReadINT(Data, "Effects Volume", 50);
        gameSettings.audioSettings.VoiceVolume = Convertor.ReadINT(Data, "Voice Volume", 50);
        gameSettings.audioSettings.UiVolume = Convertor.ReadINT(Data, "UI Volume", 50);

        gameSettings.keyControll.MoveBack = Convertor.ReadSTRING(Data, "Front", "d");
        gameSettings.keyControll.MoveFront = Convertor.ReadSTRING(Data, "Back", "a");
        gameSettings.keyControll.MoveUp = Convertor.ReadSTRING(Data, "Up", "w");
        gameSettings.keyControll.MoveDown = Convertor.ReadSTRING(Data, "Down", "s");
        gameSettings.keyControll.MenuButton = Convertor.ReadSTRING(Data, "JumpButton", "space");
        gameSettings.keyControll.JumpButton = Convertor.ReadSTRING(Data, "MenuButton", "escape");

        gameSettings.screenSettings.typeScreen = Convertor.ReadENUM(Data, "typeScreen", TypeScreen.Windowed);

        Screen.SetResolution(res.x, res.y, true);
    }
}

public class SceneChanger : MonoBehaviour
{
    string NameScene = "";
    public void LoadNewScene(string _NameScene, float TimeWait)
    {
        if (_NameScene.Length > 0)
        {
            NameScene = _NameScene;
            Invoke("Load", Mathf.Clamp(TimeWait, 0.01f, 100f));
        }
        _NameScene = "";
    }
    private void Load()
    {
        SceneManager.LoadScene(NameScene);
        NameScene = "";
    }

}
public class FileSystem : MonoBehaviour
{

    private SettingsCore SCore;
    public void OnStart(SettingsCore _SCore)
    {
        SCore = _SCore;
        RefreshPath();
        if (!ScanDirection(GetComponent<SettingsCore>().gameSettings.pathData.SavePath))
        {
            CreateDeffaultDate(GetComponent<SettingsCore>().gameSettings.pathData.SavePath);
        }
        if (!ScanDirection(GetComponent<SettingsCore>().gameSettings.pathData.LogErrorPath)) { CreateDeffaultDate(GetComponent<SettingsCore>().gameSettings.pathData.LogErrorPath); }
        if (ScanFile(GetComponent<SettingsCore>().gameSettings.pathData.GameSettingsPath)) { SCore.SetNewSettings(ReadData(GetComponent<SettingsCore>().gameSettings.pathData.GameSettingsPath)); } else { CreateDeffaultDate(GetComponent<SettingsCore>().gameSettings.pathData.GameSettingsPath, TypeFile.Settings); }
    }

    public bool ScanDirection(string path)
    {
        return Directory.Exists(path);
    }
    public bool ScanFile(string path)
    {
        return File.Exists(path);
    }

    public void WriteData(string path, string[] Data)
    {
        File.WriteAllLines(path, Data);
    }

    public string[] ReadData(string path)
    {
        if (ScanFile(path)) { return File.ReadAllLines(path); }
        return null;
    }

    private void CreateDeffaultDate(string path)
    {
        Directory.CreateDirectory(path);
    }


    private void CreateDeffaultDate(string path, TypeFile type)
    {
        string[] Data = null;

        switch (type)
        {
            case TypeFile.Settings:
                string resString = $"({SCore.gameSettings.screenSettings.resolution.x},{SCore.gameSettings.screenSettings.resolution.y})";
                Data = new string[]
                {
                    "[GENERAL]",
                    "[SOUNDS]",
                    "     Music Volume:" + SCore.gameSettings.audioSettings.MusicVolume,
                    "   Effects Volume:" + SCore.gameSettings.audioSettings.EffectVolume,
                    "     Voice Volume:" + SCore.gameSettings.audioSettings.VoiceVolume,
                    "        UI Volume:" + SCore.gameSettings.audioSettings.UiVolume,
                    "[SCREEN]",
                    "       resolution:" + resString,
                    "       typeScreen:" + SCore.gameSettings.screenSettings.typeScreen.ToString(),
                    "[KEYS]",
                    "            Front:" + SCore.gameSettings.keyControll.MoveFront,
                    "             Back:" + SCore.gameSettings.keyControll.MoveBack,
                    "             Up:" + SCore.gameSettings.keyControll.MoveUp,
                    "             Down:" + SCore.gameSettings.keyControll.MoveDown,
                    "       JumpButton:" + SCore.gameSettings.keyControll.JumpButton,
                    "       MenuButton:" + SCore.gameSettings.keyControll.MenuButton,
                };
                SCore.SetNewSettings(Data);
                break;
            case TypeFile.Network:
                Data = new string[]
                {
                    "IP:127.0.0.1",
                    "PORT:25565"
                };
                //SCore.SetNewNetworkParameter(Data);
                break;
        }

        if (Data != null)
        {
            WriteData(path, Data);
        }
    }


    private void RefreshPath()
    {
        Debug.Log(GetComponent<SettingsCore>().gameSettings.pathData);
        GetComponent<SettingsCore>().gameSettings.pathData.GameSettingsPath = Application.dataPath + GetComponent<SettingsCore>().gameSettings.pathData.GameSettingsPath;
        GetComponent<SettingsCore>().gameSettings.pathData.NetworkSettingsPath = Application.dataPath + GetComponent<SettingsCore>().gameSettings.pathData.NetworkSettingsPath;

        GetComponent<SettingsCore>().gameSettings.pathData.LogErrorPath = Application.dataPath + GetComponent<SettingsCore>().gameSettings.pathData.LogErrorPath;
        GetComponent<SettingsCore>().gameSettings.pathData.SavePath = Application.dataPath + GetComponent<SettingsCore>().gameSettings.pathData.SavePath;
    }
}
public class Convertor
{
    public static Vector2Int ReadVector2INT(string[] Array, string FindNote, Vector2Int ErrorData)
    {
        Vector2Int result = ErrorData;
        for (int i = 0; FindNote.Length > 0 && i < Array.Length; i++)
        {
            if (Array[i].ToLower().IndexOf(FindNote.ToLower()) >= 0)
            {
                int x = ErrorData.x;
                int y = ErrorData.y;
                try
                {
                    string[] info = Array[i].ToLower().Replace(FindNote.ToLower(), "").Replace("(", "").Replace(")", "").Replace(" ", "").Replace(":", "").Replace(";", "").Split(",");
                    x = int.Parse(info[0]);
                    y = int.Parse(info[1]);

                    result = new Vector2Int(x, y);
                }
                catch
                {
                    return ErrorData;
                }
            }
        }
        return result;
    }

    public static int ReadINT(string[] Array, string FindNote, int ErrorData)
    {
        int result = ErrorData;
        for (int i = 0; FindNote.Length > 0 && i < Array.Length; i++)
        {
            if (Array[i].ToLower().IndexOf(FindNote.ToLower()) >= 0)
            {
                try
                {
                    result = int.Parse(Array[i].ToLower().Replace(FindNote.ToLower(), "").Replace("(", "").Replace(")", "").Replace(" ", "").Replace(":", "").Replace(";", ""));
                }
                catch
                {
                    return ErrorData;
                }
            }
        }
        return result;
    }

    public static string ReadSTRING(string[] Array, string FindNote, string ErrorData)
    {
        string result = ErrorData;
        for (int i = 0; FindNote.Length > 0 && i < Array.Length; i++)
        {
            if (Array[i].ToLower().IndexOf(FindNote.ToLower()) >= 0)
            {
                try
                {
                    result = Array[i].ToLower().Replace(FindNote.ToLower(), "").Replace("(", "").Replace(")", "").Replace(" ", "").Replace(":", "").Replace(";", "");
                }
                catch
                {
                    return ErrorData;
                }

            }
        }
        return result;
    }

    public static TypeScreen ReadENUM(string[] Array, string FindNote, TypeScreen ErrorData)
    {
        TypeScreen result = ErrorData;
        for (int i = 0; FindNote.Length > 0 && i < Array.Length; i++)
        {
            if (Array[i].ToLower().IndexOf(FindNote.ToLower()) >= 0)
            {
                try
                {
                    string type = Array[i].ToLower().Replace(FindNote.ToLower(), "").Replace("(", "").Replace(")", "").Replace(" ", "").Replace(":", "").Replace(";", "");

                    if (type == TypeScreen.FullScreen.ToString().ToLower())
                    {
                        result = TypeScreen.FullScreen;
                        break;
                    }
                    if (type == TypeScreen.Windowed.ToString().ToLower())
                    {
                        result = TypeScreen.Windowed;
                        break;
                    }
                    if (type == TypeScreen.Stretched.ToString().ToLower())
                    {
                        result = TypeScreen.Stretched;
                        break;
                    }
                }
                catch
                {
                    return ErrorData;
                }

            }
        }
        return result;
    }
}

[System.Serializable]
public class GameSettings
{
    public PathData pathData = new PathData();

    public string Version = "0.001a";
    public ScreenSettings screenSettings;
    public AudioSettings audioSettings;
    public KeyControll keyControll;
}

[System.Serializable]
public class ScreenSettings
{
    public Vector2Int resolution = new Vector2Int(1920, 1080);
    public TypeScreen typeScreen = TypeScreen.FullScreen;
}

[System.Serializable]
public class AudioSettings
{
    [Range(0, 100)] public int MusicVolume = 50;
    [Range(0, 100)] public int EffectVolume = 50;
    [Range(0, 100)] public int VoiceVolume = 50;
    [Range(0, 100)] public int UiVolume = 50;
}

[System.Serializable]
public class KeyControll
{
    public string Action = "e";
    public string boost = "left Shift";
    public string MoveFront = "d";
    public string MoveBack = "a";
    public string MoveUp = "w";
    public string MoveDown = "s";
    public string JumpButton = "space";
    public string MenuButton = "escape";
}
[System.Serializable]
public class PathData
{
    public string GameSettingsPath = "/GameSettings.ini";
    public string NetworkSettingsPath = "/NetworkSettings.ini";
    public string LogErrorPath = "/Logs/";
    public string SavePath = "/Save/";
}

[System.Serializable]
public class Saving
{
    public string SceneName;
    public Vector3 position;
    public Vector3 rotation;
    public int health;
    public int money;
}

public enum TypeScreen
{
    Windowed, FullScreen, Stretched
}

public enum TypeFile
{
    Settings, Network, Log, Save, Other
}