using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class StartupManager : MonoBehaviour
{
    public VideoPlayer splashVideoPlayer;
    public VideoPlayer menuBackgroundVideo;
    public GameObject titleScreen;
    public GameObject mainMenuUI;

    public AudioSource splashAudioSource;
    public AudioSource menuBackgroundAudioSource;

    private bool isSplashFinished = false;
    private TitleScreenManager titleManager;

    void Start()
    {
        Debug.Log("=== STARTUP MANAGER START ===");
        
        // Создаем SettingsManager если его нет
        if (SettingsManager.Instance == null)
        {
            Debug.Log("Creating SettingsManager instance");
            GameObject settingsObj = new GameObject("SettingsManager");
            settingsObj.AddComponent<SettingsManager>();
            DontDestroyOnLoad(settingsObj);
        }
        else
        {
            Debug.Log("SettingsManager instance already exists");
        }

        // ... остальной код без изменений
        menuBackgroundVideo.gameObject.SetActive(true);
        menuBackgroundVideo.Prepare();
        
        if (titleScreen != null)
        {
            titleManager = titleScreen.GetComponent<TitleScreenManager>();
            Debug.Log("TitleScreenManager found: " + (titleManager != null));
        }
        else
        {
            Debug.LogError("TitleScreen reference is null!");
        }
        
        titleScreen.SetActive(false);
        mainMenuUI.SetActive(false);
        
        splashVideoPlayer.gameObject.SetActive(true);

        splashVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        menuBackgroundVideo.audioOutputMode = VideoAudioOutputMode.None;

        splashVideoPlayer.loopPointReached += OnSplashVideoFinished;

        StartCoroutine(StartupRoutine());
    }

    void OnSplashVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Splash video finished");
        isSplashFinished = true;
    }

    IEnumerator StartupRoutine()
    {
        Debug.Log("Starting startup routine");
        
        splashVideoPlayer.Prepare();
        yield return new WaitUntil(() => splashVideoPlayer.isPrepared);
        
        float videoDuration = (float)splashVideoPlayer.length;
        Debug.Log("Splash video duration: " + videoDuration);

        if (splashAudioSource != null)
        {
            splashAudioSource.Play();
            float masterVol = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            splashAudioSource.volume = masterVol;
        }

        splashVideoPlayer.Play();
        Debug.Log("Splash video started playing");

        // Ждем пока до конца видео останется 5 секунд
        if (videoDuration > 5f)
        {
            yield return new WaitForSeconds(videoDuration - 5f);
        }
        
        // Показываем текст "MELURINO" за 5 секунд до конца
        yield return StartCoroutine(ShowTitleText());

        // Ждем окончания видео
        yield return new WaitUntil(() => isSplashFinished);
        Debug.Log("Splash video completely finished");

        if (splashAudioSource != null)
            splashAudioSource.Stop();

        // Показываем кнопку "ИГРАТЬ" после интро
        yield return StartCoroutine(ShowPlayButton());
    }

    IEnumerator ShowTitleText()
    {
        Debug.Log("Showing title text");
        if (titleScreen != null && titleManager != null)
        {
            titleScreen.SetActive(true);
            yield return StartCoroutine(titleManager.FadeInTitleText());
        }
    }

    IEnumerator ShowPlayButton()
    {
        Debug.Log("Showing play button");
        
        // Включаем и запускаем фоновое видео меню
        menuBackgroundVideo.gameObject.SetActive(true);
        menuBackgroundVideo.Play();
        menuBackgroundVideo.isLooping = true;
        
        if (menuBackgroundAudioSource != null)
        {
            menuBackgroundAudioSource.Play();
            float masterVol = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            menuBackgroundAudioSource.volume = musicVol * masterVol;
        }

        // Выключаем сплеш-скрин
        splashVideoPlayer.gameObject.SetActive(false);

        // Ждем 2 секунды перед показом кнопки
        yield return new WaitForSeconds(2f);

        // Показываем кнопку "ИГРАТЬ" (кнопка настроек появится ПОСЛЕ нажатия на "Играть")
        if (titleManager != null)
        {
            yield return StartCoroutine(titleManager.FadeInPlayButton());
        }
        else
        {
            Debug.LogError("TitleManager is null in ShowPlayButton!");
        }
    }

    void OnDestroy()
    {
        if (splashVideoPlayer != null)
            splashVideoPlayer.loopPointReached -= OnSplashVideoFinished;
    }
}