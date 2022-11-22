using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class TurnCountDown : MonoBehaviour
{
    public int totalTurn = 10;

    public int currentTurn = 10;

    public static int totalHagfish = 3;
    public static int hagfishLeft = 3;


    public TextMeshProUGUI textElement;

    public Slider slider;

    /// <summary>
    /// Initializes the TurnCountDown object to the given starting value.
    /// Updates the display to the new value
    /// </summary>
    /// <param name="turns">the number of turns to set the count down to</param>
    public void Initialize(int targetNum, bool isBossLevel)
    {
        if (!isBossLevel)
        {
            totalTurn = targetNum;
            currentTurn = targetNum;
            UpdateDisplay();
        }
        else
        {
            hagfishLeft = totalHagfish;
            UpdateDisplayBoss();
        }
    }

    /// <summary>
    /// Updates the countdown display with the current turn count
    /// </summary>
    public void UpdateDisplay()
    {
        textElement.text = string.Format("Turns left:{0}", currentTurn);
    }

    public void UpdateDisplayBoss()
    {
        textElement.text = string.Format("Hagfish left:{0}", hagfishLeft);
    }

    /// <summary>
    /// Decreases the turn counter by 1
    /// </summary>
    public void Decrement()
    {
        currentTurn -= 1;
        UpdateDisplay();
    }

    public void DecrementBoss()
    {
        hagfishLeft -= 1;
        UpdateDisplayBoss();
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize(totalTurn, LevelManager.currentLevel > LevelManager.instance.totalLevels - LevelManager.instance.BossLevels);
    }
}
