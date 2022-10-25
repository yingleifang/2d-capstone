using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mori : PlayerUnit
{
    public SoundEffect StartOfBattleAbilitySound;
    public int abilityDamage;

    public override IEnumerator StartOfBattleAbility(BattleState state)
    {
        Debug.Log("Speeding up adjacent units");
        List<Vector3Int> tiles = state.tileManager.GetTilesInRange(location, 1, false);
        yield return StartCoroutine(PlayEOBAnim());
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
    /// Revives the unit that last died on a tile in range
    /// </summary>
    public override IEnumerator UseAbility(Vector3Int target, BattleState state)
    {
        Debug.Log("USING ABILITY");
        TileManager map = state.tileManager;
        if (target == location || !map.InBounds(target) || map.GetUnit(target) != null || map.Distance(location, target) > abilityRange)
        {
            yield break;
        }
        else
        {
            Unit unitToRevive = map.GetTileDeadUnit(target);
            if (unitToRevive == null)
            {
                yield break;
            }

            // TODO: probably need to add a dedicated revive function to play a revive animation.
            state.battleManager.SpawnUnit(target, unitToRevive);
            currentCoolDown = coolDown;
        }
        yield return null;
    }
}
