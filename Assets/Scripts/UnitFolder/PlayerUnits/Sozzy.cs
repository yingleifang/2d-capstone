using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sozzy : PlayerUnit
{
    public SoundEffect StartOfBattleAbilitySound;
    public int abilityDamage;

    public override IEnumerator StartOfBattleAbility(BattleState state)
    {
        Debug.Log("Speeding up adjacent units");
        List<Vector3Int> tiles = state.tileManager.GetTilesInRange(location, 1, false);
        audio.PlaySound(StartOfBattleAbilitySound);
        foreach (Vector3Int tile in tiles)
        {
            Unit unit = state.tileManager.GetUnit(tile);
            if (unit && unit != this && unit is PlayerUnit player)
            {
                player.currentMovementSpeed += 1;
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield break;
    }

    /// <summary>
    /// Does damage in a straight line from Sozzy. Assume that the
    /// coordinate passed in is a valid target
    /// </summary>
    public override IEnumerator UseAbility(Vector3Int target, BattleState state)
    {
        Debug.Log("USING ABILITY");
        List<Vector3Int> path = state.tileManager.FindShortestPath(location, target, false, false);
        if (!state.tileManager.IsStraightPath(path) || path.Count > abilityRange || target == location)
        {
            yield break;
        }
        else
        {
            foreach (Vector3Int coord in path)
            {
                Unit curUnit = state.tileManager.GetUnit(coord);
                Debug.Log(curUnit);
                if (!curUnit)
                {
                    Debug.Log(curUnit);
                    continue;
                }
                else
                {
                    Debug.Log(curUnit);
                    curUnit.ChangeHealth(abilityDamage * -1);
                }
            }
            currentCoolDown = coolDown;
        }
        yield return null;
    }
}
