using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;

public class SettingsScript : MonoBehaviour
{
    private IDataServiceScript dataService = new JsonDataServiceScript();
    public SettingsSaveData saveData = new();
    private bool isEncrypted;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Toggle toggleFullScreen;
    [SerializeField] private TMP_Dropdown dropDownResolution;
    /*[SerializeField] private TMP_Dropdown dropDownLanguage;
    [SerializeField] private TMP_Dropdown dropDownQuality;*/

    [Header("Master Volume")]
    [SerializeField] private Slider sliderMasterVolume;
    [SerializeField] private TextMeshProUGUI textMasterVolume;
    [SerializeField] private string volumeParameterMaster = "MasterVolume";
    private float currentMasterVolume = maxValue;

    [Header("Music Volume")]
    [SerializeField] private Slider sliderMusicVolume;
    [SerializeField] private TextMeshProUGUI textMusicVolume;
    [SerializeField] private string volumeParameterMusic = "MusicVolume";
    private float currentMusicVolume = maxValue;

    [Header("SFX Volume")]
    [SerializeField] private Slider sliderSFXVolume;
    [SerializeField] private TextMeshProUGUI textSFXVolume;
    [SerializeField] private string volumeParameterSFX = "SFXVolume";
    private float currentSFXVolume = maxValue;

    private const float minValue = 0f;
    private const float maxValue = 1000f;
    [SerializeField] private float defaultVolume = 1000f;
    public int parameterVolume = 20;
    private Resolution[] resolutions;

    private readonly string relativePath = "/settings-savefile.json";

    private void Awake()
    {
        string path = Application.persistentDataPath + relativePath;
        Debug.Log(path);

        resolutions = Screen.resolutions;

        if (!File.Exists(path)) SetFirstSaveFile();
    }

    private void Start()
    {
        SetResolution();

        SetupVolumeSlider(sliderMasterVolume, volumeParameterMaster, textMasterVolume, value => currentMasterVolume = value);
        SetupVolumeSlider(sliderMusicVolume, volumeParameterMusic, textMusicVolume, value => currentMusicVolume = value);
        SetupVolumeSlider(sliderSFXVolume, volumeParameterSFX, textSFXVolume, value => currentSFXVolume = value);

        LoadSettings();

        toggleFullScreen.onValueChanged.AddListener(OnFullscreenToggleChanged);
        dropDownResolution.onValueChanged.AddListener(OnResolutionDropdownChanged);

        Debug.Log($"[Settings] Current screen resolution after load: {Screen.width}x{Screen.height}, fullscreen: {Screen.fullScreen}");
    }

    private void OnDestroy()
    {
        if (toggleFullScreen != null)
            toggleFullScreen.onValueChanged.RemoveListener(OnFullscreenToggleChanged);
        if (dropDownResolution != null)
            dropDownResolution.onValueChanged.RemoveListener(OnResolutionDropdownChanged);
    }

    private void OnFullscreenToggleChanged(bool isOn)
    {
        SetFullscreen(isOn);
    }

    private void OnResolutionDropdownChanged(int index)
    {
        SetResolution(index);
    }

