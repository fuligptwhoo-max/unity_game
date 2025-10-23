using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelItem : MonoBehaviour
{
    [Header("References")]
    public Image background;
    public TMP_Text levelNameText;
    public Button button;
    
    [Header("Transparency Settings")]
    [Range(0, 1)] public float normalAlpha = 0.6f;
    [Range(0, 1)] public float selectedAlpha = 1f;
    
    private LevelSelectionManager manager;
    private int levelIndex;

    void Awake()
    {
        if (background == null) background = GetComponent<Image>();
        if (button == null) button = GetComponent<Button>();
        if (levelNameText == null) levelNameText = GetComponentInChildren<TMP_Text>();

        LayoutElement layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<LayoutElement>();
        }
        
        // Размеры 200x200
        layoutElement.preferredWidth = 200f;
        layoutElement.preferredHeight = 200f;
    }
    
    public void Initialize(LevelSelectionManager manager, int index, string levelName)
    {
        this.manager = manager;
        this.levelIndex = index;
        
        if (levelNameText != null)
            levelNameText.text = levelName;
        if (button != null)
            button.onClick.AddListener(OnClick);
            
        SetSelected(false);
    }
    
    void OnClick()
    {
        manager.SelectLevel(levelIndex);
    }
    
    public void SetSelected(bool selected)
    {
        transform.localScale = selected ? Vector3.one * 1.1f : Vector3.one;
        
        if (background != null)
        {
            Color color = background.color;
            color.a = selected ? selectedAlpha : normalAlpha;
            background.color = color;
        }
    }
    
    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }
}