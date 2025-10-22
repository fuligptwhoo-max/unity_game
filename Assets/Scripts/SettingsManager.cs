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
    public Button applyButton;
    
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions = new List<Resolution>();
    private List<int> refreshRates = new List<int> { 60, 75, 120, 144, 165, 240 };
    private bool isInitialized = false;
    
    // Текущие выбранные настройки
    private int selectedResolutionIndex = 0;
    private int selectedDisplayMode = 1;
    private int selectedRefreshRate = 0;
    
    // Ключи для PlayerPrefs
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string RESOLUTION_WIDTH_KEY = "ResolutionWidth";
    private const string RESOLUTION_HEIGHT_KEY = "ResolutionHeight";
    private const string DISPLAY_MODE_KEY = "DisplayMode";
    private const string REFRESH_RATE_KEY = "RefreshRate";
    
    void Start()
    {
        InitializeSettings();
    }
    
    void InitializeSettings()
    {
        // Находим компоненты
        if (backgroundAudio == null)
        {
            FindAudioSources();
        }
        
        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }
        
        settingsPanel.SetActive(false);
        
        // Инициализация слайдеров звука
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        
        // Инициализация выпадающих списков
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }
        if (displayModeDropdown != null)
        {
            displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
        }
        if (refreshRateDropdown != null)
        {
            refreshRateDropdown.onValueChanged.AddListener(OnRefreshRateChanged);
        }
        
        // Инициализация кнопок
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }
        
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplyDisplaySettings);
        }
        
        InitializeResolutionSettings();
        LoadSettings();
        isInitialized = true;
        
        Debug.Log("SettingsManager initialized in " + SceneManager.GetActiveScene().name);
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
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        
        foreach (AudioSource audioSource in allAudioSources)
        {
            // В Level1Scene ищем LevelBackgroundAudio или аналогичный
            if (audioSource.gameObject.CompareTag("BackgroundMusic") || 
                audioSource.gameObject.name.Contains("Background") ||
                audioSource.gameObject.name.Contains("Music") ||
                audioSource.gameObject.name.Contains("LevelBackground"))
            {
                backgroundAudio = audioSource;
                Debug.Log("Found background audio in " + SceneManager.GetActiveScene().name + ": " + audioSource.gameObject.name);
                break;
            }
        }
        
        // Если не нашли, используем первый доступный AudioSource
        if (backgroundAudio == null && allAudioSources.Length > 0)
        {
            backgroundAudio = allAudioSources[0];
            Debug.Log("Using first available audio source: " + backgroundAudio.gameObject.name);
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
        
        if (resolutionDropdown != null)
        {
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
        }
        
        // Настройки режима отображения
        if (displayModeDropdown != null)
        {
            displayModeDropdown.ClearOptions();
            List<string> displayOptions = new List<string> { "Полноэкранный", "Оконный без рамки", "Оконный" };
            displayModeDropdown.AddOptions(displayOptions);
            SetCurrentDisplayMode();
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
            SetCurrentRefreshRate();
        }
    }
    
    void SetCurrentDisplayMode()
    {
        if (displayModeDropdown == null) return;
        
        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.ExclusiveFullScreen:
                displayModeDropdown.value = 0;
                break;
            case FullScreenMode.FullScreenWindow:
                displayModeDropdown.value = 1;
                break;
            case FullScreenMode.Windowed:
                displayModeDropdown.value = 2;
                break;
        }
        displayModeDropdown.RefreshShownValue();
    }
    
    void SetCurrentRefreshRate()
    {
        if (refreshRateDropdown == null) return;
        
        int currentRefreshRate = (int)Screen.currentResolution.refreshRateRatio.value;
        int closestIndex = 0;
        int minDifference = int.MaxValue;
        
        for (int i = 0; i < refreshRates.Count; i++)
        {
            int difference = Mathf.Abs(refreshRates[i] - currentRefreshRate);
            if (difference < minDifference)
            {
                minDifference = difference;
                closestIndex = i;
            }
        }
        
        refreshRateDropdown.value = closestIndex;
        refreshRateDropdown.RefreshShownValue();
    }
    
    public void ToggleSettings()
    {
        if (!settingsPanel.activeSelf)
        {
            // При открытии настроек обновляем значения слайдеров
            RefreshVolumeSliders();
            settingsPanel.SetActive(true);
            StartCoroutine(FadeInSettings());
        }
        else
        {
            CloseSettings();
        }
    }
    
    void RefreshVolumeSliders()
    {
        // Принудительно обновляем слайдеры текущими значениями из PlayerPrefs
        float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(masterVol);
            if (masterVolumeText != null)
                masterVolumeText.text = Mathf.RoundToInt(masterVol * 100) + "%";
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(musicVol);
            if (musicVolumeText != null)
                musicVolumeText.text = Mathf.RoundToInt(musicVol * 100) + "%";
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(sfxVol);
            if (sfxVolumeText != null)
                sfxVolumeText.text = Mathf.RoundToInt(sfxVol * 100) + "%";
        }
        
        Debug.Log("Volume sliders refreshed: " + masterVol + ", " + musicVol + ", " + sfxVol);
    }
    
    public void CloseSettings()
    {
        StartCoroutine(FadeOutSettings());
    }
    
    void OnMasterVolumeChanged(float value)
    {
        float actualVolume = value;
        AudioListener.volume = actualVolume;
        
        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.RoundToInt(actualVolume * 100) + "%";
        
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, actualVolume);
        PlayerPrefs.Save();
        
        UpdateMusicVolume();
        UpdateSFXVolume();
        
        Debug.Log("Master volume changed to: " + actualVolume);
    }
    
    void OnMusicVolumeChanged(float value)
    {
        float actualVolume = value;
            
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(actualVolume * 100) + "%";
        
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, actualVolume);
        PlayerPrefs.Save();
        
        UpdateMusicVolume();
        
        Debug.Log("Music volume changed to: " + actualVolume);
    }
    
    void OnSFXVolumeChanged(float value)
    {
        float actualVolume = value;
        
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(actualVolume * 100) + "%";
        
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, actualVolume);
        PlayerPrefs.Save();
        
        UpdateSFXVolume();
        
        Debug.Log("SFX volume changed to: " + actualVolume);
    }
    
    void OnResolutionChanged(int index)
    {
        if (!isInitialized) return;
        selectedResolutionIndex = index;
        Debug.Log($"Resolution selected: {index}");
    }
    
    void OnDisplayModeChanged(int index)
    {
        if (!isInitialized) return;
        selectedDisplayMode = index;
        Debug.Log($"Display mode selected: {index}");
    }
    
    void OnRefreshRateChanged(int index)
    {
        if (!isInitialized) return;
        selectedRefreshRate = index;
        Debug.Log($"Refresh rate selected: {index}");
    }
    
    void UpdateMusicVolume()
    {
        if (backgroundAudio != null)
        {
            float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
            backgroundAudio.volume = musicVol * masterVol;
            Debug.Log("Updated music volume to: " + (musicVol * masterVol));
        }
        else
        {
            Debug.LogWarning("Background audio is null in UpdateMusicVolume!");
        }
    }
    
    void UpdateSFXVolume()
    {
        if (levelManager != null)
        {
            float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
            float finalVolume = sfxVol * masterVol;
            
            SetVideoPlayerVolume(levelManager.PrimaryVideoPlayer, finalVolume);
            SetVideoPlayerVolume(levelManager.SecondaryVideoPlayer, finalVolume);
            Debug.Log("Updated SFX volume for video players to: " + finalVolume);
        }
        else
        {
            Debug.LogWarning("LevelManager is null in UpdateSFXVolume!");
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
    
    public void ApplyDisplaySettings()
    {
        Debug.Log("ApplyDisplaySettings called!");
        
        if (selectedResolutionIndex >= 0 && selectedResolutionIndex < filteredResolutions.Count)
        {
            Resolution resolution = filteredResolutions[selectedResolutionIndex];
            FullScreenMode mode = GetFullScreenMode(selectedDisplayMode);
            int refreshRateValue = refreshRates[selectedRefreshRate];
            
            Debug.Log($"Applying settings: {resolution.width}x{resolution.height}, {mode}, {refreshRateValue}Hz");
            
            // Сохраняем настройки
            PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, resolution.width);
            PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, resolution.height);
            PlayerPrefs.SetInt(DISPLAY_MODE_KEY, selectedDisplayMode);
            PlayerPrefs.SetInt(REFRESH_RATE_KEY, refreshRateValue);
            PlayerPrefs.Save();
            
            // Применяем настройки
            ApplyResolution(resolution.width, resolution.height, mode, refreshRateValue);
            
            // Визуальная обратная связь
            if (applyButton != null)
            {
                TMP_Text buttonText = applyButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    string originalText = buttonText.text;
                    buttonText.text = "ПРИМЕНЕНО!";
                    StartCoroutine(RestoreButtonText(buttonText, originalText, 2f));
                }
            }
        }
        else
        {
            Debug.LogError("Invalid resolution index!");
        }
    }
    
    void ApplyResolution(int width, int height, FullScreenMode mode, int refreshRate)
    {
        try
        {
            // Используем новейший метод SetResolution
            RefreshRate rate = new RefreshRate { numerator = (uint)refreshRate, denominator = 1 };
            Screen.SetResolution(width, height, mode, rate);
            Debug.Log($"Resolution applied: {width}x{height}, {mode}, {refreshRate}Hz");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to apply resolution: {e.Message}");
            // Fallback: используем простой метод
            Screen.SetResolution(width, height, mode);
        }
    }
    
    IEnumerator RestoreButtonText(TMP_Text textComponent, string originalText, float delay)
    {
        yield return new WaitForSeconds(delay);
        textComponent.text = originalText;
    }
    
    FullScreenMode GetFullScreenMode(int index)
    {
        switch (index)
        {
            case 0: return FullScreenMode.ExclusiveFullScreen;
            case 1: return FullScreenMode.FullScreenWindow;
            case 2: return FullScreenMode.Windowed;
            default: return FullScreenMode.FullScreenWindow;
        }
    }
    
    void LoadSettings()
    {
        Debug.Log("Loading settings from PlayerPrefs in " + SceneManager.GetActiveScene().name);
        
        // Убедимся, что все ключи существуют
        if (!PlayerPrefs.HasKey(MASTER_VOLUME_KEY)) PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, 1f);
        if (!PlayerPrefs.HasKey(MUSIC_VOLUME_KEY)) PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, 1f);
        if (!PlayerPrefs.HasKey(SFX_VOLUME_KEY)) PlayerPrefs.SetFloat(SFX_VOLUME_KEY, 1f);
        if (!PlayerPrefs.HasKey(RESOLUTION_WIDTH_KEY)) 
        {
            PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, Screen.currentResolution.width);
            PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, Screen.currentResolution.height);
        }
        if (!PlayerPrefs.HasKey(DISPLAY_MODE_KEY)) PlayerPrefs.SetInt(DISPLAY_MODE_KEY, 1);
        if (!PlayerPrefs.HasKey(REFRESH_RATE_KEY)) PlayerPrefs.SetInt(REFRESH_RATE_KEY, 60);
        
        PlayerPrefs.Save();
        
        // Загружаем настройки громкости
        float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(masterVol);
            AudioListener.volume = masterVol;
        }
        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.RoundToInt(masterVol * 100) + "%";
        
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(musicVol);
        }
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(musicVol * 100) + "%";
        
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(sfxVol);
        }
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(sfxVol * 100) + "%";
        
        // Применяем настройки громкости
        UpdateMusicVolume();
        UpdateSFXVolume();
        
        // Загружаем настройки дисплея
        ApplySavedDisplaySettings();
        
        Debug.Log("Settings loaded successfully in " + SceneManager.GetActiveScene().name);
    }
    
    void ApplySavedDisplaySettings()
    {
        // Если есть сохраненные настройки дисплея - применяем их
        if (PlayerPrefs.HasKey(RESOLUTION_WIDTH_KEY) && PlayerPrefs.HasKey(RESOLUTION_HEIGHT_KEY))
        {
            int savedWidth = PlayerPrefs.GetInt(RESOLUTION_WIDTH_KEY, Screen.currentResolution.width);
            int savedHeight = PlayerPrefs.GetInt(RESOLUTION_HEIGHT_KEY, Screen.currentResolution.height);
            int savedDisplayMode = PlayerPrefs.GetInt(DISPLAY_MODE_KEY, 1);
            int savedRefreshRate = PlayerPrefs.GetInt(REFRESH_RATE_KEY, 60);
            
            FullScreenMode mode = GetFullScreenMode(savedDisplayMode);
            
            // Применяем сохраненные настройки
            ApplyResolution(savedWidth, savedHeight, mode, savedRefreshRate);
            
            Debug.Log($"Applied saved display settings: {savedWidth}x{savedHeight}, Mode: {mode}, Refresh Rate: {savedRefreshRate}Hz");
        }
    }
    
    IEnumerator FadeInSettings()
    {
        if (settingsCanvasGroup != null)
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
    }
    
    IEnumerator FadeOutSettings()
    {
        if (settingsCanvasGroup != null)
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
        }
        settingsPanel.SetActive(false);
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void GoToMainMenu()
    {
        Debug.Log("Returning to main menu...");
        
        // Устанавливаем флаг для LevelSelectionManager
        LevelSelectionManager.comingFromSettings = true;
        
        // Сохраняем настройки перед выходом
        PlayerPrefs.Save();
        
        // Загружаем сцену главного меню (SampleScene)
        SceneManager.LoadScene("SampleScene");
    }
}