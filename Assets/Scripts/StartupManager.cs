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

        StartCoroutine(StartupRoutine());
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
            menuBackgroundAudioSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f) * PlayerPrefs.GetFloat("MasterVolume", 1f);
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

    void OnDestroy()
    {
        if (splashVideoPlayer != null)
            splashVideoPlayer.loopPointReached -= OnSplashVideoFinished;
    }
}