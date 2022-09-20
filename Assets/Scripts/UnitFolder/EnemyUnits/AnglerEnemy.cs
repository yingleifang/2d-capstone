using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnglerEnemy : EnemyUnit
{
    public override IEnumerator performAction(BattleState state)
    {
        PlayerUnit furthest = FindFurthestPlayerUnit(state);

        if(!furthest)
        {
            // No player units? Something's wrong
            yield break;
        }
    }
}
