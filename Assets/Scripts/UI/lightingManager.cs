using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class lightingManager : MonoBehaviour
{
    public static lightingManager instance;
    LevelManager levelManager;
    GameObject lightGO;
    int current;
    int totalLevels;
    int bossLvls;

    GameObject ray1;
    GameObject ray2;
    GameObject tilemapGlobal;
    GameObject bgGlobal;
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



    void onSceneLoad(Scene scene, LoadSceneMode mode)
    {

        current = LevelManager.currentLevel;
        totalLevels = levelManager.totalLevels;
        bossLvls = levelManager.BossLevels;

        Debug.Log(current);

        //change lights to match new bg
        if (current > 2)
        {
            changeLights();
        }



    }
    void changeLights()
    {
        lightGO = gameObject.transform.GetChild(1).gameObject;

        ray1 = lightGO.transform.GetChild(0).gameObject;
        ray2 = lightGO.transform.GetChild(1).gameObject;
        tilemapGlobal = lightGO.transform.GetChild(2).gameObject;
        bgGlobal = lightGO.transform.GetChild(3).gameObject;

        ray1.GetComponent<Light2D>().color = new Color32(214, 248, 239, 255);
        ray1.GetComponent<Light2D>().intensity = 0.31f;

        ray2.GetComponent<Light2D>().color = new Color32(214, 248, 239, 255);
        ray2.GetComponent<Light2D>().intensity = 0.27f;

        tilemapGlobal.GetComponent<Light2D>().color = new Color32(177, 171, 190, 255);
        tilemapGlobal.GetComponent<Light2D>().intensity = 1.22f;

        tilemapGlobal.GetComponent<Light2D>().color = new Color32(164, 164, 164, 255);
        tilemapGlobal.GetComponent<Light2D>().intensity = 1.2f;

    }
}


