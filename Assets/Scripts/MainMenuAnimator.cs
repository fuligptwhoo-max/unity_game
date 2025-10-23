using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenuAnimator : MonoBehaviour
{
    [Header("Ссылки")]
    public CanvasGroup mainMenuCanvasGroup;
    
    [Header("Настройки анимации")]
    public float fadeInDuration = 2f;

    void Start()
    {
        // Убедимся, что меню полностью прозрачно в начале
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 0f;
        }
    }

    public void AnimateMenuAppearance()
    {
        StartCoroutine(FadeInMenu());
    }

    IEnumerator FadeInMenu()
    {
        if (mainMenuCanvasGroup == null) yield break;
        
        float time = 0;
        while (time < fadeInDuration)
        {
            mainMenuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeInDuration);
            time += Time.deltaTime;
            yield return null;
        }

        mainMenuCanvasGroup.alpha = 1f;
    }
}