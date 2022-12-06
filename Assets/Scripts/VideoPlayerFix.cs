using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;

public class VideoPlayerFix : MonoBehaviour
{
    public VideoClip clip;
    public VideoPlayer player;

    private void Awake()
    {
        if (player == null)
        {
            player = GetComponent<VideoPlayer>();
        }
        if (player && clip == null)
        {
            clip = player.clip;
        }
        if (clip && player)
        {
            PlayClip(clip);
        }
    }

    /// <summary>
    /// Sets up the video playback for VideoPlayer so that it works on a web build.
    /// Video must also be present in StreamingAssets folder with same name
    /// </summary>
    /// <param name="clip">the clip to play</param>
    public void PlayClip(VideoClip clip)
    {
        if (clip)
        {
            // Need a path to the file so it plays on web builds
            string file = System.IO.Path.GetFileName(clip.originalPath);
            string url = System.IO.Path.Combine(Application.streamingAssetsPath, file);
            if (Application.platform == RuntimePlatform.Android)
            {
                StartCoroutine(GetRequest(url));
            } else
            {
                player.url = url;
                player.Play();
                player.playOnAwake = true;
            }
        }
    }

    IEnumerator GetRequest(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        player.url = www.url;
        player.Play();
        player.playOnAwake = true;
    }
}
