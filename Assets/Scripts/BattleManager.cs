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

    [HideInInspector]
    public static BattleManager instance;

    PlayerUnit playerUnit;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    }

    public void EndTurn()
    {
        instance.onPlayerEndTurn();
    }

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
            StartCoroutine(ShowGameOver());
        }
        if (enemyUnits.Count <= 0)
        {
            isBattleOver = true;
            ui.LoadUnitSelection();
        }
        Debug.Log(enemyUnits.Count);
    }

    IEnumerator ShowGameOver()
    {
        foreach (EnemyUnit unit in enemyUnits.ToArray())
        {
            Destroy(unit.gameObject);
        }
        yield return StartCoroutine(ui.SwitchScene("GameOverScreen"));
        Destroy(gameObject); // Or similarly reset the battle manager
    }

    IEnumerator NextLevel()
    {
        List<Unit> unitsToSpawn = new List<Unit>();
        unitsToSpawn.AddRange(enemyUnits);
        unitsToSpawn.AddRange(playerUnits);

        yield return StartCoroutine(ui.SwitchScene());
        Debug.Log("Next level finished loading");
        
        yield return null; // Wait a frame for Start() methods. May not be needed

        // Should refactor code so we don't need to find the tileManager. Should be returned by the level changing function
        map = FindObjectOfType<TileManager>();
        // Should also probably be refactored. Kind of clunky and annoying
        state.map = map;

        foreach (Unit unit in unitsToSpawn.ToArray())
        {
            Vector3Int spawnLocation = unit.location;
            Debug.Log(spawnLocation);
            Unit mapUnit = map.GetUnit(spawnLocation);
            if(mapUnit)
            {
                mapUnit.ChangeHealth(-1);
                if(!map.FindClosestFreeTile(spawnLocation, out spawnLocation))
                {
                    // No empty space on map for falling unit
                    KillUnit(unit);
                    Destroy(unit.gameObject);
                    continue;
                }
                unit.ChangeHealth(-1);
            }

            map.AddUnitToTile(spawnLocation, unit);
            // This is kind of gross but until we refactor the Unit class this is needed
            unit.tileManager = map;
            unit.map = map.map;
        }

        StartOfPlayerTurn();
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
        if (isPlacingUnit)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            ui.selectedPrefab.transform.position = worldPos;
        }
    }

    private void HandlePlacingClicks(Vector3Int tilePos, Unit curUnit)
    {
        if (curUnit == null)
        {
            map.SpawnUnit(tilePos, ui.selectedPrefab.GetComponent<PlayerUnit>());
            isBattleOver = false;
            isPlacingUnit = false;
            StartCoroutine(NextLevel());
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
