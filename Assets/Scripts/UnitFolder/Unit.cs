using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public abstract class Unit: MonoBehaviour
{
    [HideInInspector]
    public int currentHealth, currentAttackDamage, currentAttackRange,
            currentMovementSpeed, currentCoolDown;

    public int health, attackDamage, attackRange, movementSpeed, coolDown;

    public Vector3Int location;
    private Tilemap map;
    private TileManager tileManager;

    [SerializeField]
    private AudioSource deathSound;
    [SerializeField]
    private AudioSource attackSound, damageSound;
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    protected int currentWaypointIndex = 0;
    protected List<Vector3Int> path = null;
    protected BattleManager battleManager;

    public bool inMovement = false;

    public void Start() 
    {
        currentHealth = health;
        currentAttackDamage = attackDamage;
        currentAttackRange = attackRange;
        currentMovementSpeed = movementSpeed;
        currentCoolDown = coolDown;
        tileManager = FindObjectOfType<TileManager>();
        map = FindObjectOfType<Tilemap>();
        transform.position = map.CellToWorld(location);
        tileManager.SpawnUnit(location, this);

        battleManager = FindObjectOfType<BattleManager>();
        battleManager = FindObjectOfType<BattleManager>();
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
        tileManager.RemoveUnitFromTile(location);
        location = target;
        tileManager.AddUnitToTile(location, this);
        
        transform.position = map.CellToWorld(location);

        if (tileManager.IsHazardous(target))
        {
            ChangeHealth(-1);
            damageSound.Play();
        }

        return true;
        //TODO Check bounds here. Access map classs to do this.
        //Trigger animations here
    }

    public virtual bool DoMovementAlongPath(Vector3Int target)
    {
        if (map.GetTile(target) == null)
        {
            return false;
        }
        this.path = battleManager.state.map.FindShortestPath(location, target);
        this.inMovement = true;
        StartCoroutine(smoothMovement(target));

        return true;
        //TODO Check bounds here. Access map classs to do this.
        //Trigger animations here
    }

    IEnumerator smoothMovement(Vector3Int target)
    {

        while (currentWaypointIndex < path.Count)
        {
            Debug.Log(String.Format("currentwayIndex: {0} ; pathCount: {1}", currentWaypointIndex, path.Count));
            var step = movementSpeed * Time.deltaTime * 10;
            Vector3 worldPostion = tileManager.CellToWorldPosition(path[currentWaypointIndex]);
            transform.position = Vector3.MoveTowards(transform.position, worldPostion, step);
            if (Vector3.Distance(transform.position, worldPostion) < 0.00001f)
            {
                currentWaypointIndex++;
            }
            yield return null;
        }
        this.path = null;
        this.inMovement = false;
        location = target;
        transform.position = map.CellToWorld(location);

        if (tileManager.IsHazardous(target))
        {
            ChangeHealth(-1);
            damageSound.Play();
        }
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
        tileManager.KillUnit(location);
        Destroy(this.gameObject, 5);
    }
    
}
