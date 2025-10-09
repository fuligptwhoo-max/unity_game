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

    private Vector3 titleStartPosition;
    private Vector3 titleStartScale;
    private bool isTransitioning = false;

    void Start()
    {
        // Сохраняем начальные значения
        titleStartPosition = new Vector2(0, 100f);
        titleStartScale = Vector3.one;

        // Устанавливаем начальную позицию
        titleText.anchoredPosition = titleStartPosition;
        titleText.localScale = titleStartScale;

        // Настраиваем кнопку
        playButton.onClick.AddListener(OnPlayButtonClicked);

        // Скрываем главное меню
        mainMenuUI.SetActive(false);
        mainMenuCanvasGroup.alpha = 0f;

        // Изначально скрываем текст и кнопку
        titleTextCanvasGroup.alpha = 0f;
        playButtonCanvasGroup.alpha = 0f;
        playButton.interactable = false;

        // Принудительное обновление
        Canvas.ForceUpdateCanvases();
    }

    public IEnumerator FadeInTitleText()
    {
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
        float time = 0;
        while (time < buttonFadeInDuration)
        {
            playButtonCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / buttonFadeInDuration);
            time += Time.deltaTime;
            yield return null;
        }
        playButtonCanvasGroup.alpha = 1f;
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
            titleText.anchoredPosition = Vector2.Lerp(titleStartPosition, titleTargetPosition, t);
            
            // Плавное уменьшение
            titleText.localScale = Vector3.Lerp(titleStartScale, titleTargetScale, t);

            time += Time.deltaTime;
            yield return null;
        }

        // Устанавливаем финальные значения
        titleText.anchoredPosition = titleTargetPosition;
        titleText.localScale = titleTargetScale;

        // Показываем главное меню
        yield return StartCoroutine(ShowMainMenu());
        
        isTransitioning = false;
    }

    IEnumerator FadeOutPlayButton()
    {
        float time = 0;
        float fadeOutDuration = 1f;
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
    }
}