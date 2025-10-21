using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    [Header("Ссылки")]
    public GameObject titleScreen;
    public RectTransform titleText;
    public CanvasGroup titleTextCanvasGroup;
    public Button playButton;
    public CanvasGroup playButtonCanvasGroup;
    public GameObject mainMenuUI;
    public CanvasGroup mainMenuCanvasGroup;
    public Button settingsButton;

    [Header("Настройки анимации")]
    public float titleFadeInDuration = 5f;
    public float buttonFadeInDuration = 2f;
    public float titleMoveDuration = 2f;
    public float menuFadeDuration = 2f;
    public Vector2 titleTargetPosition = new Vector2(0, 350f);
    public Vector2 titleTargetScale = new Vector2(0.4f, 0.4f);

    private Vector3 titleStartPosition;
    private Vector3 titleStartScale;
    private bool isTransitioning = false;

    void Start()
    {
        Debug.Log("TitleScreenManager Start called");
        
        // Инициализация
        titleStartPosition = new Vector2(0, 100f);
        titleStartScale = Vector3.one;

        titleText.anchoredPosition = titleStartPosition;
        titleText.localScale = titleStartScale;

        // Настраиваем кнопки
        playButton.onClick.AddListener(OnPlayButtonClicked);
        
        // Настраиваем кнопку настроек
        if (settingsButton != null)
        {
            Debug.Log("Settings button found, setting up listener");
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            settingsButton.gameObject.SetActive(false); // Скрываем изначально
        }
        else
        {
            Debug.LogError("SettingsButton is NULL in TitleScreenManager!");
        }

        // Скрываем UI элементы
        mainMenuUI.SetActive(false);
        mainMenuCanvasGroup.alpha = 0f;
        titleTextCanvasGroup.alpha = 0f;
        playButtonCanvasGroup.alpha = 0f;
        playButton.interactable = false;

        Canvas.ForceUpdateCanvases();
        Debug.Log("TitleScreenManager initialization complete");
    }

    public void OnSettingsButtonClicked()
    {
        Debug.Log("Settings button clicked!");
        
        // Используем FindAnyObjectByType вместо устаревшего FindObjectOfType
        SettingsManager settingsManager = SettingsManager.Instance;
        
        if (settingsManager == null)
        {
            Debug.Log("SettingsManager.Instance is null, trying to find manually...");
            settingsManager = FindAnyObjectByType<SettingsManager>();
        }
        
        if (settingsManager != null)
        {
            Debug.Log("SettingsManager found, calling ToggleSettings");
            settingsManager.ToggleSettings();
        }
        else
        {
            Debug.LogError("SettingsManager not found in scene!");
        }
    }

    public IEnumerator FadeInTitleText()
    {
        Debug.Log("Fading in title text");
        float time = 0;
        while (time < titleFadeInDuration)
        {
            titleTextCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / titleFadeInDuration);
            time += Time.deltaTime;
            yield return null;
        }
        titleTextCanvasGroup.alpha = 1f;
    }

    public IEnumerator FadeInPlayButton()
    {
        Debug.Log("Fading in play button");
        float time = 0;
        while (time < buttonFadeInDuration)
        {
            playButtonCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / buttonFadeInDuration);
            time += Time.deltaTime;
            yield return null;
        }
        playButtonCanvasGroup.alpha = 1f;
        playButton.interactable = true;
        
        Debug.Log("Play button faded in");
    }

    public void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked");
        if (!isTransitioning)
        {
            StartCoroutine(AnimateTitleTransition());
        }
    }

    IEnumerator AnimateTitleTransition()
    {
        isTransitioning = true;
        Debug.Log("Starting title transition animation");
        
        // Сначала скрываем кнопки
        yield return StartCoroutine(FadeOutButtons());

        // Анимация заголовка
        float time = 0;
        while (time < titleMoveDuration)
        {
            float t = time / titleMoveDuration;
            titleText.anchoredPosition = Vector2.Lerp(titleStartPosition, titleTargetPosition, t);
            titleText.localScale = Vector3.Lerp(titleStartScale, titleTargetScale, t);
            time += Time.deltaTime;
            yield return null;
        }

        titleText.anchoredPosition = titleTargetPosition;
        titleText.localScale = titleTargetScale;

        // Показываем главное меню (ВКЛЮЧАЯ КНОПКУ НАСТРОЕК)
        yield return StartCoroutine(ShowMainMenu());
        
        isTransitioning = false;
        Debug.Log("Title transition animation complete");
    }

    IEnumerator FadeOutButtons()
    {
        float time = 0;
        float fadeOutDuration = 1f;
        
        // Скрываем кнопку "Играть"
        while (time < fadeOutDuration)
        {
            playButtonCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeOutDuration);
            time += Time.deltaTime;
            yield return null;
        }
        playButtonCanvasGroup.alpha = 0f;
        playButton.interactable = false;
    }

    IEnumerator ShowMainMenu()
    {
        mainMenuUI.SetActive(true);

        float time = 0;
        while (time < menuFadeDuration)
        {
            mainMenuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / menuFadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        mainMenuCanvasGroup.alpha = 1f;
        
        // ВКЛЮЧАЕМ КНОПКУ НАСТРОЕК ПОСЛЕ ТОГО КАК ГЛАВНОЕ МЕНЮ ПОЯВИЛОСЬ
        if (settingsButton != null)
        {
            Debug.Log("Activating settings button in main menu");
            settingsButton.gameObject.SetActive(true);
            
            // Убедимся, что кнопка видима и интерактивна
            CanvasGroup canvasGroup = settingsButton.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            // Принудительно обновляем кнопку
            settingsButton.interactable = true;
            
            // Проверяем компоненты кнопки
            Image buttonImage = settingsButton.GetComponent<Image>();
            if (buttonImage != null) 
            {
                buttonImage.raycastTarget = true;
                Debug.Log("Button image raycast target: " + buttonImage.raycastTarget);
            }
            
            Debug.Log("Settings button should be visible and clickable now");
        }
        else
        {
            Debug.LogError("SettingsButton is null when trying to show it in main menu!");
        }
    }
}