using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ExitGame : MonoBehaviour
{
    public GameObject exitDialogPanel;
    public CanvasGroup exitDialogCanvasGroup;
    public Button yesButton;
    public Button noButton;
    
    void Start()
    {
        // Проверяем, назначена ли панель, перед ее использованием
        if (exitDialogPanel != null)
        {
            exitDialogPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("ExitDialogPanel не назначена в инспекторе!");
        }
        
        // Настраиваем кнопки
        if (yesButton != null)
            yesButton.onClick.AddListener(ConfirmQuit);
            
        if (noButton != null)
            noButton.onClick.AddListener(CancelQuit);
    }
    
    public void ShowExitDialog()
    {
        if (exitDialogPanel != null)
        {
            exitDialogPanel.SetActive(true);
            StartCoroutine(FadeInExitDialog());
        }
    }
    
    public void ConfirmQuit()
    {
        Debug.Log("Подтвержден выход из игры");
        StartCoroutine(FadeOutAndQuit());
    }
    
    public void CancelQuit()
    {
        StartCoroutine(FadeOutExitDialog());
    }
    
    IEnumerator FadeInExitDialog()
    {
        if (exitDialogCanvasGroup != null)
        {
            exitDialogCanvasGroup.alpha = 0f;
            float duration = 0.2f;
            float time = 0;
            
            while (time < duration)
            {
                exitDialogCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            exitDialogCanvasGroup.alpha = 1f;
        }
    }
    
    IEnumerator FadeOutExitDialog()
    {
        if (exitDialogCanvasGroup != null)
        {
            float duration = 0.2f;
            float time = 0;
            
            while (time < duration)
            {
                exitDialogCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            exitDialogCanvasGroup.alpha = 0f;
        }
        
        if (exitDialogPanel != null)
            exitDialogPanel.SetActive(false);
    }
    
    IEnumerator FadeOutAndQuit()
    {
        if (exitDialogCanvasGroup != null)
        {
            float duration = 0.5f;
            float time = 0;
            
            while (time < duration)
            {
                exitDialogCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
        }
        
        QuitGame();
    }
    
    void QuitGame()
    {
        Debug.Log("Выход из игры");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}