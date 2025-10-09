using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("Loading Screen UI")]
    public GameObject loadingScreen;
    public Slider progressBar;
    public TMP_Text progressText;
    public TMP_Text tipText;
    
    [Header("Tips")]
    public string[] loadingTips;
    
    [Header("References")]
    public LevelManager levelManager;
    
    private List<VideoClip> videosToPreload = new List<VideoClip>();
    
    void Start()
    {
        Debug.Log("LoadingScreenManager: Start");
        
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
    }
    
    public void ShowLoadingScreen(List<VideoClip> videos)
    {
        Debug.Log($"LoadingScreenManager: ShowLoadingScreen called with {videos.Count} videos");
        
        videosToPreload = videos;
        StartCoroutine(LoadingRoutine());
    }
    
    IEnumerator LoadingRoutine()
    {
        Debug.Log("LoadingRoutine started");
        
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }
        
        if (tipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            string randomTip = loadingTips[Random.Range(0, loadingTips.Length)];
            tipText.text = "Совет: " + randomTip;
        }
        
        if (progressBar != null)
            progressBar.value = 0f;
        
        if (progressText != null)
            progressText.text = "0%";
        
        Debug.Log($"Starting to preload {videosToPreload.Count} videos");
        
        List<VideoPlayer> tempVideoPlayers = new List<VideoPlayer>();
        
        for (int i = 0; i < videosToPreload.Count; i++)
        {
            if (videosToPreload[i] == null) continue;
            
            Debug.Log($"Preloading video {i+1}/{videosToPreload.Count}: {videosToPreload[i].name}");
            
            GameObject tempLoader = new GameObject($"VideoLoader_{i}");
            VideoPlayer tempVP = tempLoader.AddComponent<VideoPlayer>();
            tempVideoPlayers.Add(tempVP);
            
            tempVP.clip = videosToPreload[i];
            tempVP.playOnAwake = false;
            tempVP.skipOnDrop = true;
            tempVP.waitForFirstFrame = false;
            
            tempVP.Prepare();
            
            float progress = (float)(i + 1) / videosToPreload.Count;
            if (progressBar != null)
                progressBar.value = progress;
            
            if (progressText != null)
                progressText.text = $"{(int)(progress * 100)}%";
            
            yield return null;
        }
        
        int loadedCount = 0;
        while (loadedCount < tempVideoPlayers.Count)
        {
            loadedCount = 0;
            foreach (var vp in tempVideoPlayers)
            {
                if (vp.isPrepared)
                {
                    loadedCount++;
                }
            }
            
            float currentProgress = (float)loadedCount / tempVideoPlayers.Count;
            if (progressBar != null)
                progressBar.value = currentProgress;
            
            if (progressText != null)
                progressText.text = $"{(int)(currentProgress * 100)}%";
                
            yield return null;
        }
        
        foreach (var vp in tempVideoPlayers)
        {
            if (vp != null && vp.gameObject != null)
                Destroy(vp.gameObject);
        }
        
        Debug.Log("All videos preloaded successfully!");
        
        if (progressBar != null)
            progressBar.value = 1f;
        
        if (progressText != null)
            progressText.text = "100%";
        
        yield return new WaitForSeconds(0.3f);
        
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
        
        if (levelManager != null)
        {
            levelManager.StartLevelAfterLoading();
        }
        else
        {
            Debug.LogError("LevelManager reference is null!");
        }
    }
}