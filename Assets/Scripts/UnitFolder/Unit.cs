using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Unit: MonoBehaviour
{
    [HideInInspector]
    public int currentHealth, currentAttackDamage, currentAttackRange,
            currentMovementSpeed, currentCoolDown;

    public int health, attackDamage, attackRange, movementSpeed, coolDown;

    public Vector3Int location;
    public Tilemap map;

    public TileManager tileManager;

    private void Awake() 
    {
        currentHealth = health;
        currentAttackDamage = attackDamage;
        currentAttackRange = attackRange;
        currentMovementSpeed = movementSpeed;
        currentCoolDown = coolDown;
        transform.position = map.CellToWorld(location);
        tileManager.AddUnit(location, this);
    }


    //TODO have public Class name here associated with map

    public abstract bool useAbility(Vector3Int target);

    public void doAttack(Unit target)
    {
        target.changeHealth(currentAttackDamage * -1);
    }

    public void doMovement(Vector3Int target)
    {

        //TODO Check bounds here. Access map classs to do this.
        //Trigger animations here
    }

    public void setLocation(Vector3Int target)
    {

        //TODO Check bounds here. Access map classs to do this.        
    }

    public void changeHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
    }
}
