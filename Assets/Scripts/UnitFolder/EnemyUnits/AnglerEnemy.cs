using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnglerEnemy : EnemyUnit
{
    private PlayerUnit target = null;
    
    //Number of spaces the ability lures the target unit
    private int numSpacesLured = 1;

    //Target Logic: 
    //Chooses the furthest player unit away from him as a target and maintains
    //this same target until the target dies. Chooses his next target the same way.
    //
    //Ability Logic: 
    //Uses his ability on his target. If his target is in attack range, uses it instead
    //on the unit furthest away from him
    //
    //Movement Logic: Does not avoid hazards
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
        if (currentCoolDown > 0 || state.map.RealDistance(location, abilityAlternateTarget.location) <= numSpacesLured)
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