    private void SetupVolumeSlider(Slider slider, string volumeParameter, TextMeshProUGUI volumeText, Action<float> onValueChanged)
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = true;
        slider.onValueChanged.AddListener(value =>
        {
            onValueChanged(value);
            ApplyVolume(value, volumeParameter, volumeText);
        });
    }

    private void ApplyVolume(float volumeValue, string volumeParameter, TextMeshProUGUI volumeText)
    {
        float actualVolume = volumeValue <= 0f ? 0.00001f : volumeValue;
        audioMixer.SetFloat(volumeParameter, DecibelConvert(actualVolume));
        volumeText.text = volumeValue.ToString("0");
    }

    private float SetVolume(float savedVolume, string volumeParameter, Slider slider, TextMeshProUGUI volumeText)
    {
        float clamped = Mathf.Clamp(savedVolume, minValue, maxValue);
        slider.SetValueWithoutNotify(clamped);
        ApplyVolume(clamped, volumeParameter, volumeText);
        return clamped;
    }

    private void SetResolution()
    {
        dropDownResolution.ClearOptions();
        List<string> options = new List<string>();
        resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height; // + " " + resolutions[i].refreshRateRatio + "Hz"
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }

        dropDownResolution.AddOptions(options);
        dropDownResolution.RefreshShownValue();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        SaveSettings();
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions == null || resolutionIndex < 0 || resolutionIndex >= resolutions.Length)
        {
            Debug.LogWarning($"[Settings] Invalid resolution index: {resolutionIndex}");
            return;
        }

        Resolution resolution = resolutions[resolutionIndex];
        Debug.Log($"[Settings] Applying resolution: {resolution.width}x{resolution.height}, fullscreen: {Screen.fullScreen}");
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        SaveSettings();
    }

    public void ResetToDefaults()
    {
        Debug.Log("[Settings] Resetting all settings to default values.");

        currentMasterVolume = SetVolume(defaultVolume, volumeParameterMaster, sliderMasterVolume, textMasterVolume);
        currentMusicVolume = SetVolume(defaultVolume, volumeParameterMusic, sliderMusicVolume, textMusicVolume);
        currentSFXVolume = SetVolume(defaultVolume, volumeParameterSFX, sliderSFXVolume, textSFXVolume);

        Screen.fullScreen = true;
        toggleFullScreen.SetIsOnWithoutNotify(true);

        SetResolutionToCurrentMonitor();
        SaveSettings();

        Debug.Log("[Settings] Reset to defaults complete.");
    }

    private int GetCurrentMonitorResolutionIndex()
    {
        resolutions = Screen.resolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                return i;
        }
        return 0;
    }

    private void SetResolutionToCurrentMonitor()
    {
        int currentResolutionIndex = GetCurrentMonitorResolutionIndex();
        Resolution resolution = resolutions[currentResolutionIndex];
        Debug.Log($"[Settings] Setting resolution to current monitor: {resolution.width}x{resolution.height}, fullscreen: {Screen.fullScreen}");

        dropDownResolution.SetValueWithoutNotify(currentResolutionIndex);
        SetResolution(currentResolutionIndex);
    }

    public void SaveSettings()
    {
        FillSettingsSaveData();

        SerializeJson();
    }

    private void FillSettingsSaveData()
    {
        //saveData.language = dropDownLanguage.value;

        saveData.volumeMaster = currentMasterVolume;
        saveData.volumeMusic = currentMusicVolume;
        saveData.volumeSFX = currentSFXVolume;

        saveData.resolution = dropDownResolution.value;
        saveData.fullscreen = System.Convert.ToInt32(Screen.fullScreen);
        //saveData.quality = dropDownQuality.value;
    }

    private void SerializeJson()
    {
        long s = DateTime.Now.Ticks;
        long f = 0;
        if (dataService.SaveData("/settings-savefile.json", saveData, isEncrypted))
        {
            f = DateTime.Now.Ticks - s;
            Debug.Log($"Save Time {(f / 100000f):N4}ms");
            Debug.Log(Application.persistentDataPath + "/settings-savefile.json");
        }
        else
        {
            Debug.LogError("Cant save file bro");
        }
    }

    public void LoadSettings()
    {
        DeserializeJson();
        ReadSettingsSaveData();
    }

    private void DeserializeJson()
    {
        long s = DateTime.Now.Ticks;
        long f = 0;
        try
        {
            saveData = dataService.LoadData<SettingsSaveData>("/settings-savefile.json", isEncrypted);
            f = DateTime.Now.Ticks - s;
            Debug.Log($"Load Time {(f / 100000f):N4}ms");
        }
        catch
        {
            Debug.LogError("Cant load file bro");
        }
    }

    private void ReadSettingsSaveData()
    {
        //dropDownLanguage.value = saveData.language;

        currentMasterVolume = SetVolume(saveData.volumeMaster, volumeParameterMaster, sliderMasterVolume, textMasterVolume);
        currentMusicVolume = SetVolume(saveData.volumeMusic, volumeParameterMusic, sliderMusicVolume, textMusicVolume);
        currentSFXVolume = SetVolume(saveData.volumeSFX, volumeParameterSFX, sliderSFXVolume, textSFXVolume);

        bool fullscreen = System.Convert.ToBoolean(saveData.fullscreen);
        Screen.fullScreen = fullscreen;
        toggleFullScreen.SetIsOnWithoutNotify(fullscreen);

        dropDownResolution.SetValueWithoutNotify(saveData.resolution);
        SetResolution(saveData.resolution);
        //dropDownQuality.value = saveData.quality;
    }

    private float DecibelConvert(float volumeValue)
    {
        if (volumeValue <= 0.00001f)
            return -80f;

        return Mathf.Log10(volumeValue / maxValue) * parameterVolume;
    }

    private void SetFirstSaveFile()
    {
        //dropDownQuality.value = 1;

        Screen.fullScreen = false;
        toggleFullScreen.SetIsOnWithoutNotify(false);

        int monitorIndex = GetCurrentMonitorResolutionIndex();
        saveData.resolution = monitorIndex;
        dropDownResolution.SetValueWithoutNotify(monitorIndex);
        SetResolution(monitorIndex);

        SaveSettings();
    }
}

[Serializable]
public class SettingsSaveData
{
    public int language;

    public float volumeMaster;
    public float volumeMusic;
    public float volumeSFX;

    public int resolution;
    public int fullscreen;
    public int quality;

    public string difficulty;

    public bool skipCutScenes;
    public bool comments;
    public bool gamepadRumble;
}
