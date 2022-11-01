using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsopodEnemy : EnemyUnit
{
    //Target Logic: Always the closest player unit
    //Ability Logic: No ablity
    //Movement Logic: Does not avoid hazards
    public override IEnumerator performAction(BattleState state)
    {
        PlayerUnit closest = FindClosestPlayerUnit(state);

        if(!closest)
        {
            // No player units? Something's wrong
            Debug.LogError("No player units detected :( (IsopodEnemy performAction method)");
            yield break;
        }

        List<Vector3Int> candidates = state.tileManager.GetTilesInRange(location, currentMovementSpeed);
        Vector3Int dest = location;
        int maxDist = 0;
        foreach (Vector3Int tile in candidates)
        {
            int dist = 0;
            foreach (PlayerUnit player in state.playerUnits)
            {
                dist += state.tileManager.RealDistance(tile, player.location);
            }

            if (dist > maxDist)
            {
                maxDist = dist;
                dest = tile;
            }
        }

        // Move as far as possible away from the closest unit
        yield return StartCoroutine(MoveTowards(state, dest, currentMovementSpeed, false));

        // Attack the unit if they're in range
        if(!isDead && IsTileInAttackRange(closest.location))
        {
            yield return StartCoroutine(DoAttack(closest));
            yield return new WaitForSeconds(0.2f);
        }
    }
}
