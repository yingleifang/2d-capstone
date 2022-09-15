using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{

    public enum UnitType {RACOON, RAM, LIZARD};

    public UnitType type;
    public bool hasMoved = false;
    public bool hasAttacked = false;

    public override IEnumerator DoMovement(BattleState state, Vector3Int target)
    {
        hasMoved = true;
        return base.DoMovement(state, target);
    }

    public override IEnumerator DoAttack(Unit target)
    {
        hasAttacked = true;
        return base.DoAttack(target);
    }

    public override void StartOfTurn()
    {
        hasMoved = false;
        hasAttacked = false;
    }

    public override IEnumerator UseAbility(Vector3Int target)
    {
        if (currentCoolDown > 0)
        {
            yield break;
        }
        switch(type)
        {
            case UnitType.RACOON:
                //TODO racoon ability
                break;
            case UnitType.RAM:
                //TODO access unit on coordinates
                break;
            case UnitType.LIZARD:
                //TODO damage to all units in line from lizard and 
                break;

        }
        hasAttacked = true;
        currentCoolDown = coolDown;
        yield break;
    }
    
}
