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

    public void DoAttack(Unit target)
    {
        target.ChangeHealth(currentAttackDamage * -1);
        attackSound.Play();
    }

    public bool DoMovement(Vector3Int target)
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
