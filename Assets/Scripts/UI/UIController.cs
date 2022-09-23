using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UIController : MonoBehaviour
{
    public GameObject blackSquare;
    public UnitSelectionWindow unitSelectionWindow;
    public GameObject unitSelectionTutorial;
    public UnitInfoWindow unitInfoWindow;
    public Button endTurnButton;
    public bool isTutorial;


    public IEnumerator DisableEndTurnButton()
    {
        endTurnButton.interactable = false;
        yield break;
    }

    public IEnumerator EnableEndTurnButton()
    {
        endTurnButton.interactable = true;
        yield break;
    }

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

    public void ShowUnitInfoWindow(Unit unit)
    {
        unitInfoWindow.ShowStats(unit);
    }

    public void HideUnitInfoWindow()
    {
        unitInfoWindow.HideStats();
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
        return unitSelectionWindow.Show();
    }

    public IEnumerator ShowSelectionTutorial()
    {

        yield break;
    }

    public IEnumerator HideSelectionWindow()
    {
        return unitSelectionWindow.Hide();
    }

}
