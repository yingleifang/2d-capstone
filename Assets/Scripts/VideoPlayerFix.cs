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
            string file = System.IO.Path.GetFileName(clip.originalPath);
            Debug.Log("Clip name: " + file);
            player.url = System.IO.Path.Combine(Application.streamingAssetsPath, file);
        }
    }
}
