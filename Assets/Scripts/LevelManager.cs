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

    public int currentLevel = 0;
    public int enemyNumToSpawn = 0;

    public List<(TileDataScriptableObject, Vector3Int)> tileInfo = new List<(TileDataScriptableObject, Vector3Int)>();
    public List<(TileDataScriptableObject, Vector3Int)> nextSceneTileInfo = new List<(TileDataScriptableObject, Vector3Int)>();

    public List<(int, Vector3Int)> enemyInfo = new List<(int, Vector3Int)>();
    public List<(int, Vector3Int)> nextSceneenemyInfo = new List<(int, Vector3Int)>();

    public List<EnemyUnit> typesOfEnemiesToSpawn;

    public TileManager tileManager;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        RefreshNewGame();
    }

    public void RefreshNewGame()
    {
        Debug.Log("$$$$$$$$$$$$$$$$$");        
        tileManager = FindObjectOfType<TileManager>();
        currentLevel = 0;
        tileInfo = new List<(TileDataScriptableObject, Vector3Int)>();
        nextSceneTileInfo = new List<(TileDataScriptableObject, Vector3Int)>();
        nextSceneenemyInfo = new List<(int, Vector3Int)>();
        enemyInfo = new List<(int, Vector3Int)>();
        fillTileInfo(tileInfo);
        fillTileInfo(nextSceneTileInfo);
        fillEnemyInfo(enemyInfo, 0);
        fillEnemyInfo(nextSceneenemyInfo, 1);
    }

    public void fillTileInfo(List<(TileDataScriptableObject, Vector3Int)> curTileInfo)
    {
        int total_weight = 0;
        foreach (var tile in tileManager.tileDatas)
            total_weight += tile.weight;

        for (int x = -3; x < 4; x++)
        {
            for (int y = -2; y < 3; y++)
            {
                var rngNum = RandomNumberGenerator.GetInt32(1, total_weight + 1);
                int index = 0;
                while (rngNum > 0)
                {
                    rngNum -= tileManager.tileDatas[index].weight;
                    index++;
                }
                curTileInfo.Add((tileManager.tileDatas[index - 1], new Vector3Int(x, y, 0)));
            }
        }
    }


    public void fillEnemyInfo(List<(int, Vector3Int)> curEnemyInfo, int currentLevel)
    {
        int totalEnemy = currentLevel + 1;
        var possiblePositions = new List<Vector3Int>();
        for (int x = -3; x < 4; x++)
        {
            for (int y = -2; y < 3; y++)
            {
                possiblePositions.Add(new Vector3Int(x, y, 0));
            }
        }
        Shuffle(possiblePositions);

        for (int i = 0; i < totalEnemy; i++)
        {
            curEnemyInfo.Add((0, possiblePositions[i]));
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

    public void PreparNextBattle()
    {
        currentLevel++;
        tileManager = FindObjectOfType<TileManager>();
        tileInfo = nextSceneTileInfo;
        nextSceneTileInfo = new List<(TileDataScriptableObject, Vector3Int)>();
        fillTileInfo(nextSceneTileInfo);
        enemyInfo = nextSceneenemyInfo;
        nextSceneenemyInfo = new List<(int, Vector3Int)>();
        fillEnemyInfo(nextSceneenemyInfo, currentLevel);
    }
}
