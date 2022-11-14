using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

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
            player.url = System.IO.Path.Combine(Application.streamingAssetsPath, file);
            player.Play();
            player.playOnAwake = true;
        }
    }
}
