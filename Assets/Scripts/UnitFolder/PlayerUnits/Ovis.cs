using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ovis : PlayerUnit
{
    public SoundEffect StartOfBattleAbilitySound;
    public int abilityDamage;

    protected override void Awake()
    {
        base.Awake();
        prefab = unitPrefabSO.GetPrefab("Ovis");
    }

    public override IEnumerator StartOfBattleAbility(BattleState state)
    {
        Debug.Log("Attacking adjacent units");
        List<Vector3Int> tiles = state.tileManager.GetTilesInRange(location, 1, false);
        yield return StartCoroutine(PlayEOBAnim());
        foreach (Vector3Int tile in tiles)
        {
            Unit unit = state.tileManager.GetUnit(tile);
            if (unit && unit != this)
            {
                unit.ChangeHealth(-1);
            }
        }
        yield break;
    }

    public override IEnumerator UseAbility(Vector3Int target, BattleState state)
    {
        Unit targetUnit = state.tileManager.GetUnit(target);
        if (location == target)
        {
            yield break;
        }
        if (!targetUnit || state.tileManager.FindShortestPath(location, target).Count > abilityRange)
        {
            yield break;
        }
        else
        {
            FlipSprite(targetUnit.transform.position);
            yield return StartCoroutine(PlayAbilityAnimation());
            CubeCoord direction = state.tileManager.GetDirection(location, target, abilityRange);
            Debug.Log("direction: " + direction);
            CubeCoord cubeDestination = state.tileManager.UnityCellToCube(target) + direction;
            Vector3Int destination = state.tileManager.CubeToUnityCell(cubeDestination);
            Unit destinationUnit = state.tileManager.GetUnit(destination);
            Debug.Log(targetUnit);
            Debug.Log(destinationUnit);
            if (state.tileManager.IsImpassableTile(destination, false))
            {
                targetUnit.ChangeHealth(abilityDamage * -1);
            }
            else if (state.tileManager.GetUnit(destination))
            {
                destinationUnit.ChangeHealth(abilityDamage * -1);
                targetUnit.ChangeHealth(abilityDamage * -1);
            }
            else
            {
                yield return StartCoroutine(targetUnit.DoMovement(state, destination));
            }
            currentCoolDown = coolDown;
        }
        yield return null;
    }
}
