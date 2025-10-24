using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelSelectionManager : MonoBehaviour
{
    [Header("Preview Area")]
    public RawImage videoDisplay;
    public Button playLevelButton;
    public TMP_Text playButtonText;
    public TMP_Text levelDescriptionText;
    
    [Header("Levels Panel")]
    public ScrollRect levelsScrollRect;
    public Transform levelsContent;
    public GameObject levelItemPrefab;
    
    [Header("Level Data")]
    public List<LevelData> levels = new List<LevelData>();
    
    [Header("Audio")]
    public AudioSource audioSource;
    
    private int currentLevelIndex = -1; // -1 означает, что ничего не выбрано
    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    
    public static bool comingFromSettings = false;
    
    void Start()
    {
        InitializeComponents();
        CreateLevelItems();
        
        // УБРАНО: автоматический выбор первого уровня
        SetPreviewVisible(false);
        
        // Если пришли из настроек, тоже не выбираем уровень автоматически
        if (comingFromSettings)
        {
            comingFromSettings = false;
        }
    }
    
    void InitializeComponents()
    {
        renderTexture = new RenderTexture(1024, 1024, 24);
        
        if (videoDisplay != null)
        {
            videoDisplay.texture = renderTexture;
            
            videoPlayer = videoDisplay.GetComponent<VideoPlayer>();
            if (videoPlayer == null)
                videoPlayer = videoDisplay.gameObject.AddComponent<VideoPlayer>();
            
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }
        
        if (playLevelButton != null)
        {
            playLevelButton.onClick.RemoveAllListeners();
            playLevelButton.onClick.AddListener(OnPlayLevelClicked);
        }
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // Настраиваем вертикальный скролл
        if (levelsScrollRect != null)
        {
            levelsScrollRect.vertical = true;
            levelsScrollRect.horizontal = false;
        }
        
        // Настраиваем layout контента
        FixContentLayout();
    }
    
    void FixContentLayout()
    {
        if (levelsContent == null) return;
        
        // Удаляем горизонтальный layout если есть
        HorizontalLayoutGroup horizontal = levelsContent.GetComponent<HorizontalLayoutGroup>();
        if (horizontal != null)
        {
            DestroyImmediate(horizontal);
        }
        
        // Добавляем вертикальный layout
        VerticalLayoutGroup vertical = levelsContent.GetComponent<VerticalLayoutGroup>();
        if (vertical == null)
        {
            vertical = levelsContent.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        
        // Настраиваем для центрирования
        vertical.padding = new RectOffset(10, 10, 20, 20);
        vertical.spacing = 50f;
        vertical.childAlignment = TextAnchor.MiddleCenter;
        vertical.childControlWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandWidth = false;
        vertical.childForceExpandHeight = false;
        
        // Content Size Fitter для автоматической высоты
        ContentSizeFitter fitter = levelsContent.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = levelsContent.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Настраиваем RectTransform Content для центрирования
        RectTransform contentRect = levelsContent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.anchoredPosition = new Vector2(0f, 0f);
        }
    }
    
    void CreateLevelItems()
    {
        if (levelsContent == null)
        {
            Debug.LogError("LevelsContent is not assigned!");
            return;
        }
        
        LevelItem[] existingItems = levelsContent.GetComponentsInChildren<LevelItem>();
        Debug.Log($"Found {existingItems.Length} existing level items");
        
        for (int i = 0; i < existingItems.Length && i < levels.Count; i++)
        {
            LevelItem item = existingItems[i];
            item.Initialize(this, i, levels[i].levelName);
        }
        
        if (existingItems.Length < levels.Count)
        {
            Debug.LogWarning($"Not enough level items! Found {existingItems.Length}, but need {levels.Count}");
        }
    }
    
    public void SelectLevel(int levelIndex)
    {
        // ЕСЛИ НАЖИМАЕМ НА УЖЕ ВЫБРАННЫЙ УРОВЕНЬ - СНИМАЕМ ВЫБОР
        if (currentLevelIndex == levelIndex)
        {
            DeselectLevel();
            return;
        }
        
        if (levelIndex < 0 || levelIndex >= levels.Count) 
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }
        
        currentLevelIndex = levelIndex;
        LevelData selectedLevel = levels[levelIndex];
        
        Debug.Log($"Selected level: {selectedLevel.levelName}");
        
        SetPreviewVisible(true);
        
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            
            if (selectedLevel.previewVideo != null)
            {
                videoPlayer.clip = selectedLevel.previewVideo;
                videoPlayer.Play();
            }
        }
        
        if (audioSource != null)
        {
            audioSource.Stop();
            
            if (selectedLevel.previewAudio != null)
            {
                audioSource.clip = selectedLevel.previewAudio;
                audioSource.Play();
            }
        }
        
        if (playButtonText != null)
        {
            playButtonText.text = $"Играть: {selectedLevel.levelName}";
        }
        
        if (levelDescriptionText != null)
        {
            levelDescriptionText.text = selectedLevel.levelDescription;
        }
        
        UpdateLevelItemsVisual();
    }
    
    // НОВЫЙ МЕТОД: Снятие выбора с уровня
    public void DeselectLevel()
    {
        Debug.Log("Deselecting level");
        
        currentLevelIndex = -1;
        
        if (videoPlayer != null)
            videoPlayer.Stop();
            
        if (audioSource != null)
            audioSource.Stop();
            
        SetPreviewVisible(false);
        UpdateLevelItemsVisual();
    }
    
    void SetPreviewVisible(bool visible)
    {
        if (videoDisplay != null)
            videoDisplay.gameObject.SetActive(visible);
        if (playLevelButton != null)
            playLevelButton.gameObject.SetActive(visible);
        if (levelDescriptionText != null)
            levelDescriptionText.gameObject.SetActive(visible);
    }
    
    void UpdateLevelItemsVisual()
    {
        if (levelsContent == null) return;
        
        for (int i = 0; i < levelsContent.childCount; i++)
        {
            Transform child = levelsContent.GetChild(i);
            LevelItem item = child.GetComponent<LevelItem>();
            
            if (item != null)
            {
                item.SetSelected(i == currentLevelIndex);
            }
        }
    }
    
    void OnPlayLevelClicked()
    {
        if (currentLevelIndex < 0 || currentLevelIndex >= levels.Count) 
        {
            Debug.LogError("No level selected or invalid index!");
            return;
        }
        
        string sceneName = levels[currentLevelIndex].sceneName;
        if (!string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(LoadLevelScene(sceneName));
        }
    }

    IEnumerator LoadLevelScene(string sceneName)
    {
        if (audioSource != null)
            audioSource.Stop();
            
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = true;
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    
    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}

[System.Serializable]
public class LevelData
{
    public string levelName;
    public VideoClip previewVideo;
    public AudioClip previewAudio;
    public string sceneName;
    public string levelDescription;
}