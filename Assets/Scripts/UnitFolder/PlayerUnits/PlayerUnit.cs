using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{

    public enum UnitType {RACOON, RAM, LIZARD};

    public UnitType type;
    public bool hasMoved = false;
    public bool hasActed = false;

    public override IEnumerator DoMovement(BattleState state, Vector3Int target, bool unitBlocks = true)
    {
        hasMoved = true;
        return base.DoMovement(state, target);
    }

    public override IEnumerator DoAttack(Unit target)
    {
        hasActed = true;
        yield return StartCoroutine(base.DoAttack(target));
        yield return StartCoroutine(Dim());
    }

    public override void StartOfTurn()
    {
        hasMoved = false;
        hasActed = false;
        StartCoroutine(Undim());
    }

    public override IEnumerator UseAbility(Vector3Int target, BattleState state)
    {
        if (currentCoolDown > 0)
        {
            yield break;
        }
    }
    
}
