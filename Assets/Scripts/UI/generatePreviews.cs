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
        ShowPreview(battleManager.enemyPosNextScene, battleManager.GetState());
    }

    private void ShowPreview(EnemyPosNextScene enemyPosNextScene, BattleState state)
    {
        Debug.Log(enemyPosNextScene.locations.Count);
        foreach (var loc in enemyPosNextScene.locations)
        {
            var targetTransform = state.map.CellToWorldPosition(loc);
            GameObject gameObject = Instantiate(enemyAvatar, targetTransform, Quaternion.identity);
            gameObject.transform.SetParent(transform, false);
        }

    }
}
