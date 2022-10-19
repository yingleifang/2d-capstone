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
    public static int currentLevel = 1;
    public static int levelNums = 2;

    public int totalLevels;
    
    public int enemyNumToSpawn = 0;

    public bool random = true;

    public int[] crackChance = new int[] {60, 30};

    public Dictionary<Vector3Int, (TileDataScriptableObject, bool)> tileInfo = new Dictionary<Vector3Int, (TileDataScriptableObject, bool)>();
    public Dictionary<Vector3Int, (TileDataScriptableObject, bool)> nextSceneTileInfo = new Dictionary<Vector3Int, (TileDataScriptableObject, bool)>();

    List<(int, int)> boundList = new List<(int, int)>{(3, -4),(4, -4),(4, -5),(5, -5), (4, -5), (4, -4), (3, -4)};
    int y_range = 3;

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
    [Tooltip("hole tile need to always be the first tile in the list, shattered tiles turn into the first tile in the list")]
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
            SetLevelCounter(1);
            LevelSetup();
        }
        map = FindObjectOfType<Tilemap>();
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
    /// generates enemies based on int currentLevel. Does not set currentLevel
    /// </summary>  
    public void LevelSetup()
    {
        tileInfo = new Dictionary<Vector3Int, (TileDataScriptableObject, bool)>();
        nextSceneTileInfo = new Dictionary<Vector3Int, (TileDataScriptableObject, bool)>();
        nextSceneEnemyInfo = new List<(int, Vector3Int)>();
        enemyInfo = new List<(int, Vector3Int)>();
        fillTileInfo(tileInfo);
        fillTileInfo(nextSceneTileInfo);
        fillEnemyInfo(enemyInfo, tileInfo, currentLevel);
        fillEnemyInfo(nextSceneEnemyInfo, nextSceneTileInfo, currentLevel + 1);
    }

    /// <summary>
    /// Resets data values to prepare for new game. Does not generate enemy or tile values
    /// </summary>  
    public void RefreshNewGame()
    {
        currentLevel = 1;
        isTutorial = true;
        tileInfo = new Dictionary<Vector3Int, (TileDataScriptableObject, bool)>();
        nextSceneTileInfo = new Dictionary<Vector3Int, (TileDataScriptableObject, bool)>();
        nextSceneEnemyInfo = new List<(int, Vector3Int)>();
        enemyInfo = new List<(int, Vector3Int)>();
    }

    public void SetLevelCounter(int level)
    {
        currentLevel = level;
    }

    /// <summary>
    /// Fills the dict with random tile SO's at random vector3's (weight matters)
    /// </summary> 
    public void fillTileInfo(Dictionary<Vector3Int, (TileDataScriptableObject, bool)> curTileInfo)
    {
        map = FindObjectOfType<Tilemap>();
        int total_weight = 0;
        foreach (var tile in typesOfTilesToSpawn)
            total_weight += tile.weight;

        for (int y = -y_range; y <= y_range; y++)
        {
                var bound = boundList[y + y_range];
                for (int x = bound.Item2; x <= bound.Item1; x++)
                {
                    var rngNum = RandomNumberGenerator.GetInt32(1, total_weight + 1);
                    int index = 0;
                    while (rngNum > 0)
                    {
                        rngNum -= typesOfTilesToSpawn[index].weight;
                        index++;
                    }
                    if (typesOfTilesToSpawn[index - 1].impassable)
                    {
                        impassibleTile.Add(new Vector3Int(x, y, 0));
                    }
                    if (x == bound.Item2 || y == -y_range || x == bound.Item1 || y == y_range)
                    {
                        var crackNum = RandomNumberGenerator.GetInt32(0, 100);
                        if (crackNum < crackChance[0])
                        {
                            curTileInfo.Add(new Vector3Int(x, y, 0), (typesOfTilesToSpawn[index - 1], true));
                        }
                        else
                        {
                            curTileInfo.Add(new Vector3Int(x, y, 0), (typesOfTilesToSpawn[index - 1], false));
                        }
                    }
                    else if (x == bound.Item2 + 1 || y == -y_range + 1 || x == bound.Item1 - 1|| y == y_range - 1)
                    {
                        var crackNum = RandomNumberGenerator.GetInt32(0, 100);
                        if (crackNum < crackChance[1])
                        {
                            curTileInfo.Add(new Vector3Int(x, y, 0), (typesOfTilesToSpawn[index - 1], true));
                        }
                        else
                        {
                            curTileInfo.Add(new Vector3Int(x, y, 0), (typesOfTilesToSpawn[index - 1], false));
                        }
                    }
                    else
                    {
                        curTileInfo.Add(new Vector3Int(x, y, 0), (typesOfTilesToSpawn[index - 1], false));
                    }
                }
        }
    }

    /// <summary>
    /// Fills the list with indexes of enemy prefabs and locations of enemy prefabs (random).
    /// </summary> 
    public void fillEnemyInfo(List<(int, Vector3Int)> curEnemyInfo, Dictionary<Vector3Int, (TileDataScriptableObject, bool)> curTileInfo, int currentLevel)
    {
        map = FindObjectOfType<Tilemap>();
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
                if (curTileInfo[new Vector3Int(x, y, 0)].Item1.impassable == true || curTileInfo[new Vector3Int(x, y, 0)].Item1.hazardous == true)
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
        if (currentLevel + 1 <= numTutorialLevels && !overrideTutorial)
        {
            currentLevel++;
            return;
        }
        //Otherwise, we know we're finished with the tutorial
        else
        {
            isTutorial = false;
            //In the case that we start with tutorial levels, we will not have called LevelSetup()
            Debug.Log("nextsceneenemyinfo count :" + nextSceneEnemyInfo.Count);
            if (nextSceneEnemyInfo.Count == 0)
            {
                Debug.Log("nextsceneenemyinfoinside: " + nextSceneEnemyInfo.Count);
                // Set to zero because of increment after
                SetLevelCounter(0);
                LevelSetup();
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
        nextSceneTileInfo = new Dictionary<Vector3Int, (TileDataScriptableObject, bool)>();
        fillTileInfo(nextSceneTileInfo);
        enemyInfo = nextSceneEnemyInfo;
        nextSceneEnemyInfo = new List<(int, Vector3Int)>();
        fillEnemyInfo(nextSceneEnemyInfo, nextSceneTileInfo, currentLevel + 1);
    }
}
