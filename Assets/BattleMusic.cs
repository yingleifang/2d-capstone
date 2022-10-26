using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMusic : MonoBehaviour
{
    void Awake()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        int index = (LevelManager.currentLevel - LevelManager.instance.NumTutorialLevels + 1) / LevelManager.levelNums <= 0 ? 1 : 
                    (LevelManager.currentLevel - LevelManager.instance.NumTutorialLevels + 1) / LevelManager.levelNums;
        Debug.Log("INDEXXXXXXXXXXX " + (LevelManager.currentLevel - LevelManager.instance.NumTutorialLevels));
        Debug.Log("INDEXXXXXXXXXXX " + index);
        string path = string.Format("BattleMusic/lvl{0}_battleMusic", index);
        audioSource.clip = Resources.Load<AudioClip>(path);
        audioSource.Play();
    }
}
