using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    public string targetScene;
   // public Animator anim;
   // private int levelToLoad;
    // Update is called once per frame
    //void Update()
    //{
        
   // }

    public void MainMenu()
    {
        DestroyObjectWithTag("crossBattle");
        LevelManager.instance.RefreshNewGame();
        SceneManager.LoadScene("StartMenu");
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(targetScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DestroyObjectWithTag(string tag)
    {
        var gameObjects = GameObject.FindGameObjectsWithTag(tag);

        for (var i = 0; i < gameObjects.Length; i++)
            Destroy(gameObjects[i]);
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
