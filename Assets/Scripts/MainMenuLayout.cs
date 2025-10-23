using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuLayout : MonoBehaviour
{
    [Header("Layout References")]
    public RectTransform levelsPanel;
    public RectTransform previewPanel;
    public Button levelsToggleButton;
    
    [Header("Layout Settings")]
    public Vector2 levelsPanelPosition = new Vector2(-350, 0);
    public Vector2 previewPanelPosition = new Vector2(100, 50);
    public Vector2 previewSize = new Vector2(600, 400);
    
    void Start()
    {
        SetupLayout();
    }
    
    void SetupLayout()
    {
        // Настраиваем панель уровней слева
        if (levelsPanel != null)
        {
            levelsPanel.anchoredPosition = levelsPanelPosition;
        }
        
        // Настраиваем панель превью по центру-правой части
        if (previewPanel != null)
        {
            previewPanel.anchoredPosition = previewPanelPosition;
            
            // Настраиваем размер превью
            LayoutElement layoutElement = previewPanel.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = previewPanel.gameObject.AddComponent<LayoutElement>();
            }
            layoutElement.preferredWidth = previewSize.x;
            layoutElement.preferredHeight = previewSize.y;
        }
        
        // Настраиваем кнопку переключения панели уровней
        if (levelsToggleButton != null)
        {
            // Можно добавить иконку или настроить внешний вид кнопки
            TMP_Text buttonText = levelsToggleButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = "Уровни";
            }
        }
    }
}