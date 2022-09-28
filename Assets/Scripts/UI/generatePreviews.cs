using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleManager;

public class generatePreviews : MonoBehaviour
{
    public GameObject enemyAvatar;

    public GameObject hazzardAvatar;

    public GameObject impassibleAvatar;

    public void ShowEnemyPreview(List<(int, Vector3Int)> nextSceneenemyInfo, BattleState state)
    {
        foreach (var loc in nextSceneenemyInfo)
        {
            var targetTransform = state.map.CellToWorldPosition(loc.Item2);
            GameObject gameObject = Instantiate(enemyAvatar, targetTransform, Quaternion.identity);
            gameObject.transform.SetParent(transform, false);
        }

    }

    public void ShowHazzardPreview(Dictionary<Vector3Int, TileDataScriptableObject> nextSceneTileInfo, BattleState state)
    {
        foreach (var loc in nextSceneTileInfo)
        {
            if (loc.Value.hazardous == true)
            {
                var targetTransform = state.map.CellToWorldPosition(loc.Key);
                GameObject gameObject = Instantiate(hazzardAvatar, targetTransform, Quaternion.identity);
                gameObject.transform.SetParent(transform, false);
            }else if (loc.Value.impassable == true)
            {
                var targetTransform = state.map.CellToWorldPosition(loc.Key);
                GameObject gameObject = Instantiate(impassibleAvatar, targetTransform, Quaternion.identity);
                gameObject.transform.SetParent(transform, false);
            }
        }

    }
}
