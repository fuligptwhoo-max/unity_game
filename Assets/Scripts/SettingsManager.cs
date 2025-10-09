using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("Основные ссылки")]
    public GameObject settingsPanel;
    public CanvasGroup settingsCanvasGroup;
    
    [Header("Audio Sources")]
    public AudioSource backgroundAudio; // Фоновая музыка (универсальная)
    public LevelManager levelManager;   // Опционально, только в сцене уровня
    
    [Header("Настройки звука")]
    public Slider masterVolumeSlider;
    public TMP_Text masterVolumeText;
    public Slider musicVolumeSlider;
    public TMP_Text musicVolumeText;
    public Slider sfxVolumeSlider;
    public TMP_Text sfxVolumeText;
    
    [Header("Настройки дисплея")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown displayModeDropdown;
    public TMP_Dropdown refreshRateDropdown;
    
    [Header("Кнопки")]
    public Button mainMenuButton;
    public Button closeButton;
    
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions = new List<Resolution>();
    private List<int> refreshRates = new List<int> { 60, 75, 120, 144, 165, 240 };
    private bool isInitialized = false;
    
    // Ключи для PlayerPrefs
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string RESOLUTION_INDEX_KEY = "ResolutionIndex";
    private const string DISPLAY_MODE_KEY = "DisplayMode";
    private const string REFRESH_RATE_KEY = "RefreshRateIndex";
    
    void Start()
    {
        InitializeSettings();
    }
    
    void InitializeSettings()
    {
        // Автоматически находим компоненты если они не установлены
        if (backgroundAudio == null)
        {
            FindAudioSources();
        }
        
        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }
        
        settingsPanel.SetActive(false);
        
        // Инициализация слайдеров
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
        refreshRateDropdown.onValueChanged.AddListener(OnRefreshRateChanged);
        
        // Инициализация кнопок
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }
        
        InitializeResolutionSettings();
        LoadSettings();
        isInitialized = true;
        
        Debug.Log("SettingsManager initialized");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.F1))
        {
            ToggleSettings();
        }
    }
    
    void FindAudioSources()
    {
        // Ищем фоновую музыку
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource.gameObject.CompareTag("BackgroundMusic") || 
                audioSource.gameObject.name.Contains("Background") ||
                audioSource.gameObject.name.Contains("Music"))
            {
                backgroundAudio = audioSource;
                Debug.Log("Found background audio: " + audioSource.gameObject.name);
                break;
            }
        }
    }
    
    void InitializeResolutionSettings()
    {
        resolutions = Screen.resolutions;
        filteredResolutions.Clear();
        
        // Фильтруем разрешения, убираем дубликаты
        foreach (Resolution resolution in resolutions)
        {
            if (!filteredResolutions.Exists(r => r.width == resolution.width && r.height == resolution.height))
            {
                filteredResolutions.Add(resolution);
            }
        }
        
        resolutionDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string option = filteredResolutions[i].width + " x " + filteredResolutions[i].height;
            options.Add(option);
            
            if (filteredResolutions[i].width == Screen.currentResolution.width &&
                filteredResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        
        // Настройки режима отображения
        displayModeDropdown.ClearOptions();
        displayModeDropdown.AddOptions(new List<string> { "Полноэкранный", "Оконный без рамки", "Оконный" });
        
        // Настройки частоты обновления
        refreshRateDropdown.ClearOptions();
        List<string> refreshOptions = new List<string>();
        foreach (int rate in refreshRates)
        {
            refreshOptions.Add(rate + " Hz");
        }
        refreshRateDropdown.AddOptions(refreshOptions);
    }
    
    public void ToggleSettings()
    {
        if (!settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(true);
            StartCoroutine(FadeInSettings());
        }
        else
        {
            CloseSettings();
        }
    }
    
    public void CloseSettings()
    {
        StartCoroutine(FadeOutSettings());
    }
    
    void OnMasterVolumeChanged(float value)
    {
        float actualVolume = value;
        AudioListener.volume = actualVolume;
        
        masterVolumeText.text = Mathf.RoundToInt(actualVolume * 100) + "%";
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, actualVolume);
        PlayerPrefs.Save();
        
        UpdateMusicVolume();
    }
    
    void OnMusicVolumeChanged(float value)
    {
        float actualVolume = value;
        
        if (backgroundAudio != null)
            backgroundAudio.volume = actualVolume * PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            
        musicVolumeText.text = Mathf.RoundToInt(actualVolume * 100) + "%";
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, actualVolume);
        PlayerPrefs.Save();
    }
    
    void OnSFXVolumeChanged(float value)
    {
        float actualVolume = value;
        
        sfxVolumeText.text = Mathf.RoundToInt(actualVolume * 100) + "%";
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, actualVolume);
        PlayerPrefs.Save();
        
        UpdateSFXVolume();
    }
    
    void UpdateMusicVolume()
    {
        if (backgroundAudio != null)
        {
            float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
            backgroundAudio.volume = musicVol * masterVol;
        }
    }
    
    void UpdateSFXVolume()
    {
        // Обновляем громкость VideoPlayer'ов в LevelManager
        if (levelManager != null)
        {
            float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
            float finalVolume = sfxVol * masterVol;
            
            SetVideoPlayerVolume(levelManager.primaryVideoPlayer, finalVolume);
            SetVideoPlayerVolume(levelManager.secondaryVideoPlayer, finalVolume);
        }
    }
    
    void SetVideoPlayerVolume(VideoPlayer videoPlayer, float volume)
    {
        if (videoPlayer != null)
        {
            for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
            {
                videoPlayer.SetDirectAudioVolume(i, volume);
            }
        }
    }
    
    void OnResolutionChanged(int index)
    {
        if (!isInitialized) return;
        
        if (index >= 0 && index < filteredResolutions.Count)
        {
            Resolution resolution = filteredResolutions[index];
            int refreshRate = GetSelectedRefreshRate();
            
            // Используем новую версию SetResolution с RefreshRate
            Screen.SetResolution(resolution.width, resolution.height, GetCurrentDisplayMode(), 
                                new RefreshRate { numerator = (uint)refreshRate, denominator = 1 });
            PlayerPrefs.SetInt(RESOLUTION_INDEX_KEY, index);
            PlayerPrefs.Save();
            
            Debug.Log($"Resolution changed to: {resolution.width}x{resolution.height}");
        }
    }
    
    void OnDisplayModeChanged(int index)
    {
        if (!isInitialized) return;
        
        FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;
        switch (index)
        {
            case 0: mode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: mode = FullScreenMode.FullScreenWindow; break;
            case 2: mode = FullScreenMode.Windowed; break;
        }
        
        Screen.fullScreenMode = mode;
        PlayerPrefs.SetInt(DISPLAY_MODE_KEY, index);
        PlayerPrefs.Save();
        
        Debug.Log($"Display mode changed to: {mode}");
    }
    
    void OnRefreshRateChanged(int index)
    {
        if (!isInitialized) return;
        
        int refreshRate = refreshRates[index];
        Resolution currentResolution = Screen.currentResolution;
        
        // Используем новую версию SetResolution с RefreshRate
        Screen.SetResolution(currentResolution.width, currentResolution.height, GetCurrentDisplayMode(), 
                            new RefreshRate { numerator = (uint)refreshRate, denominator = 1 });
        PlayerPrefs.SetInt(REFRESH_RATE_KEY, index);
        PlayerPrefs.Save();
        
        Debug.Log($"Refresh rate changed to: {refreshRate}Hz");
    }
    
    FullScreenMode GetCurrentDisplayMode()
    {
        int displayMode = PlayerPrefs.GetInt(DISPLAY_MODE_KEY, 0);
        switch (displayMode)
        {
            case 0: return FullScreenMode.ExclusiveFullScreen;
            case 1: return FullScreenMode.FullScreenWindow;
            case 2: return FullScreenMode.Windowed;
            default: return FullScreenMode.FullScreenWindow;
        }
    }
    
    int GetSelectedRefreshRate()
    {
        int index = PlayerPrefs.GetInt(REFRESH_RATE_KEY, 0);
        return refreshRates[Mathf.Clamp(index, 0, refreshRates.Count - 1)];
    }
    
    void LoadSettings()
    {
        Debug.Log("Loading settings from PlayerPrefs...");
        
        // Загружаем настройки громкости
        float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        masterVolumeSlider.value = masterVol;
        AudioListener.volume = masterVol;
        masterVolumeText.text = Mathf.RoundToInt(masterVol * 100) + "%";
        
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        musicVolumeSlider.value = musicVol;
        musicVolumeText.text = Mathf.RoundToInt(musicVol * 100) + "%";
        
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        sfxVolumeSlider.value = sfxVol;
        sfxVolumeText.text = Mathf.RoundToInt(sfxVol * 100) + "%";
        
        // Применяем настройки громкости
        UpdateMusicVolume();
        UpdateSFXVolume();
        
        // Загружаем настройки дисплея
        int resIndex = PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY, -1);
        if (resIndex != -1 && resIndex < resolutionDropdown.options.Count)
        {
            resolutionDropdown.value = resIndex;
            resolutionDropdown.RefreshShownValue();
        }
        
        int displayMode = PlayerPrefs.GetInt(DISPLAY_MODE_KEY, 0);
        if (displayMode < displayModeDropdown.options.Count)
        {
            displayModeDropdown.value = displayMode;
            displayModeDropdown.RefreshShownValue();
            
            // Применяем режим отображения
            OnDisplayModeChanged(displayMode);
        }
        
        int refreshIndex = PlayerPrefs.GetInt(REFRESH_RATE_KEY, 0);
        if (refreshIndex < refreshRateDropdown.options.Count)
        {
            refreshRateDropdown.value = refreshIndex;
            refreshRateDropdown.RefreshShownValue();
        }
        
        Debug.Log("Settings loaded successfully");
    }
    
    IEnumerator FadeInSettings()
    {
        settingsCanvasGroup.alpha = 0f;
        float duration = 0.3f;
        float time = 0;
        
        while (time < duration)
        {
            settingsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        settingsCanvasGroup.alpha = 1f;
    }
    
    IEnumerator FadeOutSettings()
    {
        float duration = 0.3f;
        float time = 0;
        
        while (time < duration)
        {
            settingsCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        settingsCanvasGroup.alpha = 0f;
        settingsPanel.SetActive(false);
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void GoToMainMenu()
    {
        Debug.Log("Returning to main menu...");
        
        // Сохраняем настройки перед выходом
        PlayerPrefs.Save();
        
        // Загружаем главное меню
        SceneManager.LoadScene("SampleScene");
    }
}