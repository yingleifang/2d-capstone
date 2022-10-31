using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SetButtonFunction2 : MonoBehaviour
{
    public Button button;

    private void Start() 
    {
        button.onClick.AddListener(BattleManager.instance.SkipTutorialButton);
    }
}
