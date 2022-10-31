using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locke : PlayerUnit
{
    public SoundEffect StartOfBattleAbilitySound;
    public bool canUseAbility;
    public int abilityDamage;

    protected override void Awake()
    {
        base.Awake();
        prefab = unitPrefabSO.GetPrefab("Locke");
    }

    public override IEnumerator StartOfBattleAbility(BattleState state)
    {
        Debug.Log("Gathering rocks");
        yield return StartCoroutine(PlayEOBAnim());
        canUseAbility = true;
        yield break;
    }

    public override IEnumerator UseAbility(Vector3Int target, BattleState state)
    {
        if (location == target)
        {
            yield break;
        }

        Unit targetUnit = state.tileManager.GetUnit(target);
        if (!targetUnit || !canUseAbility || state.tileManager.FindShortestPath(location, target).Count > abilityRange)
        {
            yield break;
        }
        else
        {
            currentCoolDown = coolDown;
            targetUnit.ChangeHealth(abilityDamage * -1);
        }
        yield return null;
    }
}
