using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleManager;

public class generatePreviews : MonoBehaviour
{
    public GameObject enemyAvatar;

    BattleManager battleManager;

    // Start is called before the first frame update
    void Start()
    {
        battleManager = FindObjectOfType<BattleManager>();
        transform.position = battleManager.GetState().map.transform.position;
        ShowPreview(battleManager.levelManager.nextSceneenemyInfo, battleManager.GetState());
    }

    private void ShowPreview(List<(int, Vector3Int)> enemyPosNextScene, BattleState state)
    {
        foreach (var loc in enemyPosNextScene)
        {
            var targetTransform = state.map.CellToWorldPosition(loc.Item2);
            GameObject gameObject = Instantiate(enemyAvatar, targetTransform, Quaternion.identity);
            gameObject.transform.SetParent(transform, false);
        }

    }
}
