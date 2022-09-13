using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class BattleState
{
    public List<PlayerUnit> playerUnits;
    public List<EnemyUnit> enemyUnits;
    public TileManager map;
    [HideInInspector] public BattleManager battleManager;
}

public class BattleManager : MonoBehaviour
{
    [HideInInspector]
    public BattleState state;
    public List<PlayerUnit> playerUnits;
    public List<EnemyUnit> enemyUnits;
    public TileManager map;
    public bool isBattleOver = false;
    public bool isPlayerTurn = true;
    public bool isPlacingUnit = false;
    public Unit selectedUnit;
    public UIController ui;

    PlayerUnit playerUnit;

    // Start is called before the first frame update
    void Start()
    {
        ui.UnloadUnitSelection();
        state.playerUnits = playerUnits;
        state.enemyUnits = enemyUnits;
        state.map = map;
        state.battleManager = this;

        playerUnit = FindObjectOfType<PlayerUnit>();
    }

    public void onPlayerEndTurn()
    {
        isPlayerTurn = false;
        StartCoroutine(performEnemyMoves());
    }

    public void SpawnUnit(Unit unit)
    {
        if (unit is PlayerUnit)
        {
            playerUnits.Add((PlayerUnit)unit);
        }
        else if (unit is EnemyUnit)
        {
            enemyUnits.Add((EnemyUnit)unit);
        }
        else
        {
            Debug.Log("Adding a null unit");
        }
    }

    public void KillUnit(Unit unit)
    {
        if (unit is PlayerUnit)
        {
            playerUnits.Remove((PlayerUnit)unit);
        }
        else if (unit is EnemyUnit)
        {
            enemyUnits.Remove((EnemyUnit)unit);
        }
        else
        {
            Debug.Log("Removing a null unit");
        }
        Debug.Log(enemyUnits.Count);

        if (playerUnits.Count <= 0)
        {            
            StartCoroutine(ui.SwitchScene("GameOverScreen"));
        }
        if (enemyUnits.Count <= 0)
        {
            isBattleOver = true;
            ui.LoadUnitSelection();
        }
        Debug.Log(enemyUnits.Count);
    }

    
    IEnumerator performEnemyMoves()
    {
        EnemyUnit[] enemies = enemyUnits.ToArray();
        foreach(EnemyUnit enemy in enemies)
        {   
            yield return enemy.performAction(state);
        }

        StartOfPlayerTurn();
        yield break;
    }

    public void StartOfPlayerTurn()
    {
        foreach(PlayerUnit unit in playerUnits)
        {
            unit.StartOfTurn();
        }

        isPlayerTurn = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int tilePos = map.GetTileAtScreenPosition(Input.mousePosition);
            Debug.Log(tilePos);

            Unit curUnit = map.GetUnit(tilePos);
            Debug.Log(curUnit);

            if (!isBattleOver)
            {
                HandleBattleClicks(tilePos, curUnit);
            }
            else if (isPlacingUnit)
            {
                HandlePlacingClicks(tilePos, curUnit);
            }
        }
    }

    private void HandlePlacingClicks(Vector3Int tilePos, Unit curUnit)
    {
        if (curUnit == null)
        {
            map.SpawnUnit(tilePos, ui.selectedPrefab.GetComponent<PlayerUnit>());
            isBattleOver = false;
            isPlacingUnit = false;
            StartCoroutine(ui.SwitchScene());
        }
    }

    private void HandleBattleClicks(Vector3Int tilePos, Unit curUnit)
    {
        if(!map.GetTile(tilePos))
        {
            DeselectUnit();
        } else if (curUnit is PlayerUnit)
        {
            SelectUnit(curUnit);    
        }
        else if (curUnit is EnemyUnit)
        {
            if (isPlayerTurn && selectedUnit is PlayerUnit unit 
                && unit.hasMoved && !unit.hasAttacked && unit.IsTileInAttackRange(tilePos, map))
            {
                unit.DoAttack(curUnit);
                DeselectUnit();
            }
            else
            {
                SelectUnit(curUnit);
            }
        }
        else if (curUnit == null)
        {
            if(isPlayerTurn && selectedUnit is PlayerUnit unit
                && !unit.hasMoved && unit.IsTileInMoveRange(tilePos, map))
            {
                unit.DoMovement(tilePos);
                ShowUnitAttackRange(unit);
            }
                else
            {
                DeselectUnit();
            }
            
        }
        else
        {
            DeselectUnit();
        }
    }

    public void SelectUnit(Unit unit)
    {
        map.ClearHighlights();
        selectedUnit = unit;
        if(unit is PlayerUnit player)
        {
            if (!player.hasMoved)
            {
                ShowUnitMoveRange(unit);
            }
            else if (!player.hasAttacked)
            {
                ShowUnitAttackRange(unit);
            }
        } 
        else if(unit is EnemyUnit)
        {
            ShowUnitThreatRange(unit);
        }
    }

    public void DeselectUnit()
    {
        map.ClearHighlights();
        selectedUnit = null;
    }

    private void ShowUnitMoveRange(Unit unit)
    {
        Debug.Log("Show move range");
        map.ClearHighlights();
        map.HighlightPath(unit.GetTilesInMoveRange(map), Color.blue);
    }

    private void ShowUnitAttackRange(Unit unit)
    {
        Debug.Log("Show attack range");
        map.ClearHighlights();
        map.HighlightPath(unit.GetTilesInAttackRange(map), Color.red);
    }

    private void ShowUnitThreatRange(Unit unit)
    {
        map.ClearHighlights();
        map.HighlightPath(unit.GetTilesInThreatRange(map), Color.red);
    }
}
