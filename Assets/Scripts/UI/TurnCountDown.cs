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

    public TextMeshProUGUI textElement;

    public Slider slider;

    /// <summary>
    /// Initializes the TurnCountDown object to the given starting value.
    /// Updates the display to the new value
    /// </summary>
    /// <param name="turns">the number of turns to set the count down to</param>
    public void Initialize(int turns)
    {
        totalTurn = turns;
        currentTurn = turns;
        slider.maxValue = turns;
        UpdateDisplay();
    }

    /// <summary>
    /// Updates the countdown display with the current turn count
    /// </summary>
    public void UpdateDisplay()
    {
        slider.value = currentTurn;
        textElement.text = string.Format("Turns left:{0}", currentTurn);
    }

    /// <summary>
    /// Decreases the turn counter by 1
    /// </summary>
    public void Decrement()
    {
        currentTurn -= 1;
        UpdateDisplay();
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize(totalTurn);
    }
}
