using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCUnit : Unit
{
    public override IEnumerator UseAbility(Vector3Int target, BattleState state)
    {
        if (currentCoolDown > 0)
        {
            yield break;
        }
    }
}