using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishEnemy : EnemyUnit
{
    public override IEnumerator performAction(BattleState state)
    {
        PlayerUnit closest = FindClosestPlayerUnit(state);

        if(!closest)
        {
            // No player units? Something's wrong
            yield break;
        }

        // Move as far as possible towards the closest unit
        List<Vector3Int> path = state.map.FindShortestPath(location, closest.location);
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

        if(goal == closest.location)
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

        // Attack the unit if they're in range
        if(!isDead && IsTileInAttackRange(closest.location, state.map))
        {
            yield return StartCoroutine(DoAttack(closest));
            yield return new WaitForSeconds(0.2f);
        }
    }
}
