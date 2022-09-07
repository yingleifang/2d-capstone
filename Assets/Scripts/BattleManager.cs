using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class BattleState
{
    public List<PlayerUnit> playerUnits;
    public List<EnemyUnit> enemyUnits;
    public HexGrid hexGrid;
    [HideInInspector] public BattleManager battleManager;
}

public class BattleManager : MonoBehaviour
{
    [SerializeField]
    public BattleState state;
    public bool isBattleOver;
    public bool isPlayerTurnOver;
    public PlayerUnit selectedPlayerUnit;
    public UIController ui;

    // Start is called before the first frame update
    void Start()
    {
        state.battleManager = this;
    }

    public void onPlayerEndTurn()
    {
        Debug.Log("Player ended turn");
        StartCoroutine(performEnemyMoves());
    }

    public void SpawnUnit(Unit unit)
    {
        if (unit is PlayerUnit)
        {
            state.playerUnits.Add((PlayerUnit)unit);
        }
        else if (unit is EnemyUnit)
        {
            state.enemyUnits.Add((EnemyUnit)unit);
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
            state.playerUnits.Remove((PlayerUnit)unit);
        }
        else if (unit is EnemyUnit)
        {
            state.enemyUnits.Remove((EnemyUnit)unit);
        }
        else
        {
            Debug.Log("Removing a null unit");
        }
        Debug.Log(state.enemyUnits.Count);

        if (state.playerUnits.Count <= 0)
        {
            StartCoroutine(ui.SwitchScene("GameOverScreen"));
        }
        if (state.enemyUnits.Count <= 0)
        {
            StartCoroutine(ui.SwitchScene());
        }
        Debug.Log(state.enemyUnits.Count);
    }

    

    public void highlightPlayerRange(PlayerUnit unit)
    {
        Debug.Log("Highlighting player range");

        /*List<HexCell> cells;
        Color highlightColor;
        if(unit.canMove)
        {
            cells = unit.getMoveRange(state);
            highlightColor = Color.blue;
        } else if(unit.canAttack)
        {
            cells = unit.getAttackRange(state);
            highlightColor = Color.red;
        } else
        {
            selectedPlayerUnit = null;
            return;
        }
        
        foreach(HexCell cell in cells) {
            state.hexGrid.ColorCell(cell, highlightColor);
        }*/

        selectedPlayerUnit = unit;
    }

    IEnumerator performEnemyMoves()
    {
        foreach(EnemyUnit enemy in state.enemyUnits)
        {   
            //yield return enemy.performAction(state);
        }

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {

        
    }
}
