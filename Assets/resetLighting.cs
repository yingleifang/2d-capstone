using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class resetLighting : MonoBehaviour
{
    GameObject lightManager;
    // Start is called before the first frame update
    private void Awake()
    {
       

        if (GameObject.Find("LightingManagerBoss") != null)
        {
            lightManager = GameObject.Find("LightingManagerBoss");
            lightManager.SetActive(false); ;
        }


    }
}
