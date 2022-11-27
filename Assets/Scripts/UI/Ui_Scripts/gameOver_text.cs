using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gameOver_text : MonoBehaviour
{
    string current;
    string total;
    LevelManager levelManager;
    string myString;
    
  
    // Start is called before the first frame update
    void Start()
    {
        // Gets the current level and total level from Level manager, and creates a string to give to current/total game objs
        if (levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>())
        {
            current = LevelManager.currentLevel.ToString();
            total = levelManager.totalLevels.ToString();
            myString = "You've lasted " + current + " out of " + total + " levels";
            //Debug.Log(myString);
            GetComponent<TMPro.TextMeshProUGUI>().text = myString;


        }

    }

}
