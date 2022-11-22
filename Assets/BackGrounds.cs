using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGrounds : MonoBehaviour
{
    void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        int index = (LevelManager.currentLevel - 1) / 2;
        string path = string.Format("LevelBackgrounds/lvl{0}_background", index);
        spriteRenderer.sprite = Resources.Load<Sprite>(path);
    }

}
