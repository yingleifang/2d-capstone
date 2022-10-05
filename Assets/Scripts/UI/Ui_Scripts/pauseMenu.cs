using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pauseMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool gameIsPaused = false;

   // public GameObject pauseUI;
    public GameObject pausePanel;
    public GameObject menuButton;
    public GameObject settingsButton;
    public GameObject quitButton;


    void Start()
    {
        LeanTween.scale(pausePanel, new Vector3(0, 0, 0), 0.3f);

        LeanTween.scale(menuButton, new Vector3(1, 1, 1), 0.3f).setDelay(0.2f);
        LeanTween.scale(settingsButton, new Vector3(1, 1, 1), 0.3f).setDelay(0.4f);
        LeanTween.scale(quitButton, new Vector3(1, 1, 1), 0.3f).setDelay(0.6f);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
  
    }
    public void ButtonSelect(GameObject button)
    {
        LeanTween.scale(button, new Vector3(1.2f, 1.2f, 1.2f), 0.1f);
    }

    public void ButtonDeselect(GameObject button)
    {
        LeanTween.scale(button, new Vector3(1, 1, 1), 0.1f);
    }
    public void Resume()
    {
        pausePanel.SetActive(false);
        LeanTween.scale(pausePanel, new Vector3(0, 0, 0), 0.3f);
        //Time.timeScale = 1f;
        gameIsPaused = false;
        //gameIsPaused = false;
    }

     public void Pause()
    {
        pausePanel.SetActive(true);
        LeanTween.scale(pausePanel, new Vector3(1, 1, 1), 0.3f);
        //Time.timeScale = 0f;
        gameIsPaused = true;
    }
}
