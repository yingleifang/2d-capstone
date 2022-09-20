using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class TurnCountDown : MonoBehaviour
{
    public int totalTurn = 10;

    public int currentTurn = 10;

    public Slider slider;

    public void SetCount()
    {
        currentTurn -= 1;
        slider.value = currentTurn;
    }

    // Start is called before the first frame update
    void Start()
    {
        slider.maxValue = totalTurn;
        slider.value = totalTurn;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
