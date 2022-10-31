using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyfishEnemy : EnemyUnit
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
            Debug.LogError("No player units detected :( (FishEnemy performAction method)");
            yield break;
        }

        // Move as far as possible towards the closest unit
        yield return StartCoroutine(MoveTowards(state, closest.location, currentMovementSpeed));

        // Attack all adjacent units
        if(!isDead)
        {
            List<Vector3Int> adjacentTiles = state.tileManager.GetTilesInRange(location, 1, false);
            foreach (Vector3Int adjacentTile in adjacentTiles)
            {
                
                if (adjacentTile != location)
                {
                    Unit unit = state.tileManager.GetUnit(adjacentTile);
                    if (unit != null)
                    {
                        unit.ChangeHealth(attackDamage * -1);
                    }
                }
            }
            audio.PlaySound(attackSound);
            yield return StartCoroutine(PlayAttackAnimation());
            yield return new WaitForSeconds(0.2f);
        }
    }
}
