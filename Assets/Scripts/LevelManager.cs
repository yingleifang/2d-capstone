using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
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
    public int enemyNum = 0;

    public List<(TileDataScriptableObject, Vector3Int)> tileInfo = new List<(TileDataScriptableObject, Vector3Int)>();
    public List<(TileDataScriptableObject, Vector3Int)> nextSceneTileInfo = new List<(TileDataScriptableObject, Vector3Int)>();

    public List<(EnemyUnit, Vector3Int)> enemyInfo = new List<(EnemyUnit, Vector3Int)>();
    public List<(EnemyUnit, Vector3Int)> nextSceneenemyInfo = new List<(EnemyUnit, Vector3Int)>();

    public TileManager tileManager;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        tileManager = FindObjectOfType<TileManager>();
        fillTileInfo(tileInfo);
        fillTileInfo(nextSceneTileInfo);
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


}
