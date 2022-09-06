using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{

    enum Type {RACOON, RAM, LIZARD};

    private bool selected;
    private Type type;

    public override bool UseAbility(Vector3Int target)
    {
        if (currentCoolDown > 0)
        {
            return false;
        }
        switch(type)
        {
            case Type.RACOON:
                //TODO racoon ability
                break;
            case Type.RAM:
                //TODO access unit on coordinates
                break;
            case Type.LIZARD:
                //TODO damage to all units in line from lizard and 
                break;

        }
        return false;
    }
    
}
