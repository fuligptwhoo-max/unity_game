using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;

public class VideoPreloadOptimizer : MonoBehaviour
{
    public static VideoPreloadOptimizer Instance { get; private set; }
    
    private Dictionary<VideoClip, VideoPlayer> preloadedVideos = new Dictionary<VideoClip, VideoPlayer>();
    private List<VideoPlayer> availableVideoPlayers = new List<VideoPlayer>();
    
    [Header("Settings")]
    public int maxPreloadedVideos = 3;
    public bool enableBackgroundPreloading = true;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeVideoPlayers();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeVideoPlayers()
    {
        // Создаем пул VideoPlayer'ов
        for (int i = 0; i < maxPreloadedVideos; i++)
        {
            GameObject vpObject = new GameObject($"BackgroundVideoPlayer_{i}");
            vpObject.transform.SetParent(transform);
            VideoPlayer vp = vpObject.AddComponent<VideoPlayer>();
            
            vp.playOnAwake = false;
            vp.skipOnDrop = true;
            vp.waitForFirstFrame = false;
            
            availableVideoPlayers.Add(vp);
        }
        
        Debug.Log($"Initialized {availableVideoPlayers.Count} background video players");
    }
    
    public void PreloadVideo(VideoClip clip)
    {
        if (clip == null || preloadedVideos.ContainsKey(clip)) return;
        
        if (availableVideoPlayers.Count > 0)
        {
            VideoPlayer vp = availableVideoPlayers[0];
            availableVideoPlayers.RemoveAt(0);
            
            StartCoroutine(PreloadVideoCoroutine(clip, vp));
        }
    }
    
    private IEnumerator PreloadVideoCoroutine(VideoClip clip, VideoPlayer vp)
    {
        vp.clip = clip;
        vp.Prepare();
        
        yield return new WaitUntil(() => vp.isPrepared);
        
        preloadedVideos[clip] = vp;
        Debug.Log($"Successfully preloaded video: {clip.name}");
    }
    
    public VideoPlayer GetPreloadedVideo(VideoClip clip)
    {
        if (preloadedVideos.ContainsKey(clip))
        {
            VideoPlayer vp = preloadedVideos[clip];
            preloadedVideos.Remove(clip);
            availableVideoPlayers.Add(vp);
            return vp;
        }
        return null;
    }
    
    public bool IsVideoPreloaded(VideoClip clip)
    {
        return preloadedVideos.ContainsKey(clip);
    }
    
    public void ClearPreloadedVideos()
    {
        foreach (var kvp in preloadedVideos)
        {
            if (kvp.Value != null)
            {
                kvp.Value.Stop();
                availableVideoPlayers.Add(kvp.Value);
            }
        }
        preloadedVideos.Clear();
    }
}