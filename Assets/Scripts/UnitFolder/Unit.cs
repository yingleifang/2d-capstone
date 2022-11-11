using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;


public abstract class Unit: MonoBehaviour
{
    [HideInInspector]
    public Unit prefab;
    public UnitPrefabSO unitPrefabSO;

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

    public int abilityRange;

    public Vector3Int location;

    public Animator anim;

    public new AudioComponent audio;

    public SpriteRenderer spriteRenderer;
    public Sprite portrait;
    public string characterName;
    [TextArea(10,10)]
    public string characterDescription;
    public GameObject healthBarFill;
    public GameObject healthBarBackground;
    public DamageNumber damageNumberPrefab;
    public Transform damageNumberSpawnPoint;

    [SerializeField]
    protected SoundEffect deathSound, hitSound, attackSound, placementSound, fallSound;
    [SerializeField]

    public bool spawnOnStart = false;
    

    public bool isDead = false;

    //For pathfinding
    protected int currentWaypointIndex = 0;
    protected List<Vector3Int> path = null;
    public bool inMovement = false;
    public LevelManager levelManager;

    private int spriteSign = 1;

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

        spriteSign = MathF.Sign(spriteRenderer.transform.localScale.x);
    }

    public virtual void Start() 
    {
        currentHealth = health;
        currentAttackDamage = attackDamage;
        currentAttackRange = attackRange;
        currentMovementSpeed = movementSpeed;
        currentCoolDown = 0;
        if (LevelManager.instance.isTutorial || spawnOnStart)
        {
            BattleManager.instance.unitsToSpawn.Add(this);
        }
    }

    public void PlaySoundAnim(AnimationEvent sound)
    {
        audio.PlaySoundAnim(sound);
    }

    public IEnumerator PlayAttackAnimation()
    {
        anim.SetTrigger("Attack");
        yield return null; // Wait a frame for the animation to start
        int state = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).fullPathHash == state && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
    }

    public IEnumerator PlayDeathAnimation()
    {
        anim.SetBool("isDead", true);
        yield return null; // Wait a frame for the animation to start
        int state = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).fullPathHash == state && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
    }

    public IEnumerator PlayFallAnimation()
    {
        anim.SetTrigger("isFalling");
        yield return null; // Wait a frame for the animation to start
        int state = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).fullPathHash == state && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
    }

    public IEnumerator PlayLastWordAnimation()
    {
        anim.SetTrigger("LastWord");
        yield return null; // Wait a frame for the animation to start
        int state = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).fullPathHash == state && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
    }

    public IEnumerator PlayAbilityAnimation()
    {
        anim.SetTrigger("Ability");
        yield return null; // Wait a frame for the animation to start
        int state = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).fullPathHash == state && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
    }

    public void StartMovingAnimation()
    {
        anim.SetBool("isMoving", true);
    }

    public void StopMovingAnimation()
    {
        anim.SetBool("isMoving", false);
    }

    public IEnumerator PlayAppearAnimation(bool inBattle)
    {
        anim.SetBool("Hide", false);
        if (inBattle)
        {
            anim.SetTrigger("AppearInBattle");
        }
        else
        {
            anim.SetTrigger("Appear");
        }
        yield return null; // Wait a frame for the animation to start
        int state = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).fullPathHash == state && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
    }

    public void Show()
    {
        anim.SetBool("Hide", false);
    }

    public void Hide()
    {
        if (anim == null)
        {
            return;
        }
        anim.SetBool("Hide", true);
    }

    public IEnumerator PlayDamageAnimation()
    {
        anim.SetTrigger("Damaged");
        yield return null; // Wait a frame for the animation to start
        int state = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).fullPathHash == state && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
    }

    /// <summary>
    /// Dims the unit.
    /// </summary>
    /// <returns>a coroutine representing the dimming animation</returns>
    public IEnumerator Dim()
    {
        // TODO: may want to change the implementation for the dimmin/undimming. Maybe to a tween?
        anim.SetBool("Dim", true);
        yield return null; // Wait a frame for the animation to start
        yield return new WaitUntil(() => anim == null || !anim.IsInTransition(1));
    }

    /// <summary>
    /// Un-dims the unit.
    /// </summary>
    /// <returns>a coroutine representing the un-dimming animation</returns>
    public IEnumerator Undim()
    {
        anim.SetBool("Dim", false);
        yield return null; // Wait a frame for the animation to start
        yield return new WaitUntil(() => !gameObject || !anim.IsInTransition(1));
    }

    public abstract IEnumerator UseAbility(Vector3Int target, BattleState state);

    /// <summary>
    /// Flips the sprite across the y axis to face the target coordinate
    /// </summary> 
    public void FlipSprite(Vector3 target)
    {
        float sign = Mathf.Sign(target.x - transform.position.x);
        spriteRenderer.transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * sign * spriteSign, transform.localScale.y, transform.localScale.z);
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

    //Player unit function. Overridden in PlayerUnit.cs
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
        if (state.tileManager.GetTile(target) == null)
        {
            yield break;
        }
        if (path == null)
        {
            path = state.tileManager.FindShortestPath(location, target, unitBlocks);
            inMovement = true;
        }
        state.tileManager.RemoveUnitFromTile(location);

        yield return StartCoroutine(smoothMovement(state, target));

        state.tileManager.AddUnitToTile(target, this);

        yield break;
    }

    public IEnumerator MoveTowards(BattleState state, Vector3Int targetLocation, int numMove, bool stopBeforeTarget = true)
    {
        Debug.Log("target: " + targetLocation);
        List<Vector3Int> path = state.tileManager.FindShortestPath(location, targetLocation);
        TileBase startTile = BattleManager.instance.tileManager.map.GetTile(location);

        // If we have a path count of one, and the target destination is occupied, then we will not move at all.
        // However, unit still needs to take damage from hazards
        if (path.Count == 1 && BattleManager.instance.tileManager.dynamicTileDatas[targetLocation].unit && 
                    BattleManager.instance.tileManager.baseTileDatas[startTile].hazardous)
        {
            ChangeHealth(-1);
            yield return new WaitForSeconds(0.4f);
            yield break;
        }

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

        if(stopBeforeTarget && goal == targetLocation)
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
        Debug.Log("location: " + location + " target: " + target);
        if (!target.Equals(location))
        {
            FlipSprite(state.tileManager.CellToWorldPosition(target));
            StartMovingAnimation();
            while (currentWaypointIndex < path.Count)
            {
                var step = movementSpeed * Time.deltaTime * 10;
                Vector3 worldPostion = state.tileManager.CellToWorldPosition(path[currentWaypointIndex]);
                transform.position = Vector3.MoveTowards(transform.position, worldPostion, step);
                if (Vector3.Distance(transform.position, worldPostion) < 0.00001f)
                {
                    currentWaypointIndex++;
                }
                yield return null;
            }

            StopMovingAnimation();
        }

        path = null;
        inMovement = false;
        currentWaypointIndex = 0;
        transform.position = state.tileManager.CellToWorldPosition(target);

        if (state.tileManager.IsHazardous(target))
        {
            ChangeHealth(-1);
            audio.PlayDisposable(hitSound);
            yield return new WaitForSeconds(.5f);
        }
    }

    public virtual List<Vector3Int> GetTilesInMoveRange()
    {
        // Can override this method for unique move ranges
        return BattleManager.instance.tileManager.GetTilesInRange(location, currentMovementSpeed);
    }

    public virtual bool IsTileInMoveRange(Vector3Int tile)
    {
        return BattleManager.instance.tileManager.RealDistance(location, tile) <= currentMovementSpeed;
    }

    public virtual List<Vector3Int> GetTilesInAttackRange()
    {
        // Can override this method for unique attack ranges
        return BattleManager.instance.tileManager.GetTilesInRange(location, currentAttackRange, false);
    }

    public virtual bool IsTileInAttackRange(Vector3Int tile)
    {
        return CanAttackTileFromTile(tile, location);
    }

    /// <summary>
    /// Returns true if this unit could attack the target tile from
    /// position
    /// </summary>
    /// <param name="target">the tile to attack</param>
    /// <param name="position">the tile the unit is attacking from</param>
    /// <returns></returns>
    public virtual bool CanAttackTileFromTile(Vector3Int target, Vector3Int position)
    {
        return BattleManager.instance.tileManager.RealDistance(position, target, false) <= currentAttackRange;
    }

    public virtual List<Vector3Int> GetTilesInThreatRange()
    {
        // Can override this method for unique threat ranges
        return BattleManager.instance.tileManager.GetTilesInRange(location, currentMovementSpeed + currentAttackRange, false);
    }

    /// <summary>
    /// Returns true if the given tile is within the unit's threat range.
    /// A unit's threat range is any tile a unit can attack after moving.
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public virtual bool IsTileInThreatRange(Vector3Int tile, out Vector3Int position)
    {
        List<Vector3Int> path = BattleManager.instance.tileManager.FindShortestPath(location, tile);
        if (IsTileInAttackRange(tile))
        {
            position = location;
            return true;
        }

        for (int i = 0; i < path.Count && i < currentMovementSpeed; i++)
        {
            if (CanAttackTileFromTile(tile, path[i]))
            {
                position = path[i];
                return true;
            }
        }

        position = location;
        return false;
    }

    /// <summary>
    /// Sets the unit's position to the given location and plays the appearing animation
    /// </summary>
    /// <param name="state">current state of the battle</param>
    /// <param name="target">location to spawn at</param>
    /// <returns></returns>
    public IEnumerator AppearAt(BattleState state, Vector3Int target, bool inBattle = false)
    {
        if (state.tileManager.GetTile(target) == null)
        {
            yield break;
        }

        SetLocation(state, target);

        yield return StartCoroutine(PlayAppearAnimation(inBattle));
    }

    public void SetLocation(BattleState state, Vector3Int target)
    {
        location = target;
        transform.position = state.tileManager.CellToWorldPosition(target);
    }

    public IEnumerator BounceTo(BattleState state, Vector3Int target, float duration)
    {
        Vector3 destination = state.tileManager.CellToWorldPosition(target);
        LeanTween.move(gameObject, destination, duration);
        yield return new WaitForSeconds(duration);
    }

    public void PlayPlacementSound()
    {
        audio.PlayDisposable(placementSound);
    }

    /// <summary>
    /// Spawns a damage number prefab at damageNumberSpawnPoint
    /// with the given parameters.
    /// </summary>
    /// <param name="text">the text to display</param>
    /// <param name="color">the color of the pop up</param>
    public void SpawnDamageNumber(string text, Color color)
    {
        if(damageNumberPrefab)
        {
            DamageNumber instance = Instantiate(damageNumberPrefab, damageNumberSpawnPoint.position, Quaternion.identity);
            instance.Initialize(text, color);
        }
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        if (amount < 0)
        {
            audio.PlayDisposable(hitSound);
            SpawnDamageNumber(Mathf.Abs(amount).ToString(), Color.white);
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

    public virtual IEnumerator Die() 
    {
        audio.PlayDisposable(deathSound);
        yield return StartCoroutine(PlayLastWordAnimation());
        yield return StartCoroutine(PlayDeathAnimation());
        Destroy(gameObject);
        yield break;
    }

    public IEnumerator Fall()
    {
        yield return new WaitForSeconds(0.6f);
        spriteRenderer.sortingOrder = -3;
        audio.PlayDisposable(deathSound);
        yield return StartCoroutine(PlayFallAnimation());
        spriteRenderer.sortingOrder = 0;
    }

    public void decreaseCoolDown(int numDecrease = 1)
    {
        if (currentCoolDown == 0)
        {
            return;
        }
        else
        {
            currentCoolDown -= numDecrease;
        }
    }
}
