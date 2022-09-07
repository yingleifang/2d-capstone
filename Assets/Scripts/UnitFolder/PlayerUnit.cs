using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{

    enum UnitType {RACOON, RAM, LIZARD};

    private bool selected;
    private UnitType type;
    public bool hasMoved = false;
    public bool hasAttacked = false;

    public override void DoAttack(Unit target)
    {
        base.DoAttack(target);
        hasAttacked = true;
    }

    public override bool DoMovement(Vector3Int target)
    {
        bool success = base.DoMovement(target);
        if(success)
        {
            hasMoved = true;
        }
        return success;
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
