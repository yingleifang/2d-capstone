using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Itzel : NPCUnit
{
    public new void Start() 
    {
        currentHealth = 1;
        currentAttackDamage = attackDamage;
        currentAttackRange = attackRange;
        currentMovementSpeed = movementSpeed;
        currentCoolDown = 0;
        if (LevelManager.instance.isTutorial)
        {
            BattleManager.instance.unitsToSpawn.Add(this);
        }
    }
    public override IEnumerator UseAbility(Vector3Int target, BattleState state)
    {
        if (currentCoolDown > 0)
        {
            yield break;
        }
    }

}