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
    public int x_min = -4;
    public int x_max = 4;
    public int y_min = -2;
    public int y_max = 3;

    public int totalLevels = 4;
    public int currentLevel = 0;
    public int enemyNumToSpawn = 0;

    public bool random = true;

    public Dictionary<Vector3Int, TileDataScriptableObject> tileInfo = new Dictionary<Vector3Int, TileDataScriptableObject>();
    public Dictionary<Vector3Int, TileDataScriptableObject> nextSceneTileInfo = new Dictionary<Vector3Int, TileDataScriptableObject>();

    public List<(int, Vector3Int)> enemyInfo = new List<(int, Vector3Int)>();
    public List<(int, Vector3Int)> nextSceneenemyInfo = new List<(int, Vector3Int)>();

    public List<EnemyUnit> typesOfEnemiesToSpawn;
    public List<TileDataScriptableObject> typesOfTilesToSpawn;


    List<Vector3Int> impassibleTile = new List<Vector3Int>();

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (random)
        {
            RefreshNewGame();
        }
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
    public void RefreshNewGame()
    {
        currentLevel = 0;
        tileInfo = new Dictionary<Vector3Int, TileDataScriptableObject>();
        nextSceneTileInfo = new Dictionary<Vector3Int, TileDataScriptableObject>();
        nextSceneenemyInfo = new List<(int, Vector3Int)>();
        enemyInfo = new List<(int, Vector3Int)>();
        fillTileInfo(tileInfo);
        fillTileInfo(nextSceneTileInfo);
        fillEnemyInfo(enemyInfo, tileInfo, 0);
        fillEnemyInfo(nextSceneenemyInfo, nextSceneTileInfo, 1);
    }

    public void fillTileInfo(Dictionary<Vector3Int, TileDataScriptableObject> curTileInfo)
    {
        int total_weight = 0;
        foreach (var tile in typesOfTilesToSpawn)
            total_weight += tile.weight;

        for (int x = -3; x < 4; x++)
        {
            for (int y = -2; y < 3; y++)
            {
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


    public void fillEnemyInfo(List<(int, Vector3Int)> curEnemyInfo, Dictionary<Vector3Int, TileDataScriptableObject> curTileInfo, int currentLevel)
    {
        Debug.Log("############");
        Debug.Log(currentLevel);
        int totalEnemy = currentLevel < 2 ? currentLevel + 1 : 3;
        var possiblePositions = new List<Vector3Int>();
        for (int x = -3; x < 4; x++)
        {
            for (int y = -2; y < 3; y++)
            {
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

    public void PrepareNextBattle()
    {
        currentLevel++;

        if (!random)
        {
            return;
        }

        tileInfo = nextSceneTileInfo;
        nextSceneTileInfo = new Dictionary<Vector3Int, TileDataScriptableObject>();
        fillTileInfo(nextSceneTileInfo);
        enemyInfo = nextSceneenemyInfo;
        nextSceneenemyInfo = new List<(int, Vector3Int)>();
        fillEnemyInfo(nextSceneenemyInfo, nextSceneTileInfo, currentLevel);
    }
}
