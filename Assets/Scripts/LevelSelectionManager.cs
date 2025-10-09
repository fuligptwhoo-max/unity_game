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
    
    [Header("Levels Slider")]
    public ScrollRect levelsScrollRect;
    public Transform levelsContent;
    public GameObject levelItemPrefab;
    
    [Header("Level Data")]
    public List<LevelData> levels = new List<LevelData>();
    
    [Header("Audio")]
    public AudioSource audioSource;
    
    private int currentLevelIndex = -1;
    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    
    void Start()
    {
        InitializeComponents();
        CreateLevelItems();
        SetPreviewVisible(false);
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
        
        // УБРАЛИ автоматический выбор первого уровня
        // Уровни теперь не выбраны по умолчанию
    }
    
    public void SelectLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count) 
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }
        
        currentLevelIndex = levelIndex;
        LevelData selectedLevel = levels[levelIndex];
        
        Debug.Log($"Selected level: {selectedLevel.levelName}");
        
        SetPreviewVisible(true);
        
        // Останавливаем предыдущее видео
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            
            if (selectedLevel.previewVideo != null)
            {
                videoPlayer.clip = selectedLevel.previewVideo;
                videoPlayer.Play();
            }
        }
        
        // Останавливаем предыдущий звук и запускаем новый
        if (audioSource != null)
        {
            audioSource.Stop();
            
            if (selectedLevel.previewAudio != null)
            {
                audioSource.clip = selectedLevel.previewAudio;
                audioSource.Play();
                Debug.Log($"Playing preview audio: {selectedLevel.previewAudio.name}");
            }
            else
            {
                Debug.LogWarning($"No preview audio for level: {selectedLevel.levelName}");
            }
        }
        else
        {
            Debug.LogError("AudioSource is not assigned!");
        }
        
        if (playButtonText != null)
        {
            playButtonText.text = $"Пройти {selectedLevel.levelName}";
        }
        
        UpdateLevelItemsVisual();
    }
    
    void SetPreviewVisible(bool visible)
    {
        if (videoDisplay != null)
            videoDisplay.gameObject.SetActive(visible);
        if (playLevelButton != null)
            playLevelButton.gameObject.SetActive(visible);
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
            Debug.Log($"Loading scene: {sceneName}");
            StartCoroutine(LoadLevelScene(sceneName));
        }
        else
        {
            Debug.LogError("Scene name is empty!");
        }
    }

    IEnumerator LoadLevelScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        
        // Останавливаем звук превью при переходе на уровень
        if (audioSource != null)
            audioSource.Stop();
            
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = true;
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        Debug.Log("Scene loaded successfully");
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
}