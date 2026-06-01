using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Audio;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    public static System.Action OnGameResumed;

    [Header("References")]
    public Animator menuAnimator;

    [Header("Settings UI")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Slider sfxSlider;
    public Slider musicSlider;

    [Header("Audio & Animator")]
    public AudioMixer audioMixer;
    public string screenIdParamName = "ScreenState";
    public string sfxMixerParam = "SFXVolume";
    public string musicMixerParam = "MusicVolume";

    [Header("Screen States IDs")]
    public int hiddenState = 0;
    public int mainMenuState = 1;
    public int settingsState = 2;

    [Header("Settings")]
    public bool startPaused = false;

    private bool isPaused = false;

    [System.Serializable]
    private struct ResolutionOption
    {
        public int width;
        public int height;
        public int refreshRate;
    }
    private List<ResolutionOption> _resolutionOptions;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        LoadSettings();
        PopulateResolutions();
    }

    private void Start()
    {
        SetScreenState(hiddenState);
        if (!startPaused) ResumeGame();
        else PauseGame();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            HandleEscapeInput();
    }

    #region Core Pause Logic
    public void TogglePause() { if (isPaused) ResumeGame(); else PauseGame(); }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetScreenState(mainMenuState);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetScreenState(hiddenState);
        OnGameResumed?.Invoke();
    }

    public bool IsPaused() => isPaused;
    #endregion

    #region Input & Navigation
    private void HandleEscapeInput()
    {
        int currentState = GetCurrentState();
        if (currentState == settingsState) GoBackToMain();
        else if (currentState == mainMenuState) ResumeGame();
        else PauseGame();
    }

    private int GetCurrentState() => menuAnimator ? menuAnimator.GetInteger(screenIdParamName) : hiddenState;
    private void SetScreenState(int state) { if (menuAnimator) menuAnimator.SetInteger(screenIdParamName, state); }

    public void OpenSettings() => SetScreenState(settingsState);
    public void GoBackToMain() => SetScreenState(mainMenuState);

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    #endregion

    public void UpdateSFXVolume()
    {
        if (!sfxSlider || !audioMixer) return;
        audioMixer.SetFloat(sfxMixerParam, ConvertVolumeToDB(sfxSlider.value));
    }

    public void UpdateMusicVolume()
    {
        if (!musicSlider || !audioMixer) return;
        audioMixer.SetFloat(musicMixerParam, ConvertVolumeToDB(musicSlider.value));
    }

    public void UpdateFullscreenMode()
    {
        if (!fullscreenToggle) return;
        Screen.fullScreen = fullscreenToggle.isOn;
    }

    public void UpdateResolution()
    {
        if (!resolutionDropdown || _resolutionOptions == null) return;
        int index = resolutionDropdown.value;
        if (index < 0 || index >= _resolutionOptions.Count) return;

        var opt = _resolutionOptions[index];
        Screen.SetResolution(opt.width, opt.height, Screen.fullScreenMode, new RefreshRate { numerator = (uint)opt.refreshRate, denominator = 1 });
    }

    public void SaveSettings()
    {
        if (sfxSlider) PlayerPrefs.SetFloat("SFX_Volume", sfxSlider.value);
        if (musicSlider) PlayerPrefs.SetFloat("Music_Volume", musicSlider.value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        if (!audioMixer) return;
        audioMixer.SetFloat(sfxMixerParam, ConvertVolumeToDB(value));
    }

    public void SetMusicVolume(float value)
    {
        if (!audioMixer) return;
        audioMixer.SetFloat(musicMixerParam, ConvertVolumeToDB(value));
    }

    public void SetFullscreenMode(bool isFullscreen) => Screen.fullScreen = isFullscreen;

    public void ChangeResolution(int index)
    {
        if (index < 0 || index >= _resolutionOptions.Count) return;
        var opt = _resolutionOptions[index];
        Screen.SetResolution(opt.width, opt.height, Screen.fullScreenMode, new RefreshRate { numerator = (uint)opt.refreshRate, denominator = 1 });
    }

    #region Helpers & Initialization
    private float ConvertVolumeToDB(float volume) => volume <= 0.01f ? -80f : Mathf.Log10(volume) * 20f;

    private void LoadSettings()
    {
        if (fullscreenToggle) fullscreenToggle.isOn = Screen.fullScreen;

        if (sfxSlider)
        {
            float v = PlayerPrefs.GetFloat("SFX_Volume", 1f);
            sfxSlider.value = v;
            UpdateSFXVolume();
        }

        if (musicSlider)
        {
            float v = PlayerPrefs.GetFloat("Music_Volume", 1f);
            musicSlider.value = v;
            UpdateMusicVolume();
        }
    }

    private void PopulateResolutions()
    {
        if (!resolutionDropdown) return;

        int[] commonRefreshRates = { 60, 75, 90, 100, 120, 144, 165, 200, 240 };
        var systemResolutions = Screen.resolutions;

        var seen = new HashSet<(int, int)>();
        _resolutionOptions = new List<ResolutionOption>();
        List<string> options = new List<string>();
        int currentResIndex = 0;

        foreach (var res in systemResolutions)
        {
            var key = (res.width, res.height);
            if (!seen.Contains(key))
            {
                seen.Add(key);
                foreach (int hz in commonRefreshRates)
                {
                    _resolutionOptions.Add(new ResolutionOption { width = res.width, height = res.height, refreshRate = hz });
                    options.Add($"{res.width}x{res.height} @ {hz}Hz");
                }
            }
        }

        for (int i = 0; i < _resolutionOptions.Count; i++)
        {
            var opt = _resolutionOptions[i];
            if (opt.width == Screen.currentResolution.width &&
                opt.height == Screen.currentResolution.height &&
                Mathf.Approximately((float)Screen.currentResolution.refreshRateRatio.value, opt.refreshRate))
            {
                currentResIndex = i;
                break;
            }
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();
    }
    #endregion
}
