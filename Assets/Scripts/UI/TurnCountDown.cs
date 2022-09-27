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

    public void SetCount()
    {
        currentTurn -= 1;
        slider.value = currentTurn;
        textElement.text = string.Format("Turn left:{0}", currentTurn);
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
