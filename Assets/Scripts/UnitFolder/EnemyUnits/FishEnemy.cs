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
        yield return StartCoroutine(MoveTowards(state, closest.location, currentMovementSpeed));

        // Attack the unit if they're in range
        if(!isDead && IsTileInAttackRange(closest.location))
        {
            yield return StartCoroutine(DoAttack(closest));
            yield return new WaitForSeconds(0.2f);
        }
    }
}
