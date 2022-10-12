using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGrounds : MonoBehaviour
{
    public int LevelNums = 2;
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        int index = (LevelManager.currentLevel + 1) / LevelNums <= 0 ? 1 : (LevelManager.currentLevel + 1) / LevelNums;
        string path = string.Format("LevelBackgrounds/lvl{0}_background", index);
        Debug.Log(LevelManager.currentLevel);
        Debug.Log("?///////////////////");
        spriteRenderer.sprite = Resources.Load<Sprite>(path);
    }

}
