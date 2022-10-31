using SpriteGlow;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class PlayerUnit : Unit
{

    public enum UnitType {RACOON, RAM, LIZARD};

    [TextArea]
    public string startOfBattleAbilityDescription;
    [TextArea]
    public string inBattleAbilityDescription;
    public VideoClip previewClip;

    public UnitType type;
    public bool hasMoved = false;
    public bool hasAttacked = false;

    private PostProcessingSettings postProcessingSettings;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        if (!anim)
        {
            audio = GetComponent<AudioComponent>();
        }
        if (!anim)
        {
            anim = GetComponent<Animator>();
        }
        postProcessingSettings = FindObjectOfType<PostProcessingSettings>();
    }

    public IEnumerator PlayEOBAnim()
    {
        anim.Play("EOB Ability", 0, 0f);
        yield return new WaitForSeconds(.1f); // Wait a frame for the animation to start
        int state = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).fullPathHash == state && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
    }

    public override IEnumerator DoMovement(BattleState state, Vector3Int target, bool unitBlocks = true)
    {
        postProcessingSettings.CanAttackGlow(this);
        hasMoved = true;
        yield return base.DoMovement(state, target);
    }

    public override IEnumerator DoAttack(Unit target)
    {
        postProcessingSettings.DisableGlow(this);
        hasAttacked = true;
        yield return StartCoroutine(base.DoAttack(target));
        yield return StartCoroutine(Dim());
    }

    public override void StartOfTurn()
    {
        hasMoved = false;
        hasAttacked = false;
        StartCoroutine(Undim());
    }

    public override IEnumerator UseAbility(Vector3Int target, BattleState state)
    {
        if (currentCoolDown > 0)
        {
            yield break;
        }
    }

    /// <summary>
    /// Returns the tiles within range for the unit's ability.
    /// Defaults to returning all tiles within abilityRange tiles of the unit's location.
    /// </summary>
    /// <returns>a list of tiles within ability range for the unit</returns>
    public virtual List<Vector3Int> GetTilesInAbilityRange()
    {
        return BattleManager.instance.tileManager.GetTilesInRange(location, abilityRange, false);
    }

    /// <summary>
    /// Determines if the given tile is in range of the unit's ability
    /// </summary>
    /// <param name="tilePos">the tile to check</param>
    /// <returns>true if the tile is in the unit's ability range, false otherwise</returns>
    public virtual bool IsTileInAbilityRange(Vector3Int tilePos)
    {
        return GetTilesInAbilityRange().Contains(tilePos);
    }

    public bool UnitsToAttackInRange(List<EnemyUnit> enemyUnits)
    {
        List<Vector3Int> tiles = GetTilesInAttackRange();
        foreach (var tile in tiles)
        {
            foreach (var enemy in enemyUnits)
            {
                if (enemy.location.Equals(tile))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
}
