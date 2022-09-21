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

        // Move as far as possible towards the furthest unit
        List<Vector3Int> path = state.map.FindShortestPath(location, furthest.location);
        Vector3Int goal;
        int goalIndex;
        if(currentMovementSpeed > path.Count)
        {
            goalIndex = path.Count - 1;
        } else
        {
            goalIndex = currentMovementSpeed - 1;
        }

        if(goalIndex < 0)
        {
            goal = location;
        } else
        {
            goal = path[goalIndex];
        }

        if(goal == furthest.location)
        {
            if(goalIndex - 1 < 0)
            {
                goal = location;
            } else
            {
                goal = path[goalIndex - 1];
            }
        }
        yield return StartCoroutine(DoMovement(state, goal));
        yield return new WaitForSeconds(0.1f);

        List<Vector3Int> tilesInRange = GetTilesInAttackRange();
        Unit targetUnit = null;

        foreach (Vector3Int coord in tilesInRange)
        {
            if(!isDead && BattleManager.instance.map.dynamicTileDatas[coord].unit != null)
            {
                targetUnit = BattleManager.instance.map.dynamicTileDatas[coord].unit;
                break;
            }
        }
        yield return StartCoroutine(DoAttack(targetUnit));
        yield return new WaitForSeconds(0.2f);
    }
}
