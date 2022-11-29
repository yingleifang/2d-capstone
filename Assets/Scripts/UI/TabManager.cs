using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    private Button selected;
    public Button startingSelected;

    public void Start()
    {
        if (startingSelected)
        {
            Select(startingSelected);
        }
    }

    public void Select(Button button)
    {
        if (selected)
        {
            selected.animator.SetBool("Select", false);
        }

        button.animator.SetBool("Select", true);
        selected = button;
    }
}
