using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    // public int x_min = -4;
    // public int x_max = 4;
    // public int y_min = -2;
    // public int y_max = 3;

    public int totalLevels;
    public static int currentLevel = 1;
    public int enemyNumToSpawn = 0;

    public bool random = true;

    public Dictionary<Vector3Int, TileDataScriptableObject> tileInfo = new Dictionary<Vector3Int, TileDataScriptableObject>();
    public Dictionary<Vector3Int, TileDataScriptableObject> nextSceneTileInfo = new Dictionary<Vector3Int, TileDataScriptableObject>();

    /// <summary>
    /// Use the int to index into typesOfEnemiesToSpawn to get enemy prefab. Vector is the map position
    /// </summary>
    public List<(int, Vector3Int)> enemyInfo = new List<(int, Vector3Int)>();
    /// <summary>
    /// Use the int to index into typesOfEnemiesToSpawn to get enemy prefab. Vector is the map position
    /// </summary>
    public List<(int, Vector3Int)> nextSceneEnemyInfo = new List<(int, Vector3Int)>();

    /// <summary>
    /// List of enemy prefabs (populated in editor)
    /// </summary>
    public List<EnemyUnit> typesOfEnemiesToSpawn;
    /// <summary>
    /// List of tileSO's (populated in editor)
    /// </summary>       
    public List<TileDataScriptableObject> typesOfTilesToSpawn;
    public Tilemap map;

    [ReadOnly] public bool isTutorial;
    /// <summary>
    /// Set to true if you want to test non-tutorial levels
    /// </summary>
    public bool overrideTutorial;
    private int numTutorialLevels = 1;

    [HideInInspector]
    public static LevelManager instance;

    List<Vector3Int> impassibleTile = new List<Vector3Int>();

    public levelTransition levelTransitionObj;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if (currentLevel - 1 < numTutorialLevels)
        {
            isTutorial = true;
        }
        if (overrideTutorial)
        {
            Debug.Log("aWake");
            isTutorial = false;
            RefreshNewGame();
        }
        map = FindObjectOfType<Tilemap>();
        Debug.Log("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
        Debug.Log(currentLevel);
    }

    //public void ProcessTypesOfEnemiesToSpawn()
    //{
    //    if (currentLevel < 2)
    //    {
    //        typesOfEnemiesToSpawn = totalTypesOfEnemiesToSpawn[0]
    //    }
    //}

    //public void ProcessTypesOfTilesToSpawn()
    //{

    //}

    /// <summary>
    /// Setup randomized tiles for the current level and the next level
    /// </summary>  
    public void RefreshNewGame()
    {
        currentLevel = 0;
        tileInfo = new Dictionary<Vector3Int, TileDataScriptableObject>();
        nextSceneTileInfo = new Dictionary<Vector3Int, TileDataScriptableObject>();
        nextSceneEnemyInfo = new List<(int, Vector3Int)>();
        enemyInfo = new List<(int, Vector3Int)>();
        fillTileInfo(tileInfo);
        fillTileInfo(nextSceneTileInfo);
        fillEnemyInfo(enemyInfo, tileInfo, currentLevel);
        fillEnemyInfo(nextSceneEnemyInfo, nextSceneTileInfo, currentLevel + 1);
    }

    /// <summary>
    /// Fills the dict with random tile SO's at random vector3's (weight matters)
    /// </summary> 
    public void fillTileInfo(Dictionary<Vector3Int, TileDataScriptableObject> curTileInfo)
    {
        int total_weight = 0;
        foreach (var tile in typesOfTilesToSpawn)
            total_weight += tile.weight;

        for (int x = (int)map.localBounds.min.x; x < map.localBounds.max.x; x++)
        {
            for (int y = (int)map.localBounds.min.y; y < map.localBounds.max.y; y++)
            {
                if (!map.GetTile(new Vector3Int(x, y, 0)))
                {
                    continue;
                }
                var rngNum = RandomNumberGenerator.GetInt32(1, total_weight + 1);
                int index = 0;
                while (rngNum > 0)
                {
                    rngNum -= typesOfTilesToSpawn[index].weight;
                    index++;
                }
                if (typesOfTilesToSpawn[index - 1].impassable) {
                    impassibleTile.Add(new Vector3Int(x, y, 0));
                }

                curTileInfo.Add(new Vector3Int(x, y, 0), typesOfTilesToSpawn[index - 1]);
            }
        }
    }

    /// <summary>
    /// Fills the list with indexes of enemy prefabs and locations of enemy prefabs (random).
    /// </summary> 
    public void fillEnemyInfo(List<(int, Vector3Int)> curEnemyInfo, Dictionary<Vector3Int, TileDataScriptableObject> curTileInfo, int currentLevel)
    {
        int totalEnemy = currentLevel < 3 ? currentLevel: 3;
        var possiblePositions = new List<Vector3Int>();
        for (int x = (int)map.localBounds.min.x; x < map.localBounds.max.x; x++)
        {
            for (int y = (int)map.localBounds.min.y; y < map.localBounds.max.y; y++)
            {
                if (!map.GetTile(new Vector3Int(x, y, 0)))
                {
                    continue;
                }
                if (curTileInfo[new Vector3Int(x, y, 0)].impassable == true || curTileInfo[new Vector3Int(x, y, 0)].hazardous == true)
                {
                    continue;
                }
                possiblePositions.Add(new Vector3Int(x, y, 0));
            }
        }
        Shuffle(possiblePositions);

        for (int i = 0; i < totalEnemy; i++)
        {
            if (currentLevel < 2)
            {
                curEnemyInfo.Add((0, possiblePositions[i]));
            }
            else
            {
                int k = RandomNumberGenerator.GetInt32(typesOfEnemiesToSpawn.Count);
                curEnemyInfo.Add((k, possiblePositions[i]));
            }
            
        }
    }

    /// <summary>
    /// Randomizes the positions of all elements in the list
    /// </summary> 
    public void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = RandomNumberGenerator.GetInt32(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Increments the current level and sets the isTutorial bool accordingly.
    /// Generates current and next level data if we exit tutorial
    /// </summary>
    public void IncrementLevel()
    {
        Debug.Log("############");
        Debug.Log("Current real level: " + currentLevel);
        //levelTransitionObj.LoadNextLevel();
        //Still in tutorial
        if (currentLevel < numTutorialLevels && !overrideTutorial)
        {
            currentLevel++;
            return;
        }
        //Otherwise, we know we're finished with the tutorial
        else
        {
            isTutorial = false;
            //In the case that we start with tutorial levels, we will not have called RefreshNewGame()
            Debug.Log("nextsceneenemyinfo count :" + nextSceneEnemyInfo.Count);
            if (nextSceneEnemyInfo.Count == 0)
            {
                Debug.Log("nextsceneenemyinfoinside: " + nextSceneEnemyInfo.Count);
                Debug.Log("*********************************************");
                RefreshNewGame();
                Debug.Log("nextsceneenemyinfoinside: " + nextSceneEnemyInfo.Count);
            }
            currentLevel++;
        }
        
    }

    /// <summary>
    /// Moves the next scene data to current scene data. Generates new data for next scene.
    /// Call this after loading the next scene.
    /// </summary>
    public void PrepareNextBattle()
    {
        map = FindObjectOfType<Tilemap>();

        tileInfo = nextSceneTileInfo;
        nextSceneTileInfo = new Dictionary<Vector3Int, TileDataScriptableObject>();
        fillTileInfo(nextSceneTileInfo);
        enemyInfo = nextSceneEnemyInfo;
        nextSceneEnemyInfo = new List<(int, Vector3Int)>();
        Debug.Log("!!!!!!!!!!!!!");
        Debug.Log(currentLevel);
        fillEnemyInfo(nextSceneEnemyInfo, nextSceneTileInfo, currentLevel + 1);
    }
}
