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
    private Resolution[] resolutions;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

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

    #region ⬇️ PUBLIC VOID METHODS (read from references) ⬇️
    /// <summary>
    /// Reads value from sfxSlider reference and applies to AudioMixer.
    /// Can be called from Button.OnClick() or any void event.
    /// </summary>
    public void UpdateSFXVolume()
    {
        if (!sfxSlider || !audioMixer) return;
        audioMixer.SetFloat(sfxMixerParam, ConvertVolumeToDB(sfxSlider.value));
    }

    /// <summary>
    /// Reads value from musicSlider reference and applies to AudioMixer.
    /// </summary>
    public void UpdateMusicVolume()
    {
        if (!musicSlider || !audioMixer) return;
        audioMixer.SetFloat(musicMixerParam, ConvertVolumeToDB(musicSlider.value));
    }

    /// <summary>
    /// Reads value from fullscreenToggle reference and applies to Screen.
    /// </summary>
    public void UpdateFullscreenMode()
    {
        if (!fullscreenToggle) return;
        Screen.fullScreen = fullscreenToggle.isOn;
    }

    /// <summary>
    /// Reads value from resolutionDropdown reference and applies to Screen.
    /// </summary>
    public void UpdateResolution()
    {
        if (!resolutionDropdown || resolutions == null) return;
        int index = resolutionDropdown.value;
        if (index < 0 || index >= resolutions.Length) return;
        
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    /// <summary>
    /// Saves current slider values to PlayerPrefs.
    /// </summary>
    public void SaveSettings()
    {
        if (sfxSlider) PlayerPrefs.SetFloat("SFX_Volume", sfxSlider.value);
        if (musicSlider) PlayerPrefs.SetFloat("Music_Volume", musicSlider.value);
        PlayerPrefs.Save();
    }
    #endregion

    #region ⬇️ OPTIONAL: Parameterized versions for onValueChanged events ⬇️
    /// Use these if you prefer wiring Slider.onValueChanged(float) directly
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
        if (index < 0 || index >= resolutions.Length) return;
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
    #endregion

    #region Helpers & Initialization
    private float ConvertVolumeToDB(float volume) => volume <= 0.01f ? -80f : Mathf.Log10(volume) * 20f;

    private void LoadSettings()
    {
        if (fullscreenToggle) fullscreenToggle.isOn = Screen.fullScreen;

        if (sfxSlider)
        {
            float v = PlayerPrefs.GetFloat("SFX_Volume", 1f);
            sfxSlider.value = v;
            UpdateSFXVolume(); // Apply to mixer on load
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
        resolutions = Screen.resolutions;
        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add($"{resolutions[i].width}x{resolutions[i].height} @ {resolutions[i].refreshRateRatio}");
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentResIndex = i;
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();
    }
    #endregion
}