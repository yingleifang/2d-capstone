using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mori : PlayerUnit
{
    public SoundEffect StartOfBattleAbilitySound;
    public int abilityDamage;
    public int startOfBattleHealing = 1;

    protected override void Awake()
    {
        base.Awake();
        prefab = unitPrefabSO.GetPrefab("Mori");
    }

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
                player.ChangeHealth(startOfBattleHealing);
                yield return StartCoroutine(unit.SpawnStatNumber("<sprite=\"heart\" name=\"heart\">", 1, Color.green));
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
        Debug.Log("target: " + target + " location: " + location + " bounds " + map.InBounds(target) + " unit " + map.GetUnit(target) != null + " distance " + map.Distance(location, target));
        if (target == location || !map.InBounds(target) || map.GetUnit(target) != null || map.Distance(location, target) > abilityRange)
        {
            yield break;
        }
        else
        {
            Unit unitToRevive = map.GetTileDeadUnit(target);
            Debug.Log("Unit: " + unitToRevive);
            if (unitToRevive == null)
            {
                Debug.Log("No unit to revive");
                yield break;
            }

            FlipSprite(map.CellToWorldPosition(target));
            yield return StartCoroutine(PlayAbilityAnimation());
            Unit unit = Instantiate(unitToRevive);
            // TODO: probably need to add a dedicated revive function to play a revive animation.
            StartCoroutine(state.battleManager.SpawnUnit(target, unit));
            currentCoolDown = coolDown;
        }
        map.ClearTileDeadUnit(target);
        map.ClearTileDecoration(target);
        yield return null;
    }

    public override List<Vector3Int> GetTilesInAbilityRange()
    {
        TileManager map = BattleManager.instance.tileManager;
        List<Vector3Int> tiles = map.GetTilesInRange(location, abilityRange, false);
        List<Vector3Int> results = new List<Vector3Int>();
        foreach (Vector3Int tile in tiles)
        {
            Unit deadUnit = map.GetTileDeadUnit(tile);
            if (deadUnit != null && deadUnit is PlayerUnit)
            {
                Debug.Log("Unit: " + map.GetTileDeadUnit(tile) + " tile: " + tile);
                results.Add(tile);
            }
        }
        return results;
    }
}
