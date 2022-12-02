using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightingManagerBoss : MonoBehaviour
{
    public static lightingManagerBoss instance;
    LevelManager levelManager;
    GameObject oldLightManager;
    GameObject lightGO;
    // Start is called before the first frame update
    private void Awake()
    {
        // turn off global light, bc u can't see shit if its off in the editor
        gameObject.transform.GetChild(0).gameObject.SetActive(false);


        if (GameObject.Find("LIghtManager-lvls") != null)
        {
            oldLightManager = GameObject.Find("LIghtManager-lvls");
            oldLightManager.SetActive(false);
        }

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
}
