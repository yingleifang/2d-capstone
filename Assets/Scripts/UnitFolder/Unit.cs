using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public abstract class Unit: MonoBehaviour
{
    public int health;
    [ReadOnly] public int currentHealth;

    public int attackDamage;
    [ReadOnly] public int currentAttackDamage;

    public int attackRange;
    [ReadOnly] public int currentAttackRange;

    public int movementSpeed;
    [ReadOnly] public int currentMovementSpeed;

    public int coolDown;
    [ReadOnly] public int currentCoolDown;


    public Vector3Int location;

    public Animator anim;

    public new AudioComponent audio;

    public Sprite portrait;
    public string characterName;
    public GameObject healthBarFill;
    public GameObject healthBarBackground;

    [SerializeField]
    private SoundEffect deathSound, hitSound, attackSound, placementSound, fallSound;
    [SerializeField]
    public SpriteRenderer spriteRenderer;

    public bool isDead = false;

    protected int currentWaypointIndex = 0;
    protected List<Vector3Int> path = null;

    public bool inMovement = false;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        if(!anim)
        {
            audio = GetComponent<AudioComponent>();
        }
        if(!anim)
        {
            anim = GetComponent<Animator>();
        }
    }

    public void Start() 
    {
        currentHealth = health;
        currentAttackDamage = attackDamage;
        currentAttackRange = attackRange;
        currentMovementSpeed = movementSpeed;
        currentCoolDown = coolDown;
        BattleManager.instance.unitsToSpawn.Add(this);
    }

    public IEnumerator PlayAttackAnimation()
    {
        anim.SetTrigger("Attack");
        yield return null; // Wait a frame for the animation to start
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Base_Layer.Attacking"));
    }

    public IEnumerator PlayDeathAnimation()
    {
        anim.SetBool("isDead", true);
        yield return null;
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
    }

    public void StartMovingAnimation()
    {
        anim.SetBool("isMoving", true);
    }

    public void StopMovingAnimation()
    {
        anim.SetBool("isMoving", false);
    }

    public IEnumerator PlayFallingAnimation()
    {
        yield break;
    }

    public IEnumerator PlayDamageAnimation()
    {
        anim.SetTrigger("Damaged");
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Base_Layer.Damaged"));
    }

    public abstract IEnumerator UseAbility(Vector3Int target);

    public void FlipSprite(Vector3 target)
    {
        float sign = Mathf.Sign(target.x - transform.position.x);
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * sign, transform.localScale.y, transform.localScale.z);
    }

    public virtual IEnumerator DoAttack(Unit target)
    {
        FlipSprite(target.transform.position);
        yield return StartCoroutine(PlayAttackAnimation());
        target.ChangeHealth(currentAttackDamage * -1);
        audio.PlaySound(attackSound);
        yield break;
    }

    public virtual IEnumerator StartOfBattleAbility(BattleState state)
    {
        yield break;
    }

    public virtual void StartOfTurn()
    {
        return;
    }

    public virtual void StartOfBattle()
    {
        currentMovementSpeed = movementSpeed;
        currentAttackDamage = attackDamage;
        currentAttackRange = attackRange;
    }

    public virtual IEnumerator DoMovement(BattleState state, Vector3Int target, bool unitBlocks = true)
    {
        if (state.map.GetTile(target) == null)
        {
            yield break;
        }
        if (path == null)
        {
            path = state.map.FindShortestPath(location, target, unitBlocks);
            inMovement = true;
        }
        state.map.RemoveUnitFromTile(location);
        state.map.AddUnitToTile(target, this);
        yield return StartCoroutine(smoothMovement(state, target));

        yield break;
        //TODO Check bounds here. Access map classs to do this.
        //Trigger animations here
    }

    public IEnumerator MoveTowards(BattleState state, Vector3Int targetLocation, int numMove)
    {
        List<Vector3Int> path = state.map.FindShortestPath(location, targetLocation);
        Vector3Int goal;
        int goalIndex;
        if(numMove > path.Count)
        {
            goalIndex = path.Count - 1;
        } else
        {
            goalIndex = numMove - 1;
        }

        if(goalIndex < 0)
        {
            goal = location;
        } else
        {
            goal = path[goalIndex];
        }

        if(goal == targetLocation)
        {
            if(goalIndex - 1 < 0)
            {
                goal = location;
            } else
            {
                goal = path[goalIndex - 1];
            }
        }
        yield return StartCoroutine(DoMovement(state, goal));
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator smoothMovement(BattleState state, Vector3Int target)
    {
        FlipSprite(state.map.CellToWorldPosition(target));
        StartMovingAnimation();
        while (currentWaypointIndex < path.Count)
        {
            var step = movementSpeed * Time.deltaTime * 10;
            Vector3 worldPostion = state.map.CellToWorldPosition(path[currentWaypointIndex]);
            transform.position = Vector3.MoveTowards(transform.position, worldPostion, step);
            if (Vector3.Distance(transform.position, worldPostion) < 0.00001f)
            {
                currentWaypointIndex++;
            }
            yield return null;
        }

        StopMovingAnimation();

        path = null;
        inMovement = false;
        currentWaypointIndex = 0;
        transform.position = state.map.CellToWorldPosition(location);

        if (state.map.IsHazardous(target))
        {
            ChangeHealth(-1);
            audio.PlayDisposable(hitSound);
        }
    }

    public virtual List<Vector3Int> GetTilesInMoveRange()
    {
        // Can override this method for unique move ranges
        return BattleManager.instance.map.GetTilesInRange(location, currentMovementSpeed);
    }

    public virtual bool IsTileInMoveRange(Vector3Int tile)
    {
        return BattleManager.instance.map.RealDistance(location, tile) <= currentMovementSpeed;
    }

    public virtual List<Vector3Int> GetTilesInAttackRange()
    {
        // Can override this method for unique attack ranges
        return BattleManager.instance.map.GetTilesInRange(location, currentAttackRange, false);
    }

    public virtual bool IsTileInAttackRange(Vector3Int tile)
    {
        return BattleManager.instance.map.RealDistance(location, tile, false) <= currentAttackRange;
    }

    public virtual List<Vector3Int> GetTilesInThreatRange()
    {
        // Can override this method for unique threat ranges
        return BattleManager.instance.map.GetTilesInRange(location, currentMovementSpeed + currentAttackRange, false);
    }

    public IEnumerator SetLocation(BattleState state, Vector3Int target)
    {
        if (state.map.GetTile(target) == null)
        {
            yield break;
        }

        location = target;
        
        transform.position = state.map.CellToWorldPosition(target);

        anim.SetBool("Hide", false);
        anim.SetTrigger("Appear");
    }

    public void PlayPlacementSound()
    {
        audio.PlayDisposable(placementSound);
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        if (amount < 0)
        {
            audio.PlayDisposable(hitSound);
            StartCoroutine(PlayDamageAnimation());
        }
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
        }
        healthBarFill.GetComponent<Image>().fillAmount = (float) currentHealth / (float) health;
    }

    private void OnMouseEnter()
    {
        healthBarBackground.SetActive(true);
        healthBarFill.SetActive(true);
    }

    private void OnMouseExit()
    {
        healthBarBackground.SetActive(false);
        healthBarFill.SetActive(false);
    }

    public IEnumerator Die() 
    {
        audio.PlayDisposable(deathSound);
        yield return StartCoroutine(PlayDeathAnimation());
        Destroy(gameObject);
        yield break;
    }
    
}
