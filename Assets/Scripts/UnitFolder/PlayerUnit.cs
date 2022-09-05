using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{

    enum Type {Racoon, Ram, Lizard};

    private bool selected;
    private Type type;

    public override bool useAbility(Vector3Int target)
    {
        if (currentCoolDown > 0)
        {
            return false;
        }
        switch(type)
        {
            case Type.Racoon:
                //TODO racoon ability
                break;
            case Type.Ram:
                //TODO access unit on coordinates
                break;
            case Type.Lizard:
                //TODO damage to all units in line from lizard and 
                break;

        }
        return false;
    }
    
}
