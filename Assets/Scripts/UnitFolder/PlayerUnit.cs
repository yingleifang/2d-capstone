using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{

    enum UnitType {RACOON, RAM, LIZARD};

    private bool selected;
    private UnitType type;

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
