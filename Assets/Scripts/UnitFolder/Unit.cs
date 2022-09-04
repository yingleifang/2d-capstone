using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit: MonoBehaviour
{
    public int health;
    public int attackDamage;
    public int attackRange;
    public int movementSpeed;
    public int coolDown;
    public int[] location;

    //TODO have public Class name here associated with map

    public abstract bool useAbility(int i, int j);

    public void doAttack(Unit target)
    {
        target.takeDamage(attackDamage);
    }

    public void doMovement(int i, int j)
    {
        location[0] += i;
        location[1] += j;

        //TODO Check bounds here. Access map classs to do this.
        //Trigger animations here
    }

    public void setLocation(int i, int j)
    {
        location[0] += i;
        location[1] += j;

        //TODO Check bounds here. Access map classs to do this.        
    }

    public void takeDamage(int damage)
    {
        health -= damage;
        if (health < 0)
        {
            health = 0;
        }
    }
}
