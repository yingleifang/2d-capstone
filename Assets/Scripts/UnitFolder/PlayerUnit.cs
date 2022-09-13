using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{

    public enum UnitType {RACOON, RAM, LIZARD};

    private bool selected;
    public UnitType type;
    public bool hasMoved = false;
    public bool hasAttacked = false;

    public override bool DoMovement(Vector3Int target)
    {
        hasMoved = true;
        return base.DoMovement(target);
    }

    public override void DoAttack(Unit target)
    {
        base.DoAttack(target);
        hasAttacked = true;
    }

    public override void StartOfTurn()
    {
        hasMoved = false;
        hasAttacked = false;
    }

    public override bool UseAbility(Vector3Int target)
    {
        if (currentCoolDown > 0)
        {
            return false;
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
        return false;
    }
    
}
