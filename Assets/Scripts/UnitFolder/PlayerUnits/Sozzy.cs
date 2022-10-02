using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sozzy : PlayerUnit
{
    public SoundEffect StartOfBattleAbilitySound;
    public int abilityDamage;
    public int abilityRange;
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
    public override IEnumerator UseAbility(Vector3Int target)
    {
        yield return null;
    }
}
