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

    [Header("Настройки анимации")]
    public float titleFadeInDuration = 5f;    // Появление текста за 5 секунд
    public float buttonFadeInDuration = 2f;   // Появление кнопки за 2 секунды
    public float titleMoveDuration = 2f;
    public float menuFadeDuration = 2f;
    public Vector2 titleTargetPosition = new Vector2(0, 350f);
    public Vector2 titleTargetScale = new Vector2(0.4f, 0.4f);

    [HideInInspector]
    public Vector3 titleStartPosition;
    [HideInInspector]
    public Vector3 titleStartScale;
    private bool isTransitioning = false;

    void Start()
    {
        // Сохраняем начальные значения
        titleStartPosition = new Vector3(0, 100f, 0);
        titleStartScale = Vector3.one;

        // Устанавливаем начальную позицию
        if (titleText != null)
        {
            titleText.anchoredPosition = titleStartPosition;
            titleText.localScale = titleStartScale;
        }

        // Настраиваем кнопку
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        // Скрываем главное меню
        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);
        
        if (mainMenuCanvasGroup != null)
            mainMenuCanvasGroup.alpha = 0f;

        // Изначально скрываем текст и кнопку
        if (titleTextCanvasGroup != null)
            titleTextCanvasGroup.alpha = 0f;
        
        if (playButtonCanvasGroup != null)
        {
            playButtonCanvasGroup.alpha = 0f;
            playButton.interactable = false;
        }

        // Принудительное обновление
        Canvas.ForceUpdateCanvases();
    }

    public IEnumerator FadeInTitleText()
    {
        if (titleTextCanvasGroup == null) yield break;
        
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
        if (playButtonCanvasGroup == null) yield break;
        
        float time = 0;
        while (time < buttonFadeInDuration)
        {
            playButtonCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / buttonFadeInDuration);
            time += Time.deltaTime;
            yield return null;
        }
        playButtonCanvasGroup.alpha = 1f;
        if (playButton != null)
            playButton.interactable = true;
    }

    public void OnPlayButtonClicked()
    {
        if (!isTransitioning)
        {
            StartCoroutine(AnimateTitleTransition());
        }
    }

    IEnumerator AnimateTitleTransition()
    {
        isTransitioning = true;
        
        // Сначала плавно скрываем кнопку "Играть"
        yield return StartCoroutine(FadeOutPlayButton());

        // Анимация перемещения и уменьшения заголовка
        float time = 0;
        while (time < titleMoveDuration)
        {
            float t = time / titleMoveDuration;
            
            // Плавное перемещение вверх
            if (titleText != null)
            {
                titleText.anchoredPosition = Vector2.Lerp(titleStartPosition, titleTargetPosition, t);
                
                // Плавное уменьшение
                titleText.localScale = Vector3.Lerp(titleStartScale, titleTargetScale, t);
            }

            time += Time.deltaTime;
            yield return null;
        }

        // Устанавливаем финальные значения
        if (titleText != null)
        {
            titleText.anchoredPosition = titleTargetPosition;
            titleText.localScale = titleTargetScale;
        }

        // Показываем главное меню
        yield return StartCoroutine(ShowMainMenu());
        
        isTransitioning = false;
    }

    IEnumerator FadeOutPlayButton()
    {
        if (playButtonCanvasGroup == null) yield break;
        
        float time = 0;
        float fadeOutDuration = 1f;
        while (time < fadeOutDuration)
        {
            playButtonCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeOutDuration);
            time += Time.deltaTime;
            yield return null;
        }
        playButtonCanvasGroup.alpha = 0f;
        if (playButton != null)
            playButton.interactable = false;
    }

    IEnumerator ShowMainMenu()
    {
        if (mainMenuUI != null)
            mainMenuUI.SetActive(true);

        if (mainMenuCanvasGroup == null) yield break;
        
        float time = 0;
        while (time < menuFadeDuration)
        {
            mainMenuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / menuFadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        mainMenuCanvasGroup.alpha = 1f;
    }
}