using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class nextAfterVideo : MonoBehaviour
{
    public VideoPlayer VideoPlayer; 
    public string targetScene;

    // Start is called before the first frame update
    void Start()
    {
        VideoPlayer.loopPointReached += LoadScene;
    }
    void LoadScene(VideoPlayer vidObj)
    {
        SceneManager.LoadScene(targetScene);
    }
}
