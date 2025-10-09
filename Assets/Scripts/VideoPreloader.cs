using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;

public class VideoPreloader : MonoBehaviour
{
    private static VideoPreloader _instance;
    public static VideoPreloader Instance => _instance;
    
    private Dictionary<VideoClip, VideoPlayer> preloadedVideos = new Dictionary<VideoClip, VideoPlayer>();
    
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public IEnumerator PreloadVideo(VideoClip videoClip)
    {
        if (videoClip == null || preloadedVideos.ContainsKey(videoClip))
            yield break;
            
        GameObject loaderObject = new GameObject($"VideoPreloader_{videoClip.name}");
        VideoPlayer videoPlayer = loaderObject.AddComponent<VideoPlayer>();
        
        videoPlayer.clip = videoClip;
        videoPlayer.playOnAwake = false;
        
        videoPlayer.Prepare();
        
        yield return new WaitUntil(() => videoPlayer.isPrepared);
        
        preloadedVideos[videoClip] = videoPlayer;
        
        Debug.Log($"Video preloaded and ready: {videoClip.name}");
    }
    
    public VideoPlayer GetPreloadedVideo(VideoClip videoClip)
    {
        if (preloadedVideos.ContainsKey(videoClip))
        {
            return preloadedVideos[videoClip];
        }
        return null;
    }
    
    public void ReleaseVideo(VideoClip videoClip)
    {
        if (preloadedVideos.ContainsKey(videoClip))
        {
            Destroy(preloadedVideos[videoClip].gameObject);
            preloadedVideos.Remove(videoClip);
        }
    }
}