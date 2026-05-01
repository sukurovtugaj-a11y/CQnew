using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    public Slider volumeSlider;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenTypeDropdown;

    private SettingsCore settingsCore;

    private Vector2Int[] resolutions = new Vector2Int[]
    {
        new Vector2Int(640, 480),
        new Vector2Int(1024, 768),
        new Vector2Int(1280, 720),
        new Vector2Int(1920, 1080)
    };

    void Start()
    {
        settingsCore = FindObjectOfType<SettingsCore>();

        LoadSettings();

        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        if (screenTypeDropdown != null)
            screenTypeDropdown.onValueChanged.AddListener(OnScreenTypeChanged);
    }

    void LoadSettings()
    {
        if (settingsCore == null)
            settingsCore = FindObjectOfType<SettingsCore>();

        if (settingsCore == null || settingsCore.gameSettings == null) return;

        var gs = settingsCore.gameSettings;

        if (volumeSlider != null)
            volumeSlider.value = gs.audioSettings.MasterVolume;

        if (resolutionDropdown != null)
        {
            int resIndex = 0;
            Vector2Int current = gs.screenSettings.resolution;
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].x == current.x && resolutions[i].y == current.y)
                {
                    resIndex = i;
                    break;
                }
            }
            resolutionDropdown.SetValueWithoutNotify(resIndex);
        }

        if (screenTypeDropdown != null)
        {
            int screenIndex = (int)gs.screenSettings.typeScreen;
            screenTypeDropdown.SetValueWithoutNotify(Mathf.Clamp(screenIndex, 0, 2));
        }

        AudioReverbZone[] reverbZones = FindObjectsOfType<AudioReverbZone>();
        foreach (var reverb in reverbZones)
        {
            reverb.enabled = gs.audioSettings.MasterVolume > 1f;
        }
    }

    public void OnVolumeChanged(float value)
    {
        if (settingsCore == null || settingsCore.gameSettings == null) return;
        settingsCore.gameSettings.audioSettings.MasterVolume = Mathf.RoundToInt(value);
        AudioListener.volume = value / 100f;

        AudioReverbZone[] reverbZones = FindObjectsOfType<AudioReverbZone>();
        foreach (var reverb in reverbZones)
        {
            reverb.enabled = value > 1f;
        }
    }

    public void OnResolutionChanged(int index)
    {
        if (settingsCore == null || settingsCore.gameSettings == null) return;
        Vector2Int res = resolutions[Mathf.Clamp(index, 0, resolutions.Length - 1)];
        settingsCore.gameSettings.screenSettings.resolution = res;

        if (settingsCore.gameSettings.screenSettings.typeScreen != TypeScreen.Stretched)
        {
            bool fullscreen = settingsCore.gameSettings.screenSettings.typeScreen == TypeScreen.FullScreen;
            Screen.SetResolution(res.x, res.y, fullscreen);
            Debug.Log($"[SettingsMenu] Разрешение: {res.x}x{res.y}, Fullscreen={fullscreen}");
        }
        else
        {
            Debug.Log($"[SettingsMenu] Разрешение: {res.x}x{res.y}, но режим Растянутый — пропуск");
        }
    }

    public void OnScreenTypeChanged(int index)
    {
        if (settingsCore == null || settingsCore.gameSettings == null) return;

        TypeScreen type = TypeScreen.Windowed;
        switch (index)
        {
            case 0: type = TypeScreen.Windowed; break;
            case 1: type = TypeScreen.FullScreen; break;
            case 2: type = TypeScreen.Stretched; break;
        }

        settingsCore.gameSettings.screenSettings.typeScreen = type;
        Debug.Log($"[SettingsMenu] Тип экрана: {type}");
        ApplyScreenSettings();
    }

    void ApplyScreenSettings()
    {
        if (settingsCore == null || settingsCore.gameSettings == null) return;

        var screen = settingsCore.gameSettings.screenSettings;
        Vector2Int res = screen.resolution;

        if (screen.typeScreen == TypeScreen.Stretched)
        {
            Screen.SetResolution(res.x, res.y, false);
        }
        else
        {
            bool fullscreen = screen.typeScreen == TypeScreen.FullScreen;
            Screen.SetResolution(res.x, res.y, fullscreen);
        }
    }

    public void OnBack()
    {
        settingsCore.SaveSettings();
    }

    public void RefreshSettings()
    {
        LoadSettings();
    }
}
