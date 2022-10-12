using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGrounds : MonoBehaviour
{
    public int LevelNums = 2;
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        string path = string.Format("/Backgrounds/LevelBackgrounds/lvl{0}_background.png", System.Math.Floor((double)LevelManager.currentLevel/LevelNums));
        spriteRenderer.sprite = Resources.Load(path) as Sprite;
    }

}
