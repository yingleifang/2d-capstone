using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleManager;

[Serializable]
public class Overlay
{
    public Sprite sprite;
    [TextArea]
    public string description;
}

public class generatePreviews : MonoBehaviour
{
    public GameObject enemyAvatar;
    public Overlay enemyOverlay;

    public GameObject hazardAvatar;
    public Overlay hazardOverlay;

    public GameObject impassibleAvatar;
    public Overlay impassibleOverlay;

    public void ShowEnemyPreview(List<(int, Vector3Int)> nextSceneenemyInfo, BattleState state)
    {
        foreach (var loc in nextSceneenemyInfo)
        {
            var targetTransform = state.tileManager.CellToWorldPosition(loc.Item2);
            GameObject gameObject = Instantiate(enemyAvatar, targetTransform, Quaternion.identity);
            gameObject.transform.SetParent(transform, false);
            state.tileManager.SetTileOverlay(loc.Item2, enemyOverlay);
        }

    }

    public void ShowHazzardPreview(Dictionary<Vector3Int, (TileDataScriptableObject, bool)> nextSceneTileInfo, BattleState state)
    {
        foreach (var loc in nextSceneTileInfo)
        {
            if (loc.Value.Item1.hazardous == true)
            {
                var targetTransform = state.tileManager.CellToWorldPosition(loc.Key);
                GameObject gameObject = Instantiate(hazardAvatar, targetTransform, Quaternion.identity);
                gameObject.transform.SetParent(transform, false);
                state.tileManager.SetTileOverlay(loc.Key, hazardOverlay);
            } else if (loc.Value.Item1.impassable == true)
            {
                var targetTransform = state.tileManager.CellToWorldPosition(loc.Key);
                GameObject gameObject = Instantiate(impassibleAvatar, targetTransform, Quaternion.identity);
                gameObject.transform.SetParent(transform, false);
                state.tileManager.SetTileOverlay(loc.Key, impassibleOverlay);
            }
        }

    }
}
