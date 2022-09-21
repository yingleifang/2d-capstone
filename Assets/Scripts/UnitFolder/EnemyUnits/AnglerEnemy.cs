using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnglerEnemy : EnemyUnit
{
    private PlayerUnit target = null;
    private int numSpacesLured = 1;
    public override IEnumerator performAction(BattleState state)
    {
        if (coolDown > 0)
        {
            coolDown--;
        }
        if (target == null || target.currentHealth <= 0)
        {
            target = FindFurthestPlayerUnit(state);
        }

        if(!target)
        {
            // No player units? Something's wrong
            yield break;
        }

        // Move as far as possible towards the furthest unit
        yield return StartCoroutine(MoveTowards(state, target.location, currentMovementSpeed));

        PlayerUnit abilityAlternateTarget = FindFurthestPlayerUnit(state);
        if (currentCoolDown > 0 || BattleManager.instance.map.RealDistance(location, abilityAlternateTarget.location) < numSpacesLured)
        {
            if (IsTileInAttackRange(target.location))
            {
                yield return StartCoroutine(DoAttack(target));
                yield return new WaitForSeconds(0.2f);   
            }
            else
            {
                List<Vector3Int> tilesInRange = GetTilesInAttackRange();
                PlayerUnit targetUnit = null;

                foreach (Vector3Int coord in tilesInRange)
                {
                    Unit currentUnit = BattleManager.instance.map.dynamicTileDatas[coord].unit;
                    if(!isDead && currentUnit != null && currentUnit is PlayerUnit)
                    {
                        targetUnit = (PlayerUnit)currentUnit;
                        break;
                    }
                }
                if (targetUnit != null)
                {
                    yield return StartCoroutine(DoAttack(targetUnit));
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
        else
        {
            if (IsTileInAttackRange(target.location))
            {
                yield return StartCoroutine(abilityAlternateTarget.MoveTowards(state, location, numSpacesLured));
            }
            else
            {
                yield return StartCoroutine(target.MoveTowards(state, location, numSpacesLured));
            }
        }
    }
}
