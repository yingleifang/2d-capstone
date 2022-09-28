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
    public TurnCountDown turnCountDown;
    public bool isTutorial;
    public new AudioComponent audio;
    [SerializeField]
    private SoundEffect clickSound;

    /// <summary>
    /// Displays the given tile data in the info window
    /// </summary>
    /// <param name="tileData">the tile data to display. Will do nothing if null</param>
    public void ShowTileInWindow(HexTileData tileData)
    {
        unitInfoWindow.ShowTile(tileData);
    }

    /// <summary>
    /// Initializes the turn count down to the given value
    /// </summary>
    /// <param name="turns">the number of turns to set the turn counter to</param>
    public void InitializeTurnCount(int turns)
    {
        turnCountDown.Initialize(turns);
    }

    /// <summary>
    /// Indicates whether the battle is over due to the turn count down
    /// </summary>
    /// <returns>returns true if the turn count down is 0 or less, false otherwise</returns>
    public bool isOutOfTurns()
    {
        return turnCountDown && turnCountDown.currentTurn <= 0;
    }

    /// <summary>
    /// Decreases the turn count down by 1
    /// </summary>
    public void DecrementTurnCount()
    {
        turnCountDown.Decrement();
    }

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

    public IEnumerator SwitchScene(int index, int fadeSpeed = 2)
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
        
        SceneManager.LoadScene(index);
    }

    public IEnumerator FadeOut(int fadeSpeed = 2)
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
    }

    public IEnumerator FadeIn(int fadeSpeed = 2)
    {
        Color goColor = blackSquare.GetComponent<Image>().color;
        float newA = 0;
        while (blackSquare.GetComponent<Image>().color.a > 0)
        {
            newA = goColor.a - (fadeSpeed * Time.deltaTime);
            //Debug.Log(newA);
            goColor = new Color (goColor.r, goColor.g, goColor.b, newA);
            blackSquare.GetComponent<Image>().color = goColor;
            yield return null;
        }          
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
        StartCoroutine(SwitchScene(SceneManager.GetActiveScene().buildIndex + 1, 2));
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

    public void PlayClickSound()
    {
        audio.PlaySound(clickSound);
    }

}
