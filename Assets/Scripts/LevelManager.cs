using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [Header("Video Components")]
    public VideoPlayer primaryVideoPlayer;
    public VideoPlayer secondaryVideoPlayer;
    public RawImage videoDisplay;
    
    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public Button continueButton;
    
    [Header("Choice UI")]
    public GameObject choicePanel;
    public Button[] choiceButtons;
    
    [Header("Pause UI")]
    public GameObject pausePanel;
    public Button[] pauseButtons;
    public TMP_Text pauseDialogueText;
    
    [Header("Level Content")]
    public List<SegmentData> storySegments;
    
    [Header("Loading Reference")]
    public LoadingScreenManager loadingScreen;
    
    [Header("Audio for Settings")]
    public AudioSource levelBackgroundAudio;
    
    // Публичные свойства для доступа из SettingsManager
    public VideoPlayer PrimaryVideoPlayer => primaryVideoPlayer;
    public VideoPlayer SecondaryVideoPlayer => secondaryVideoPlayer;
    
    private int currentSegmentIndex = 0;
    private RenderTexture primaryRenderTexture;
    private RenderTexture secondaryRenderTexture;
    private Coroutine currentSegmentCoroutine;
    private Dictionary<string, bool> gameFlags = new Dictionary<string, bool>();
    private bool isPaused = false;
    private bool usingPrimaryVideoPlayer = true;
    
    // Ключи для сохранения
    private const string CURRENT_SEGMENT_KEY = "CurrentSegment";
    private const string GAME_FLAGS_KEY = "GameFlags";
    
    [System.Serializable]
    public class SegmentData
    {
        [Header("Basic Settings")]
        public string segmentName;
        public VideoClip video;
        public AudioClip audio;
        public bool autoContinue = true;
        public bool isLooping = false;
        
        [Header("Dialogue Settings")]
        public bool showDialogue = false;
        public string dialogueText = "";
        public string continueButtonText = "Продолжить";
        
        [Header("Choice Settings")]
        public bool showChoices = false;
        public List<ChoiceOption> choices = new List<ChoiceOption>();
        
        [Header("Pause Settings")]
        public bool hasPause = false;
        public float pauseTime = 0f;
        public List<PauseButton> pauseButtons = new List<PauseButton>();
        public string pauseDialogueText = "";
        
        [Header("Next Segment")]
        public int nextSegment = -1;
        public string requiredFlag = "";
    }
    
    [System.Serializable]
    public class ChoiceOption
    {
        public string choiceText;
        public int targetSegment;
        public string setFlag = "";
    }
    
    [System.Serializable]
    public class PauseButton
    {
        public string buttonText;
        public string setFlag = "";
    }

    void Start()
    {
        Debug.Log("LevelManager: Initializing with dual VideoPlayer system");
        
        // ЗАГРУЖАЕМ СОХРАНЕНИЕ СРАЗУ ПРИ СТАРТЕ
        LoadProgress();
        
        InitializeLevel();

        List<VideoClip> videosToPreload = new List<VideoClip>();
        foreach (var segment in storySegments)
        {
            if (segment.video != null && !videosToPreload.Contains(segment.video))
            {
                videosToPreload.Add(segment.video);
                Debug.Log($"Added video to preload: {segment.video.name}");
            }
        }

        Debug.Log($"Total videos to preload: {videosToPreload.Count}");

        if (loadingScreen != null)
        {
            Debug.Log("Calling LoadingScreenManager.ShowLoadingScreen");
            loadingScreen.ShowLoadingScreen(videosToPreload);
        }
        else
        {
            Debug.LogError("LoadingScreenManager reference is NULL! Starting level immediately.");
            StartLevelAfterLoading();
        }

        ApplyVolumeSettings();
    }
    
    // УЛУЧШЕННЫЙ МЕТОД: Загрузка прогресса
    private void LoadProgress()
    {
        if (PlayerPrefs.HasKey(CURRENT_SEGMENT_KEY))
        {
            currentSegmentIndex = PlayerPrefs.GetInt(CURRENT_SEGMENT_KEY, 0);
            Debug.Log($"Loaded progress: segment {currentSegmentIndex}");
        }
        else
        {
            currentSegmentIndex = 0;
            Debug.Log("No saved progress found, starting from segment 0");
        }
        
        // Загружаем флаги игры
        if (PlayerPrefs.HasKey(GAME_FLAGS_KEY))
        {
            string flagsJson = PlayerPrefs.GetString(GAME_FLAGS_KEY);
            SerializableDictionary loadedFlags = JsonUtility.FromJson<SerializableDictionary>(flagsJson);
            if (loadedFlags != null)
            {
                gameFlags = loadedFlags.ToDictionary();
                Debug.Log($"Loaded {gameFlags.Count} game flags");
            }
        }
        else
        {
            // Инициализируем базовые флаги если нет сохранений
            gameFlags["hasApproachedDoor"] = false;
            Debug.Log("Initialized default game flags");
        }
    }
    
    // УЛУЧШЕННЫЙ МЕТОД: Сохранение прогресса
    public void SaveProgress()
    {
        PlayerPrefs.SetInt(CURRENT_SEGMENT_KEY, currentSegmentIndex);
        
        // Сохраняем флаги игры
        SerializableDictionary flagsToSave = new SerializableDictionary(gameFlags);
        string flagsJson = JsonUtility.ToJson(flagsToSave);
        PlayerPrefs.SetString(GAME_FLAGS_KEY, flagsJson);
        
        PlayerPrefs.Save();
        Debug.Log($"Progress saved: segment {currentSegmentIndex}, flags: {gameFlags.Count}");
    }
    
    // НОВЫЙ МЕТОД: Сброс прогресса (для тестирования)
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(CURRENT_SEGMENT_KEY);
        PlayerPrefs.DeleteKey(GAME_FLAGS_KEY);
        PlayerPrefs.Save();
        currentSegmentIndex = 0;
        gameFlags.Clear();
        gameFlags["hasApproachedDoor"] = false;
        Debug.Log("Progress reset to beginning");
    }
    
    // КЛАСС: Для сериализации Dictionary
    [System.Serializable]
    public class SerializableDictionary
    {
        [System.Serializable]
        public class KeyValuePair
        {
            public string key;
            public bool value;
        }
        
        public List<KeyValuePair> items = new List<KeyValuePair>();
        
        public SerializableDictionary() { }
        
        public SerializableDictionary(Dictionary<string, bool> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                items.Add(new KeyValuePair { key = kvp.Key, value = kvp.Value });
            }
        }
        
        public Dictionary<string, bool> ToDictionary()
        {
            Dictionary<string, bool> dict = new Dictionary<string, bool>();
            foreach (var item in items)
            {
                dict[item.key] = item.value;
            }
            return dict;
        }
    }
    
    private void ApplyVolumeSettings()
    {
        // Применяем громкость вручную
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float finalVolume = sfxVol * masterVol;
        
        SetVideoPlayerVolume(primaryVideoPlayer, finalVolume);
        SetVideoPlayerVolume(secondaryVideoPlayer, finalVolume);
        
        Debug.Log("Applied volume settings manually: " + finalVolume);
    }

    // Вспомогательный метод для установки громкости VideoPlayer
    private void SetVideoPlayerVolume(VideoPlayer videoPlayer, float volume)
    {
        if (videoPlayer != null)
        {
            for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
            {
                videoPlayer.SetDirectAudioVolume(i, volume);
            }
        }
    }
    
    void Update()
    {
        VideoPlayer activeVideoPlayer = usingPrimaryVideoPlayer ? primaryVideoPlayer : secondaryVideoPlayer;
        
        if (activeVideoPlayer.isPlaying && Time.frameCount % 300 == 0)
        {
            Debug.Log($"Video playing: {activeVideoPlayer.time:F2}/{activeVideoPlayer.length:F2}, Segment: {currentSegmentIndex}");
        }
        
        // НОВАЯ ФУНКЦИЯ: Пропуск текущего сегмента по нажатию Space или правой кнопки мыши
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1)) && activeVideoPlayer.isPlaying)
        {
            SkipCurrentSegment();
        }
        
        // Тестирование: Сброс прогресса по F5
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ResetProgress();
            Debug.Log("Progress reset (F5 pressed)");
        }
        
        // Тестирование: Сохранение прогресса по F6
        if (Input.GetKeyDown(KeyCode.F6))
        {
            SaveProgress();
            Debug.Log("Progress manually saved (F6 pressed)");
        }
        
        // УБРАНА обработка ESC - теперь это делает только SettingsManager
        // Оставляем только F1 для открытия настроек в уровне
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleSettingsInLevel();
        }
    }
    
    // НОВЫЙ МЕТОД: Пропуск текущего сегмента
    private void SkipCurrentSegment()
    {
        if (currentSegmentCoroutine != null)
        {
            StopCoroutine(currentSegmentCoroutine);
            currentSegmentCoroutine = null;
        }
        
        VideoPlayer activeVideoPlayer = usingPrimaryVideoPlayer ? primaryVideoPlayer : secondaryVideoPlayer;
        activeVideoPlayer.Stop();
        
        // Переходим к следующему сегменту
        SegmentData currentSegment = storySegments[currentSegmentIndex];
        int nextSegment = GetNextSegmentIndex(currentSegment);
        
        if (nextSegment != -1)
        {
            Debug.Log($"Skipping to segment: {nextSegment}");
            SwitchVideoPlayer();
            PlaySegmentImmediately(nextSegment);
        }
        else
        {
            Debug.Log("No next segment available after skip");
        }
    }
    
    // Метод для переключения настроек в уровне (только по F1)
    void ToggleSettingsInLevel()
    {
        SettingsManager settings = FindAnyObjectByType<SettingsManager>();
        if (settings != null)
        {
            settings.ToggleSettings();
        }
        else
        {
            Debug.LogWarning("SettingsManager not found in Level1Scene!");
        }
    }
    
    public void StartLevelAfterLoading()
    {
        Debug.Log($"Starting level after loading from segment: {currentSegmentIndex}");
        
        // ПРОВЕРЯЕМ КОРРЕКТНОСТЬ СОХРАНЕННОГО СЕГМЕНТА
        if (currentSegmentIndex < 0 || currentSegmentIndex >= storySegments.Count)
        {
            Debug.LogWarning($"Invalid saved segment index: {currentSegmentIndex}, resetting to 0");
            currentSegmentIndex = 0;
        }
        
        if (storySegments.Count > 0 && storySegments[currentSegmentIndex].video != null)
        {
            StartCoroutine(PrepareFirstVideo());
        }
        else
        {
            PlaySegmentImmediately(currentSegmentIndex);
        }
    }
    
    IEnumerator PrepareFirstVideo()
    {
        Debug.Log("Preparing first video for immediate playback");
        
        VideoPlayer activeVideoPlayer = usingPrimaryVideoPlayer ? primaryVideoPlayer : secondaryVideoPlayer;
        activeVideoPlayer.clip = storySegments[currentSegmentIndex].video;
        activeVideoPlayer.isLooping = storySegments[currentSegmentIndex].isLooping;
        
        activeVideoPlayer.Prepare();
        yield return new WaitUntil(() => activeVideoPlayer.isPrepared);
        
        Debug.Log("First video prepared, starting playback");
        PlaySegmentImmediately(currentSegmentIndex);
    }
    
    void InitializeLevel()
    {
        Debug.Log("Initializing Level with dual VideoPlayer system");
        
        primaryRenderTexture = new RenderTexture(1920, 1080, 24);
        secondaryRenderTexture = new RenderTexture(1920, 1080, 24);
        
        if (primaryVideoPlayer != null)
        {
            primaryVideoPlayer.targetTexture = primaryRenderTexture;
            primaryVideoPlayer.playOnAwake = false;
            primaryVideoPlayer.loopPointReached += OnVideoFinished;
            primaryVideoPlayer.skipOnDrop = true;
            primaryVideoPlayer.waitForFirstFrame = false;
            
            // Включаем аудио дорожки
            primaryVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            Debug.Log("Primary VideoPlayer initialized");
        }
        
        if (secondaryVideoPlayer != null)
        {
            secondaryVideoPlayer.targetTexture = secondaryRenderTexture;
            secondaryVideoPlayer.playOnAwake = false;
            secondaryVideoPlayer.skipOnDrop = true;
            secondaryVideoPlayer.waitForFirstFrame = false;
            
            // Включаем аудио дорожки
            secondaryVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            Debug.Log("Secondary VideoPlayer initialized");
        }
        
        if (videoDisplay != null)
        {
            videoDisplay.texture = primaryRenderTexture;
            usingPrimaryVideoPlayer = true;
            Debug.Log("VideoDisplay initialized with primary texture");
        }
        
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
        
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;
            if (choiceButtons[i] != null)
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
        }
        
        for (int i = 0; i < pauseButtons.Length; i++)
        {
            int index = i;
            if (pauseButtons[i] != null)
                pauseButtons[i].onClick.AddListener(() => OnPauseButtonClicked(index));
        }
    }
    
    void PlaySegmentImmediately(int segmentIndex)
    {
        Debug.Log($"PlaySegmentImmediately: {segmentIndex}");
        
        if (currentSegmentCoroutine != null)
        {
            StopCoroutine(currentSegmentCoroutine);
        }
        
        currentSegmentCoroutine = StartCoroutine(PlaySegmentCoroutine(segmentIndex));
    }
    
    IEnumerator PlaySegmentCoroutine(int segmentIndex)
    {
        if (segmentIndex < 0 || segmentIndex >= storySegments.Count)
        {
            Debug.LogError($"Invalid segment index: {segmentIndex}");
            yield break;
        }
        
        SegmentData segment = storySegments[segmentIndex];
        currentSegmentIndex = segmentIndex;
        
        Debug.Log($"=== STARTING SEGMENT: {segment.segmentName} (Index: {segmentIndex}) ===");
        
        HideAllUI();
        
        if (segment.video == null)
        {
            Debug.LogError($"No video assigned for segment: {segment.segmentName}");
            yield break;
        }
        
        Debug.Log($"Setting video: {segment.video.name}");
        
        VideoPlayer activeVideoPlayer = usingPrimaryVideoPlayer ? primaryVideoPlayer : secondaryVideoPlayer;
        VideoPlayer standbyVideoPlayer = usingPrimaryVideoPlayer ? secondaryVideoPlayer : primaryVideoPlayer;
        
        int nextSegmentIndex = GetNextSegmentIndex(segment);
        if (nextSegmentIndex != -1 && storySegments[nextSegmentIndex].video != null)
        {
            StartCoroutine(PrepareNextVideoInBackground(storySegments[nextSegmentIndex].video, standbyVideoPlayer));
        }
        
        if (activeVideoPlayer.clip == segment.video && activeVideoPlayer.isPrepared)
        {
            Debug.Log("Video already prepared, starting immediately");
            activeVideoPlayer.Play();
        }
        else
        {
            activeVideoPlayer.Stop();
            activeVideoPlayer.clip = segment.video;
            activeVideoPlayer.isLooping = segment.isLooping;
            
            activeVideoPlayer.Prepare();
            
            float waitStartTime = Time.time;
            while (!activeVideoPlayer.isPrepared && Time.time - waitStartTime < 0.5f)
            {
                yield return null;
            }
            
            if (activeVideoPlayer.isPrepared)
            {
                Debug.Log("Video prepared quickly, starting playback");
                activeVideoPlayer.Play();
            }
            else
            {
                Debug.Log("Starting playback without full preparation");
                activeVideoPlayer.Play();
            }
        }
        
        if (segment.hasPause)
        {
            Debug.Log($"Segment has pause at {segment.pauseTime}s");
            yield return StartCoroutine(HandlePause(segment));
        }
        
        if (segment.autoContinue && !segment.isLooping)
        {
            Debug.Log("Waiting for video to finish (autoContinue)");
            yield return StartCoroutine(WaitForVideoEnd());
            
            int nextSegment = GetNextSegmentIndex(segment);
            if (nextSegment != -1)
            {
                Debug.Log($"Auto-advancing to segment: {nextSegment}");
                SwitchVideoPlayer();
                PlaySegmentImmediately(nextSegment);
                yield break;
            }
        }
        
        if (segment.showDialogue)
        {
            Debug.Log("Showing dialogue");
            yield return StartCoroutine(ShowDialogue(segment.dialogueText, segment.continueButtonText));
        }
        
        if (segment.showChoices)
        {
            Debug.Log("Showing choices");
            yield return StartCoroutine(ShowChoices(segment.choices));
        }
        else if (!segment.autoContinue)
        {
            Debug.Log("Waiting for click to continue");
            yield return StartCoroutine(WaitForClick());
            
            int nextSegment = GetNextSegmentIndex(segment);
            if (nextSegment != -1)
            {
                Debug.Log($"Advancing to segment: {nextSegment}");
                SwitchVideoPlayer();
                PlaySegmentImmediately(nextSegment);
            }
        }
        
        // СОХРАНЕНИЕ ПРОГРЕССА после каждого сегмента
        SaveProgress();
    }
    
    IEnumerator PrepareNextVideoInBackground(VideoClip nextVideo, VideoPlayer targetPlayer)
    {
        if (targetPlayer.clip == nextVideo && targetPlayer.isPrepared)
        {
            Debug.Log($"Next video already prepared in background: {nextVideo.name}");
            yield break;
        }
        
        Debug.Log($"Preparing next video in background: {nextVideo.name}");
        
        targetPlayer.Stop();
        targetPlayer.clip = nextVideo;
        targetPlayer.Prepare();
        
        yield return null;
    }
    
    void SwitchVideoPlayer()
    {
        usingPrimaryVideoPlayer = !usingPrimaryVideoPlayer;
        
        if (videoDisplay != null)
        {
            videoDisplay.texture = usingPrimaryVideoPlayer ? primaryRenderTexture : secondaryRenderTexture;
            Debug.Log($"Switched to {(usingPrimaryVideoPlayer ? "primary" : "secondary")} video player");
        }
    }
    
    IEnumerator HandlePause(SegmentData segment)
    {
        if (!segment.hasPause) yield break;
        
        Debug.Log($"Waiting for pause time: {segment.pauseTime}s");
        
        VideoPlayer activeVideoPlayer = usingPrimaryVideoPlayer ? primaryVideoPlayer : secondaryVideoPlayer;
        
        float timer = 0f;
        while (timer < segment.pauseTime && activeVideoPlayer.isPlaying)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (activeVideoPlayer.isPlaying)
        {
            Debug.Log("Pausing video for pause event");
            activeVideoPlayer.Pause();
            isPaused = true;
            
            yield return StartCoroutine(ShowPauseButtons(segment.pauseButtons, segment.pauseDialogueText));
            
            activeVideoPlayer.Play();
            isPaused = false;
            Debug.Log("Resuming video after pause");
        }
    }
    
    IEnumerator WaitForVideoEnd()
    {
        Debug.Log("WaitForVideoEnd started");
        
        VideoPlayer activeVideoPlayer = usingPrimaryVideoPlayer ? primaryVideoPlayer : secondaryVideoPlayer;
        
        if (activeVideoPlayer.clip == null) yield break;
        
        bool videoFinished = false;
        
        VideoPlayer.EventHandler finishHandler = (vp) => {
            videoFinished = true;
            Debug.Log("Video finished event received");
        };
        
        activeVideoPlayer.loopPointReached += finishHandler;
        
        while (!videoFinished && activeVideoPlayer.isPlaying)
        {
            yield return null;
        }
        
        activeVideoPlayer.loopPointReached -= finishHandler;
        Debug.Log("WaitForVideoEnd completed");
    }
    
    IEnumerator WaitForClick()
    {
        Debug.Log("WaitForClick started");
        
        bool clicked = false;
        UnityAction clickAction = () => {
            clicked = true;
            Debug.Log("Continue button clicked");
        };
        
        continueButton.onClick.AddListener(clickAction);
        continueButton.gameObject.SetActive(true);
        
        yield return new WaitUntil(() => clicked);
        
        continueButton.onClick.RemoveListener(clickAction);
        continueButton.gameObject.SetActive(false);
        
        Debug.Log("WaitForClick completed");
    }
    
    IEnumerator ShowPauseButtons(List<PauseButton> buttons, string dialogueText)
    {
        Debug.Log($"ShowPauseButtons: {buttons.Count} buttons");
        
        if (pausePanel == null) yield break;
        
        foreach (Button button in pauseButtons)
        {
            if (button != null) button.gameObject.SetActive(false);
        }
        
        for (int i = 0; i < buttons.Count && i < pauseButtons.Length; i++)
        {
            if (pauseButtons[i] != null)
            {
                pauseButtons[i].gameObject.SetActive(true);
                TMP_Text buttonText = pauseButtons[i].GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = buttons[i].buttonText;
            }
        }
        
        if (pauseDialogueText != null && !string.IsNullOrEmpty(dialogueText))
        {
            pauseDialogueText.text = dialogueText;
        }
        
        pausePanel.SetActive(true);
        
        bool buttonClicked = false;
        int selectedButton = -1;
        
        UnityAction<int> buttonAction = (buttonIndex) => {
            selectedButton = buttonIndex;
            buttonClicked = true;
        };
        
        for (int i = 0; i < buttons.Count && i < pauseButtons.Length; i++)
        {
            int index = i;
            if (pauseButtons[index] != null)
            {
                pauseButtons[index].onClick.RemoveAllListeners();
                pauseButtons[index].onClick.AddListener(() => buttonAction(index));
            }
        }
        
        yield return new WaitUntil(() => buttonClicked);
        
        if (selectedButton >= 0 && selectedButton < buttons.Count)
        {
            PauseButton clickedButton = buttons[selectedButton];
            if (!string.IsNullOrEmpty(clickedButton.setFlag))
            {
                gameFlags[clickedButton.setFlag] = true;
                SaveProgress(); // Сохраняем после изменения флагов
            }
        }
        
        pausePanel.SetActive(false);
        
        for (int i = 0; i < pauseButtons.Length; i++)
        {
            if (pauseButtons[i] != null)
                pauseButtons[i].onClick.RemoveAllListeners();
        }
    }
    
    void HideAllUI()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
    }
    
    IEnumerator ShowDialogue(string text, string buttonText)
    {
        if (dialoguePanel != null && dialogueText != null)
        {
            dialogueText.text = text;
            
            TMP_Text buttonTextComponent = continueButton.GetComponentInChildren<TMP_Text>();
            if (buttonTextComponent != null)
                buttonTextComponent.text = buttonText;
                
            dialoguePanel.SetActive(true);
            continueButton.gameObject.SetActive(true);
            
            bool continueClicked = false;
            UnityAction continueAction = () => continueClicked = true;
            continueButton.onClick.AddListener(continueAction);
            
            yield return new WaitUntil(() => continueClicked);
            
            continueButton.onClick.RemoveListener(continueAction);
            dialoguePanel.SetActive(false);
            continueButton.gameObject.SetActive(false);
        }
    }
    
    IEnumerator ShowChoices(List<ChoiceOption> choices)
    {
        if (choicePanel == null) yield break;
        
        Debug.Log($"Showing {choices.Count} choices");
        
        foreach (Button button in choiceButtons)
        {
            if (button != null) button.gameObject.SetActive(false);
        }
        
        for (int i = 0; i < choices.Count && i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
            {
                choiceButtons[i].gameObject.SetActive(true);
                TMP_Text buttonText = choiceButtons[i].GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = choices[i].choiceText;
            }
        }
        
        choicePanel.SetActive(true);
        
        bool choiceMade = false;
        int selectedChoice = -1;
        
        UnityAction<int> choiceAction = (choiceIndex) => {
            selectedChoice = choiceIndex;
            choiceMade = true;
            Debug.Log($"Choice selected: {choiceIndex} -> {choices[choiceIndex].targetSegment}");
        };
        
        for (int i = 0; i < choices.Count && i < choiceButtons.Length; i++)
        {
            int index = i;
            if (choiceButtons[index] != null)
                choiceButtons[index].onClick.AddListener(() => choiceAction(index));
        }
        
        yield return new WaitUntil(() => choiceMade);
        
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
                choiceButtons[i].onClick.RemoveAllListeners();
        }
        
        choicePanel.SetActive(false);
        
        if (selectedChoice >= 0 && selectedChoice < choices.Count)
        {
            ChoiceOption chosen = choices[selectedChoice];
            
            if (!string.IsNullOrEmpty(chosen.setFlag))
            {
                gameFlags[chosen.setFlag] = true;
                SaveProgress(); // Сохраняем после выбора
            }
            
            Debug.Log($"Transitioning to segment: {chosen.targetSegment}");
            SwitchVideoPlayer();
            PlaySegmentImmediately(chosen.targetSegment);
        }
    }
    
    int GetNextSegmentIndex(SegmentData segment)
    {
        if (!string.IsNullOrEmpty(segment.requiredFlag))
        {
            if (!gameFlags.ContainsKey(segment.requiredFlag) || !gameFlags[segment.requiredFlag])
            {
                return -1;
            }
        }
        
        if (segment.nextSegment != -1)
        {
            return segment.nextSegment;
        }
        else if (currentSegmentIndex + 1 < storySegments.Count)
        {
            return currentSegmentIndex + 1;
        }
        
        return -1;
    }
    
    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("OnVideoFinished called");
    }
    
    void OnContinueClicked()
    {
        Debug.Log("OnContinueClicked");
    }
    
    void OnChoiceSelected(int choiceIndex)
    {
        Debug.Log($"OnChoiceSelected: {choiceIndex}");
    }
    
    void OnPauseButtonClicked(int buttonIndex)
    {
        Debug.Log($"OnPauseButtonClicked: {buttonIndex}");
    }
    
    void OnDestroy()
    {
        // СОХРАНЯЕМ ПРОГРЕСС ПРИ ВЫХОДЕ ИЗ УРОВНЯ
        SaveProgress();
        
        if (primaryVideoPlayer != null)
            primaryVideoPlayer.loopPointReached -= OnVideoFinished;
            
        if (primaryRenderTexture != null)
        {
            primaryRenderTexture.Release();
            Destroy(primaryRenderTexture);
        }
        
        if (secondaryRenderTexture != null)
        {
            secondaryRenderTexture.Release();
            Destroy(secondaryRenderTexture);
        }
    }
}