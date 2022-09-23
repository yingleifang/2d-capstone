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
            Debug.Log("No player units detected (AnglerEnemy performAction method) :(");
            yield break;
        }

        // Move as far as possible towards the furthest unit
        yield return StartCoroutine(MoveTowards(state, target.location, currentMovementSpeed));

        PlayerUnit abilityAlternateTarget = FindFurthestPlayerUnit(state);
        
        //Don't want to use ability if it is on cooldown, or if there are no units who can move any closer to us
        Debug.Log("real distance: " + state.map.RealDistance(location, abilityAlternateTarget.location));
        if (currentCoolDown > 0 || state.map.RealDistance(location, abilityAlternateTarget.location) <= numSpacesLured)
        {
            Debug.Log("Attacking");
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
                    Unit currentUnit = state.map.dynamicTileDatas[coord].unit;
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
            Debug.Log("Using ability");
            currentCoolDown = coolDown;
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
