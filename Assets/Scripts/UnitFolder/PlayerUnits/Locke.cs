using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locke : PlayerUnit
{
    public SoundEffect StartOfBattleAbilitySound;
    public bool canUseAbility;

    public override IEnumerator StartOfBattleAbility(BattleState state)
    {
        Debug.Log("Gathering rocks");
        audio.PlaySound(StartOfBattleAbilitySound);
        canUseAbility = true;
        yield break;
    }
}
