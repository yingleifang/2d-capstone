using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class Locke : PlayerUnit
{
    public SoundEffect StartOfBattleAbilitySound;
    public bool canUseAbility;
    public int abilityDamage;
    public int currentAbilityDamage;

    private TileManager tileManager;

    public int distanceMoved = 0;

    protected override void Awake()
    {
        base.Awake();
        prefab = unitPrefabSO.GetPrefab("Locke");
        currentAbilityDamage = abilityDamage;
        inBattleAbilityDescription += "Current Damage: " + currentAbilityDamage;
    }

    public override IEnumerator StartOfBattleAbility(BattleState state)
    {
        Debug.Log("Gathering rocks");
        var position = transform.position;
        tileManager = FindObjectOfType<TileManager>();
        Debug.Log(position);
        var cellPos = tileManager.map.WorldToCell(position);
        Debug.Log(cellPos);
        yield return StartCoroutine(PlayEOBAnim());

        if (cellPos != null)
        {
            Debug.Log(LevelManager.instance);
            TileBase startTile = tileManager.map.GetTile(location);
            TileDataScriptableObject tile = tileManager.baseTileDatas[startTile];
            if (tile.bonusStatModifier == 0)
            {
                yield return StartCoroutine(SpawnStatNumber("<sprite=\"heart\" name=\"heart\">", 1, Color.green));
                health += 1;
                currentHealth += 1;
            }
            else if (tile.bonusStatModifier == 1)
            {
                yield return StartCoroutine(SpawnStatNumber("<sprite=\"fast\" name=\"fast\">", 1, Color.green));
                movementSpeed += 1;
                currentMovementSpeed += 1;
            }
            else if (tile.bonusStatModifier == 2)
            {
                yield return StartCoroutine(SpawnStatNumber("<sprite=\"sword\" name=\"sword\">", 1, Color.green));
                attackDamage += 1;
                currentAttackDamage += 1;
            }
        }
        canUseAbility = true;
        yield break;
    }

    public override IEnumerator DoAttack(Unit target)
    {
        distanceMoved = 0;
        currentAbilityDamage = abilityDamage;
        inBattleAbilityDescription = inBattleAbilityDescription.Remove(inBattleAbilityDescription.Length - 1, 1) + currentAbilityDamage;
        return base.DoAttack(target);
    }

    public override IEnumerator DoMovement(BattleState state, Vector3Int target, bool unitBlocks = true)
    {
        postProcessingSettings.CanAttackGlow(this);
        hasMoved = true;
        if (state.tileManager.GetTile(target) == null)
        {
            yield break;
        }
        if (path == null)
        {
            path = state.tileManager.FindShortestPath(location, target, unitBlocks);
            currentAbilityDamage = path.Count + abilityDamage;
            inBattleAbilityDescription = inBattleAbilityDescription.Remove(inBattleAbilityDescription.Length - 1, 1) + currentAbilityDamage;
            inMovement = true;
        }
        state.tileManager.RemoveUnitFromTile(location);

        yield return StartCoroutine(smoothMovement(state, target));

        state.tileManager.AddUnitToTile(target, this);

        yield break;
    }

    public override IEnumerator UseAbility(Vector3Int target, BattleState state)
    {
        if (location == target)
        {
            yield break;
        }

        Unit targetUnit = state.tileManager.GetUnit(target);
        if (!targetUnit || !canUseAbility || state.tileManager.FindShortestPath(location, target).Count > abilityRange)
        {
            yield break;
        }
        else
        {
            FlipSprite(targetUnit.transform.position);
            yield return StartCoroutine(PlayAbilityAnimation());
            currentCoolDown = coolDown;
            targetUnit.ChangeHealth(currentAbilityDamage * -1);
        }
        yield return null;
    }
}
