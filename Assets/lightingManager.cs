using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightingManager : MonoBehaviour
{
    public static lightingManager instance;
    LevelManager levelManager;
    int currentIndex;
    int totalLevels;
    private void Awake()
    {
        // turn off global light, bc u can't see shit if its off in the editor
        gameObject.transform.GetChild(0).gameObject.SetActive(false);

        // add to do not destroy. 
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Start()
    {

        gameObject.transform.GetChild(1).gameObject.SetActive(true);

    }

    //public static void changeLights()
  //  {
        // time to switch to boss lights
    //    instance.gameObject.transform.GetChild(1).gameObject.SetActive(false);
   //     instance.gameObject.transform.GetChild(2).gameObject.SetActive(true);   
   // }
}

