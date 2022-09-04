using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    enum Type {Fish};

    Type type;

    EnemyUnit(Type type, int health, int attackDamage, int attackRange, int movementSpeed, 
                int coolDown, int[] location)
    {
        this.type = type;
        this.health = health;
        this.attackDamage = attackDamage;
        this.attackRange = attackRange;
        this.movementSpeed = movementSpeed;
        this.coolDown = coolDown;
        this.location = location;
    }

    public override bool useAbility(int i, int j)
    {
        return false;
    }

}
