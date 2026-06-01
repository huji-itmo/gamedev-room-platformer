using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Для загрузки сцен
using TMPro;
using System.Collections.Generic;
using UnityEngine.Audio;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

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
    // 0 = скрыто (если нужно), 1 = главное меню, 2 = настройки
    public int mainMenuState = 1;
    public int settingsState = 2;

    [Header("Game Settings")]
    [Tooltip("Имя или индекс сцены, которая загрузится при нажатии Играть")]
    public string gameSceneName = "GameLevel01";

    private Resolution[] resolutions;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // В главном меню обычно не нужен DontDestroyOnLoad, если меню не должно висеть во время игры.
        // Если нужно, раскомментируй строку ниже:
        // DontDestroyOnLoad(gameObject); 

        LoadSettings();
        PopulateResolutions();
    }

    private void Start()
    {
        // Убеждаемся, что время не заморожено (на случай возврата из игры)
        Time.timeScale = 1f;
        SetScreenState(mainMenuState);
    }

    #region ⬇️ NAVIGATION & GAME LOOP ⬇️

    /// <summary>
    /// Загружает игровую сцену.
    /// Назначь на кнопку "Play" / "Start".
    /// </summary>
    public void StartGame()
    {
        // Сохраняем настройки перед выходом
        SaveSettings();
        
        // Загружаем сцену (асинхронно или нет)
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Выход из приложения.
    /// Назначь на кнопку "Quit".
    /// </summary>
    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Переключение на экран настроек.
    /// Назначь на кнопку "Settings".
    /// </summary>
    public void OpenSettings() => SetScreenState(settingsState);

    /// <summary>
    /// Возврат в главное меню.
    /// Назначь на кнопку "Back" в настройках.
    /// </summary>
    public void GoBackToMain() => SetScreenState(mainMenuState);

    private void SetScreenState(int state)
    {
        if (menuAnimator) menuAnimator.SetInteger(screenIdParamName, state);
    }

    #endregion

    #region ⬇️ PUBLIC VOID METHODS (read from references) ⬇️
    // Эти методы читают значения напрямую из UI-элементов.
    // Идеально подходят для Button.OnClick() или событий, где не нужны параметры.

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
        if (!resolutionDropdown || resolutions == null) return;
        int index = resolutionDropdown.value;
        if (index < 0 || index >= resolutions.Length) return;
        
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void SaveSettings()
    {
        if (sfxSlider) PlayerPrefs.SetFloat("SFX_Volume", sfxSlider.value);
        if (musicSlider) PlayerPrefs.SetFloat("Music_Volume", musicSlider.value);
        PlayerPrefs.Save();
    }
    #endregion

    #region ⬇️ OPTIONAL: Parameterized versions (for Slider.onValueChanged) ⬇️
    // Если хочешь, чтобы звук менялся мгновенно при перетаскивании ползунка,
    // используй эти методы в событии Slider.onValueChanged(float).

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
