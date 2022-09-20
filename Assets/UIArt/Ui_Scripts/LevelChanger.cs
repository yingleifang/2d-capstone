using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
   // public Animator anim;
   // private int levelToLoad;
    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("LevelOne");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
   // public void FadeToLevel (int levelIndex)
    //{
     //   anim.SetTrigger("fadeOut");
   // }

    //public void OnFadeComplete()
   // {
    //    SceneManager.LoadScene(levelToLoad);
    //}
}
