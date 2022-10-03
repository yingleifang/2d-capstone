using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class levelTransition : MonoBehaviour
{
    public Animator blackfade;
    public Animator blackedge;
    public Animator rock1;
    public Animator rock2;
    public Animator rock3;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void LoadNextLevel()
    {

        StartCoroutine(LoadLevel());
    }

    IEnumerator LoadLevel()
    {
        blackfade.SetTrigger("start");
        blackedge.SetTrigger("start");
        rock1.SetTrigger("start2");
        rock2.SetTrigger("start2");
        rock3.SetTrigger("start3");
        yield return new WaitForSeconds(1f);

       //SceneManager.LoadScene(levelIndex);
    }
}
