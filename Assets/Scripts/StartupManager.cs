using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class StartupManager : MonoBehaviour
{
    public VideoPlayer splashVideoPlayer;
    public VideoPlayer menuBackgroundVideo;
    public GameObject mainMenuUI;
    public GameObject settingsButton;

    public AudioSource splashAudioSource;
    public AudioSource menuBackgroundAudioSource;

    private bool isSplashFinished = false;
    private MainMenuAnimator menuAnimator;

    void Start()
    {
        // Сначала скрываем всё
        mainMenuUI.SetActive(false);
        settingsButton.SetActive(false);
        menuBackgroundVideo.gameObject.SetActive(false);
        
        splashVideoPlayer.gameObject.SetActive(true);

        splashVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        menuBackgroundVideo.audioOutputMode = VideoAudioOutputMode.None;

        splashVideoPlayer.loopPointReached += OnSplashVideoFinished;

        // Проверяем, нужно ли пропускать начальное видео
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
        
        if (splashAudioSource != null)
        {
            splashAudioSource.Play();
            splashAudioSource.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        }

        splashVideoPlayer.Play();

        // Ждем окончания видео
        yield return new WaitUntil(() => isSplashFinished);

        if (splashAudioSource != null)
            splashAudioSource.Stop();

        // Переключаемся на фоновое видео меню
        splashVideoPlayer.gameObject.SetActive(false);
        
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

        // Ждем 2 секунды перед показом главного меню
        yield return new WaitForSeconds(2f);

        // Показываем главное меню
        ShowMainMenu();
    }

    IEnumerator SkipSplashAndShowMainMenu()
    {
        // Сразу скрываем сплеш
        splashVideoPlayer.gameObject.SetActive(false);
        
        if (splashAudioSource != null)
            splashAudioSource.Stop();

        // Включаем фоновое видео меню
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

        // Ждем 2 секунды
        yield return new WaitForSeconds(2f);

        // Показываем главное меню
        ShowMainMenu();
    }

    void ShowMainMenu()
    {
        // Активируем главное меню
        mainMenuUI.SetActive(true);
        settingsButton.SetActive(true);
        
        // Запускаем анимацию появления через отдельный скрипт на активном объекте
        menuAnimator = mainMenuUI.GetComponent<MainMenuAnimator>();
        if (menuAnimator != null)
        {
            menuAnimator.AnimateMenuAppearance();
        }
    }

    void ApplySavedDisplaySettings()
    {
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