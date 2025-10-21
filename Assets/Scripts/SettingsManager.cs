using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Основные ссылки")]
    public GameObject settingsPanel;
    public CanvasGroup settingsCanvasGroup;
    
    [Header("Audio Sources")]
    public AudioSource backgroundAudio;
    public LevelManager levelManager;
    
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
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        InitializeSettings();
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Переинициализируем настройки при загрузке новой сцены
        FindAudioSources();
        FindLevelManager();
        ApplyAllSettings();
    }
    
    void InitializeSettings()
    {
        FindAudioSources();
        FindLevelManager();
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        // Инициализация слайдеров
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        
        // Инициализация dropdown'ов
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }
        
        if (displayModeDropdown != null)
        {
            displayModeDropdown.onValueChanged.RemoveAllListeners();
            displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
        }
        
        if (refreshRateDropdown != null)
        {
            refreshRateDropdown.onValueChanged.RemoveAllListeners();
            refreshRateDropdown.onValueChanged.AddListener(OnRefreshRateChanged);
        }
        
        // Инициализация кнопок
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
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
        if (backgroundAudio != null) return;
        
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
    
    void FindLevelManager()
    {
        if (levelManager != null) return;
        levelManager = FindAnyObjectByType<LevelManager>();
    }
    
    void InitializeResolutionSettings()
    {
        if (resolutionDropdown == null) return;
        
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
        
        // Устанавливаем сохраненное значение или текущее
        int savedResolution = PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY, currentResolutionIndex);
        if (savedResolution < resolutionDropdown.options.Count)
        {
            resolutionDropdown.value = savedResolution;
        }
        else
        {
            resolutionDropdown.value = currentResolutionIndex;
        }
        resolutionDropdown.RefreshShownValue();
        
        // Настройки режима отображения
        if (displayModeDropdown != null)
        {
            displayModeDropdown.ClearOptions();
            displayModeDropdown.AddOptions(new List<string> { "Полноэкранный", "Оконный без рамки", "Оконный" });
            
            int savedDisplayMode = PlayerPrefs.GetInt(DISPLAY_MODE_KEY, 0);
            if (savedDisplayMode < displayModeDropdown.options.Count)
            {
                displayModeDropdown.value = savedDisplayMode;
            }
            displayModeDropdown.RefreshShownValue();
        }
        
        // Настройки частоты обновления
        if (refreshRateDropdown != null)
        {
            refreshRateDropdown.ClearOptions();
            List<string> refreshOptions = new List<string>();
            foreach (int rate in refreshRates)
            {
                refreshOptions.Add(rate + " Hz");
            }
            refreshRateDropdown.AddOptions(refreshOptions);
            
            int savedRefreshRate = PlayerPrefs.GetInt(REFRESH_RATE_KEY, 0);
            if (savedRefreshRate < refreshRateDropdown.options.Count)
            {
                refreshRateDropdown.value = savedRefreshRate;
            }
            refreshRateDropdown.RefreshShownValue();
        }
    }
    
    public void ToggleSettings()
    {
        if (settingsPanel == null) 
        {
            Debug.LogError("SettingsPanel is null!");
            return;
        }
        
        Debug.Log("Toggling settings panel. Current active state: " + settingsPanel.activeSelf);
        
        if (!settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(true);
            if (settingsCanvasGroup != null)
            {
                settingsCanvasGroup.alpha = 1f;
                settingsCanvasGroup.interactable = true;
                settingsCanvasGroup.blocksRaycasts = true;
            }
            Debug.Log("Settings panel opened");
        }
        else
        {
            CloseSettings();
        }
    }
    
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        Debug.Log("Settings panel closed");
    }
    
    void OnMasterVolumeChanged(float value)
    {
        float actualVolume = value;
        AudioListener.volume = actualVolume;
        
        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.RoundToInt(actualVolume * 100) + "%";
        
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, actualVolume);
        PlayerPrefs.Save();
        
        UpdateAllAudioVolumes();
    }
    
    void OnMusicVolumeChanged(float value)
    {
        float actualVolume = value;
        
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(actualVolume * 100) + "%";
        
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, actualVolume);
        PlayerPrefs.Save();
        
        UpdateAllAudioVolumes();
    }
    
    void OnSFXVolumeChanged(float value)
    {
        float actualVolume = value;
        
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(actualVolume * 100) + "%";
        
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, actualVolume);
        PlayerPrefs.Save();
        
        UpdateAllAudioVolumes();
    }
    
    void UpdateAllAudioVolumes()
    {
        float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 0.75f);
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.8f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);
        
        // Обновляем фоновую музыку
        if (backgroundAudio != null)
        {
            backgroundAudio.volume = musicVol * masterVol;
        }
        
        // Обновляем VideoPlayer'ы в LevelManager
        UpdateVideoPlayersVolume(sfxVol * masterVol);
        
        // Обновляем другие аудио источники в сцене
        UpdateSceneAudioVolumes(masterVol);
    }
    
    void UpdateVideoPlayersVolume(float volume)
    {
        if (levelManager != null)
        {
            SetVideoPlayerVolume(levelManager.PrimaryVideoPlayer, volume);
            SetVideoPlayerVolume(levelManager.SecondaryVideoPlayer, volume);
        }
        
        // Также обновляем VideoPlayer'ы в меню выбора уровня
        LevelSelectionManager levelSelection = FindAnyObjectByType<LevelSelectionManager>();
        if (levelSelection != null)
        {
            VideoPlayer menuVideoPlayer = levelSelection.GetComponentInChildren<VideoPlayer>();
            if (menuVideoPlayer != null)
            {
                SetVideoPlayerVolume(menuVideoPlayer, volume);
            }
        }
    }
    
    void UpdateSceneAudioVolumes(float masterVolume)
    {
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audioSource in allAudioSources)
        {
            // Не трогаем фоновую музыку - она уже обработана
            if (audioSource == backgroundAudio) continue;
            
            // Для остальных аудио источников применяем общую громкость
            if (!audioSource.gameObject.CompareTag("BackgroundMusic") &&
                !audioSource.gameObject.name.Contains("Background") &&
                !audioSource.gameObject.name.Contains("Music"))
            {
                audioSource.volume = masterVolume;
            }
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
            int refreshRateValue = GetSelectedRefreshRate();
            
            FullScreenMode mode = GetCurrentDisplayMode();
            
            // Используем новую версию SetResolution с RefreshRate
            RefreshRate refreshRate = new RefreshRate { numerator = (uint)refreshRateValue, denominator = 1 };
            Screen.SetResolution(resolution.width, resolution.height, mode, refreshRate);
            
            PlayerPrefs.SetInt(RESOLUTION_INDEX_KEY, index);
            PlayerPrefs.Save();
            
            Debug.Log($"Resolution changed to: {resolution.width}x{resolution.height} @ {refreshRateValue}Hz, Mode: {mode}");
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
        
        int refreshRateValue = refreshRates[index];
        Resolution currentResolution = Screen.currentResolution;
        FullScreenMode mode = GetCurrentDisplayMode();
        
        // Используем новую версию SetResolution с RefreshRate
        RefreshRate refreshRate = new RefreshRate { numerator = (uint)refreshRateValue, denominator = 1 };
        Screen.SetResolution(currentResolution.width, currentResolution.height, mode, refreshRate);
        
        PlayerPrefs.SetInt(REFRESH_RATE_KEY, index);
        PlayerPrefs.Save();
        
        Debug.Log($"Refresh rate changed to: {refreshRateValue}Hz");
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
        if (index >= 0 && index < refreshRates.Count)
            return refreshRates[index];
        return 60;
    }
    
    void LoadSettings()
    {
        Debug.Log("Loading settings from PlayerPrefs...");
        
        // Загружаем настройки громкости с корректными значениями по умолчанию
        float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 0.75f);
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.8f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);
        
        // Устанавливаем значения слайдеров
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVol;
            AudioListener.volume = masterVol;
        }
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVol;
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVol;
        
        // Обновляем тексты
        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.RoundToInt(masterVol * 100) + "%";
        
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(musicVol * 100) + "%";
        
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(sfxVol * 100) + "%";
        
        // Применяем настройки громкости
        UpdateAllAudioVolumes();
        
        // Применяем графические настройки
        ApplyGraphicsSettings();
        
        Debug.Log("Settings loaded successfully");
    }
    
    void ApplyGraphicsSettings()
    {
        // Применяем разрешение
        int resIndex = PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY, -1);
        if (resIndex != -1 && resIndex < filteredResolutions.Count)
        {
            Resolution resolution = filteredResolutions[resIndex];
            int refreshRateValue = GetSelectedRefreshRate();
            FullScreenMode mode = GetCurrentDisplayMode();
            
            // Используем новую версию SetResolution с RefreshRate
            RefreshRate refreshRate = new RefreshRate { numerator = (uint)refreshRateValue, denominator = 1 };
            Screen.SetResolution(resolution.width, resolution.height, mode, refreshRate);
        }
        
        // Применяем режим отображения
        int displayMode = PlayerPrefs.GetInt(DISPLAY_MODE_KEY, 0);
        OnDisplayModeChanged(displayMode);
    }
    
    void ApplyAllSettings()
    {
        LoadSettings();
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
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}