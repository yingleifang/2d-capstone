using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    //public GameObject startButton;
    //public GameObject settingsButton;
    //public GameObject quitButton;
    // Start is called before the first frame update

    public void ButtonSelect(GameObject button)
    {
        LeanTween.scale(button, new Vector3(1.5f, 1.5f, 1.5f), 0.1f);
    }

    public void ButtonDeselect(GameObject button)
    {
        LeanTween.scale(button, new Vector3(1, 1, 1), 0.1f);
    }


}
