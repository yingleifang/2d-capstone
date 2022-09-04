using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{

    enum Type {Racoon, Ram, Lizard};

    private bool selected;
    private Type type;

    PlayerUnit(Type type, int health, int attackDamage, int attackRange, int movementSpeed, 
                int coolDown, int[] location)
    {
        this.type = type;
        this.health = health;
        this.attackDamage = attackDamage;
        this.attackRange = attackRange;
        this.movementSpeed = movementSpeed;
        this.coolDown = coolDown;
        this.location = location;
        selected = false;
    }

    public override bool useAbility(int i, int j)
    {
        if (coolDown > 0)
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
