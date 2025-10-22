using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class StartupManager : MonoBehaviour
{
    public VideoPlayer splashVideoPlayer;
    public VideoPlayer menuBackgroundVideo;
    public GameObject titleScreen;
    public GameObject mainMenuUI;
    public GameObject settingsButton;

    public AudioSource splashAudioSource;
    public AudioSource menuBackgroundAudioSource;

    private bool isSplashFinished = false;
    private TitleScreenManager titleManager;

    void Start()
    {
        menuBackgroundVideo.gameObject.SetActive(true);
        menuBackgroundVideo.Prepare();
        
        if (titleScreen != null)
        {
            titleManager = titleScreen.GetComponent<TitleScreenManager>();
        }
        
        titleScreen.SetActive(false);
        mainMenuUI.SetActive(false);
        settingsButton.SetActive(false);
        
        splashVideoPlayer.gameObject.SetActive(true);

        splashVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        menuBackgroundVideo.audioOutputMode = VideoAudioOutputMode.None;

        splashVideoPlayer.loopPointReached += OnSplashVideoFinished;

        // Проверяем, нужно ли пропускать начальное видео (если пришли из настроек уровня)
        if (LevelSelectionManager.comingFromSettings)
        {
            LevelSelectionManager.comingFromSettings = false;
            StartCoroutine(SkipSplashAndShowMainMenu());
        }
        else
        {
            StartCoroutine(StartupRoutine());
        }
    }

    void OnSplashVideoFinished(VideoPlayer vp)
    {
        isSplashFinished = true;
    }

    IEnumerator StartupRoutine()
    {
        splashVideoPlayer.Prepare();
        yield return new WaitUntil(() => splashVideoPlayer.isPrepared);
        
        float videoDuration = (float)splashVideoPlayer.length;

        if (splashAudioSource != null)
        {
            splashAudioSource.Play();
            splashAudioSource.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        }

        splashVideoPlayer.Play();

        // Ждем пока до конца видео останется 5 секунд
        if (videoDuration > 5f)
        {
            yield return new WaitForSeconds(videoDuration - 5f);
        }
        
        // Показываем текст "MELURINO" за 5 секунд до конца
        yield return StartCoroutine(ShowTitleText());

        // Ждем окончания видео
        yield return new WaitUntil(() => isSplashFinished);

        if (splashAudioSource != null)
            splashAudioSource.Stop();

        // Показываем кнопку "ИГРАТЬ" после интро
        yield return StartCoroutine(ShowPlayButton());
    }

    IEnumerator ShowTitleText()
    {
        if (titleScreen != null && titleManager != null)
        {
            titleScreen.SetActive(true);
            yield return StartCoroutine(titleManager.FadeInTitleText());
        }
    }

    IEnumerator ShowPlayButton()
    {
        // Включаем и запускаем фоновое видео меню
        menuBackgroundVideo.gameObject.SetActive(true);
        menuBackgroundVideo.Play();
        menuBackgroundVideo.isLooping = true;
        
        if (menuBackgroundAudioSource != null)
        {
            menuBackgroundAudioSource.Play();
            float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
            menuBackgroundAudioSource.volume = musicVol * masterVol;
        }

        // Выключаем сплеш-скрин
        splashVideoPlayer.gameObject.SetActive(false);

        // Ждем 2 секунды перед показом кнопки
        yield return new WaitForSeconds(2f);

        // Показываем кнопку "ИГРАТЬ"
        if (titleManager != null)
        {
            yield return StartCoroutine(titleManager.FadeInPlayButton());
        }
        
        settingsButton.SetActive(true);
    }

    IEnumerator SkipSplashAndShowMainMenu()
    {
        // Сразу скрываем сплеш и показываем главное меню
        splashVideoPlayer.gameObject.SetActive(false);
        
        if (splashAudioSource != null)
            splashAudioSource.Stop();

        // Включаем и запускаем фоновое видео меню
        menuBackgroundVideo.gameObject.SetActive(true);
        menuBackgroundVideo.Play();
        menuBackgroundVideo.isLooping = true;
        
        if (menuBackgroundAudioSource != null)
        {
            menuBackgroundAudioSource.Play();
            float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
            menuBackgroundAudioSource.volume = musicVol * masterVol;
        }

    // Применяем сохраненные настройки дисплея
    ApplySavedDisplaySettings();

        // Применяем сохраненные настройки дисплея
        ApplySavedDisplaySettings();

        // Показываем главное меню сразу с правильной анимацией заголовка
        titleScreen.SetActive(true);
        
        // Запускаем анимацию заголовка как после нажатия "Играть"
        if (titleManager != null)
        {
            // Сразу устанавливаем заголовок в финальное положение
            titleManager.titleText.anchoredPosition = titleManager.titleTargetPosition;
            titleManager.titleText.localScale = titleManager.titleTargetScale;
            titleManager.titleTextCanvasGroup.alpha = 1f;
            
            // Скрываем кнопку "Играть"
            titleManager.playButtonCanvasGroup.alpha = 0f;
            titleManager.playButton.interactable = false;
            
            // Показываем главное меню
            titleManager.mainMenuUI.SetActive(true);
            titleManager.mainMenuCanvasGroup.alpha = 1f;
        }

        mainMenuUI.SetActive(true);
        settingsButton.SetActive(true);

        yield return null;
    }

void ApplySavedDisplaySettings()
{
    // Если есть сохраненные настройки дисплея - применяем их
    if (PlayerPrefs.HasKey("ResolutionWidth") && PlayerPrefs.HasKey("ResolutionHeight"))
    {
        int savedWidth = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
        int savedHeight = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);
        int savedDisplayMode = PlayerPrefs.GetInt("DisplayMode", 1);
        int savedRefreshRate = PlayerPrefs.GetInt("RefreshRate", 60);
        
        FullScreenMode mode = FullScreenMode.FullScreenWindow;
        switch (savedDisplayMode)
        {
            case 0: mode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: mode = FullScreenMode.FullScreenWindow; break;
            case 2: mode = FullScreenMode.Windowed; break;
        }
        
        // Применяем сохраненные настройки
        try
        {
            RefreshRate refreshRate = new RefreshRate { numerator = (uint)savedRefreshRate, denominator = 1 };
            Screen.SetResolution(savedWidth, savedHeight, mode, refreshRate);
            Debug.Log($"Applied saved display settings on startup: {savedWidth}x{savedHeight}, {mode}, {savedRefreshRate}Hz");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to apply resolution on startup: {e.Message}");
            Screen.SetResolution(savedWidth, savedHeight, mode);
        }
    }
}

    void OnDestroy()
    {
        if (splashVideoPlayer != null)
            splashVideoPlayer.loopPointReached -= OnSplashVideoFinished;
    }
}