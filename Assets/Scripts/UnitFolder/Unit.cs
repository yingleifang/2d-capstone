using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public abstract class Unit: MonoBehaviour
{
    [HideInInspector]
    public int currentHealth, currentAttackDamage, currentAttackRange,
            currentMovementSpeed, currentCoolDown;

    public int health, attackDamage, attackRange, movementSpeed, coolDown;

    public Vector3Int location;
    public Tilemap map;
    public TileManager tileManager;

    [SerializeField]
    private AudioSource deathSound;
    [SerializeField]
    private AudioSource attackSound;
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    public void Start() 
    {
        currentHealth = health;
        currentAttackDamage = attackDamage;
        currentAttackRange = attackRange;
        currentMovementSpeed = movementSpeed;
        currentCoolDown = coolDown;
        transform.position = map.CellToWorld(location);
        tileManager.AddUnit(location, this);
    }

    public abstract bool UseAbility(Vector3Int target);

    public virtual void DoAttack(Unit target)
    {
        target.ChangeHealth(currentAttackDamage * -1);
        attackSound.Play();
    }

    public virtual void StartOfTurn()
    {
        return;
    }

    public virtual bool DoMovement(Vector3Int target)
    {
        if (map.GetTile(target) == null)
        {
            return false;
        }
        tileManager.RemoveUnit(location);
        location = target;
        tileManager.AddUnit(location, this);
        transform.position = map.CellToWorld(location);

        return true;
        //TODO Check bounds here. Access map classs to do this.
        //Trigger animations here
    }

    public virtual List<Vector3Int> GetTilesInMoveRange(TileManager map)
    {
        // Can override this method for unique move ranges
        return map.GetTilesInRange(location, currentMovementSpeed);
    }

    public virtual bool IsTileInMoveRange(Vector3Int tile, TileManager map)
    {
        return map.RealDistance(location, tile) <= currentMovementSpeed;
    }

    public virtual List<Vector3Int> GetTilesInAttackRange(TileManager map)
    {
        // Can override this method for unique attack ranges
        return map.GetTilesInRange(location, currentAttackRange, false);
    }

    public virtual bool IsTileInAttackRange(Vector3Int tile, TileManager map)
    {
        return map.RealDistance(location, tile, false) <= currentAttackRange;
    }

    public virtual List<Vector3Int> GetTilesInThreatRange(TileManager map)
    {
        // Can override this method for unique threat ranges
        return map.GetTilesInRange(location, currentMovementSpeed + currentAttackRange, false);
    }

    public void SetLocation(Vector3Int target)
    {

        //TODO Check bounds here. Access map classs to do this.        
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Die() 
    {
        deathSound.Play();
        spriteRenderer.enabled = false;
        tileManager.RemoveUnit(location);
        Destroy(this.gameObject, 5);
    }
    
}
