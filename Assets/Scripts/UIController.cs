using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UIController : MonoBehaviour
{
    public GameObject blackSquare;
    public UnitSelectionWindow selectionWindow;

    public IEnumerator SwitchScene(int fadeSpeed = 2)
    {
        Color goColor = blackSquare.GetComponent<Image>().color;
        float newA = 0;

        while (blackSquare.GetComponent<Image>().color.a < 1)
        {
            newA = goColor.a + (fadeSpeed * Time.deltaTime);
            goColor = new Color (goColor.r, goColor.g, goColor.b, newA);
            blackSquare.GetComponent<Image>().color = goColor;
            yield return null;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public IEnumerator SwitchScene(string sceneName, int fadeSpeed = 2)
    {
        Color goColor = blackSquare.GetComponent<Image>().color;
        float newA = 0;

        while (blackSquare.GetComponent<Image>().color.a < 1)
        {
            newA = goColor.a + (fadeSpeed * Time.deltaTime);
            //Debug.Log(newA);
            goColor = new Color (goColor.r, goColor.g, goColor.b, newA);
            blackSquare.GetComponent<Image>().color = goColor;
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void StartButton()
    {
        StartCoroutine(SwitchScene());
    }

    public void PlayAgainButton()
    {
        StartCoroutine(SwitchScene("StartMenu"));
    }

    public IEnumerator ShowSelectionWindow()
    {
        Debug.Log("Showing window");
        return selectionWindow.Show();
    }

    public IEnumerator HideSelectionWindow()
    {
        return selectionWindow.Hide();
    }
}
