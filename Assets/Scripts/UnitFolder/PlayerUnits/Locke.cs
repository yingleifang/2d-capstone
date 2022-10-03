using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locke : PlayerUnit
{
    public SoundEffect StartOfBattleAbilitySound;
    public bool canUseAbility;
    public int abilityDamage;

    public override IEnumerator StartOfBattleAbility(BattleState state)
    {
        Debug.Log("Gathering rocks");
        audio.PlaySound(StartOfBattleAbilitySound);
        canUseAbility = true;
        yield break;
    }

    public override IEnumerator UseAbility(Vector3Int target, BattleState state)
    {
        Unit targetUnit = state.tileManager.GetUnit(target);
        if (!targetUnit || !canUseAbility || state.tileManager.FindShortestPath(location, target).Count > abilityRange)
        {
            yield break;
        }
        else
        {
            targetUnit.ChangeHealth(abilityDamage * -1);
        }
        yield return null;
    }
}
