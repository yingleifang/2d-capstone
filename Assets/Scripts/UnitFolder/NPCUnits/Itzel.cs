using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Itzel : NPCUnit
{
    private void Awake() 
    {
        currentHealth = 1;
    }
    public override IEnumerator UseAbility(Vector3Int target, BattleState state)
    {
        if (currentCoolDown > 0)
        {
            yield break;
        }
    }

}