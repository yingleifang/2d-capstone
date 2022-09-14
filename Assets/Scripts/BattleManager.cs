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
    private BattleState state;
    public List<PlayerUnit> playerUnits;
    public List<EnemyUnit> enemyUnits;
    public List<Unit> unitsToSpawn;
    public TileManager map;
    public bool isBattleOver = false;
    public bool isPlayerTurn = true;
    public bool isPlacingUnit = false;
    public bool acceptingInput = true;
    public Unit selectedUnit;
    public UIController ui;
    public PlayerUnit unitToPlace;

    [HideInInspector]
    public static BattleManager instance;

    public void SetUnitToPlace(PlayerUnit prefab)
    {
        isPlacingUnit = true;
        unitToPlace = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
    }

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

        state = new BattleState();
    }

    public void EndTurn()
    {
        instance.OnPlayerEndTurn();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ui.HideSelectionWindow());
        StartCoroutine(InitializeBattle());
    }

    private IEnumerator InitializeBattle()
    {
        // Done to delay coroutine to allow units to add themselves to unitsToSpawn
        yield return new WaitForFixedUpdate();

        // Place units waiting to be spawned on new map
        Debug.Log("Units to spawn: " + unitsToSpawn.Count);
        List<Coroutine> animations = new List<Coroutine>();
        foreach (Unit unit in unitsToSpawn.ToArray())
        {
            animations.Add(StartCoroutine(SpawnUnit(unit.location, unit)));
        }

        foreach(Coroutine anim in animations)
        {
            yield return anim;
        }

        yield return StartCoroutine(UpdateBattleState());

        animations.Clear();

        // Activate any start of battle abilities
        foreach (Unit unit in unitsToSpawn.ToArray())
        {
            animations.Add(StartCoroutine(unit.StartOfBattleAbility(GetState())));
        }

        foreach (Coroutine anim in animations)
        {
            yield return anim;
        }

        yield return StartCoroutine(UpdateBattleState());

        unitsToSpawn.Clear();

        yield return StartCoroutine(StartOfPlayerTurn());
        isBattleOver = false;
        selectedUnit = null;
        acceptingInput = true;
        isPlayerTurn = true;
    }



    public void OnPlayerEndTurn()
    {
        if(isPlayerTurn && acceptingInput)
        {
            isPlayerTurn = false;
            StartCoroutine(performEnemyMoves());
        }
    }

    public IEnumerator SpawnUnit(Vector3Int location, Unit unit, bool addToUnitList = true)
    {
        if(addToUnitList)
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
                yield break;
            }
        }

        Vector3Int spawnLocation = location;
        Debug.Log(spawnLocation);
        Unit mapUnit = map.GetUnit(spawnLocation);
        if (mapUnit)
        {
            // Unit fell on another unit!
            Debug.Log("Falling unit collision!");
            mapUnit.ChangeHealth(-1);
            if (!map.FindClosestFreeTile(spawnLocation, out spawnLocation))
            {
                // No empty space on map for falling unit
                KillUnit(unit);
                Destroy(unit.gameObject);
                yield break;
            }
            unit.ChangeHealth(-1);
        }

        map.AddUnitToTile(spawnLocation, unit);
        unit.SetLocation(GetState(), spawnLocation);

        yield return StartCoroutine(map.OnUnitFallOnTile(GetState(), unit, spawnLocation));
    }

    public IEnumerator KillUnit(Unit unit)
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

        map.KillUnit(unit.location);
        yield return StartCoroutine(unit.Die());

        
    }

    public void CheckIfBattleOver()
    {
        if (playerUnits.Count <= 0)
        {
            isBattleOver = true;
            StartCoroutine(ShowGameOver());
        }
        if (enemyUnits.Count <= 0)
        {
            isBattleOver = true;
            StartCoroutine(ui.ShowSelectionWindow());
        }
        Debug.Log("Enemies left: " + enemyUnits.Count);
    }

    public BattleState GetState()
    {
        state.playerUnits = playerUnits;
        state.enemyUnits = enemyUnits;
        state.map = map;
        state.battleManager = this;
        return state;
    }

    IEnumerator ShowGameOver()
    {
        yield return StartCoroutine(ui.SwitchScene("GameOverScreen"));
        foreach (EnemyUnit unit in enemyUnits.ToArray())
        {
            Destroy(unit.gameObject);
        }
        Destroy(gameObject); // Or similarly reset the battle manager
    }

    IEnumerator NextLevel()
    {
        acceptingInput = false;

        unitsToSpawn.AddRange(enemyUnits);
        unitsToSpawn.AddRange(playerUnits);

        enemyUnits.Clear();
        playerUnits.Clear();

        yield return StartCoroutine(ui.SwitchScene());
        Debug.Log("Next level finished loading");

        yield return null; // Need to wait a frame for the new level to load

        // Should refactor code so we don't need to find the tileManager. Should be returned by the level changing function
        map = FindObjectOfType<TileManager>();
        // This is probably also unnecessary if we structure things better
        ui = FindObjectOfType<UIController>();
        Debug.Log("Found new TileManager: " + map != null);

        // If statement below destroys everything if we reach the win screen
        // Should probably be handled more elegantly
        if (map == null || ui == null)
        {
            Debug.Log("No TileManager or UIController found. Destroying GameManager");
            foreach (Unit unit in unitsToSpawn)
            {
                Destroy(unit.gameObject);
            }

            Destroy(gameObject);
        }

        StartCoroutine(InitializeBattle());
    }

    
    IEnumerator performEnemyMoves()
    {
        EnemyUnit[] enemies = enemyUnits.ToArray();
        foreach(EnemyUnit enemy in enemies)
        {   
            yield return enemy.performAction(GetState());
            yield return StartCoroutine(UpdateBattleState());
            if(isBattleOver)
            {
                yield break;
            }
        }

        yield return StartCoroutine(StartOfPlayerTurn());
        DeselectUnit();
        yield break;
    }

    public IEnumerator UpdateBattleState()
    {
        List<Coroutine> animations = new List<Coroutine>();
        foreach (Unit unit in playerUnits.ToArray())
        {
            if (unit.isDead)
            {
                animations.Add(StartCoroutine(KillUnit(unit)));
            }
        }

        foreach (Unit unit in enemyUnits.ToArray())
        {
            if (unit.isDead)
            {
                animations.Add(StartCoroutine(KillUnit(unit)));
            }
        }

        foreach (Coroutine anim in animations)
        {
            yield return anim;
        }

        CheckIfBattleOver();
    }

    public IEnumerator StartOfPlayerTurn()
    {
        foreach(PlayerUnit unit in playerUnits)
        {
            unit.StartOfTurn();
        }

        isPlayerTurn = true;
        yield break;
    }

    // Update is called once per frame
    void Update()
    {
        if (acceptingInput && Input.GetMouseButtonDown(0))
        {
            Vector3Int tilePos = map.GetTileAtScreenPosition(Input.mousePosition);
            Debug.Log(tilePos);

            Unit curUnit = map.GetUnit(tilePos);
            Debug.Log(curUnit);

            if (!isBattleOver && acceptingInput)
            {
                acceptingInput = false;
                StartCoroutine(HandleBattleClicks(tilePos, curUnit));
            }
            else if (isPlacingUnit)
            {
                StartCoroutine(HandlePlacingClicks(tilePos, curUnit));
            }
        }
        if (isPlacingUnit)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            unitToPlace.transform.position = worldPos;
        }
    }

    private IEnumerator HandlePlacingClicks(Vector3Int tilePos, Unit curUnit)
    {
        if (map.InBounds(tilePos) && curUnit == null)
        {
            Debug.Log("Unit placement location: " + tilePos);
            isPlacingUnit = false;
            unitToPlace.location = tilePos;
            yield return StartCoroutine(SpawnUnit(tilePos, unitToPlace));
            unitsToSpawn.Remove(unitToPlace);
            unitToPlace = null;
            StartCoroutine(NextLevel());
        }
    }

    private IEnumerator HandleBattleClicks(Vector3Int tilePos, Unit curUnit)
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
                map.ClearHighlights();
                yield return StartCoroutine(unit.DoAttack(curUnit));
                if(curUnit.isDead)
                {
                    yield return StartCoroutine(KillUnit(curUnit));
                    yield return StartCoroutine(UpdateBattleState());
                }
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
                map.ClearHighlights();
                yield return StartCoroutine(unit.DoMovement(state, tilePos));
                yield return StartCoroutine(UpdateBattleState());
                if(!isBattleOver && unit && !unit.isDead)
                {
                    ShowUnitAttackRange(unit);
                }
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
        acceptingInput = true;
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
        if(isPlayerTurn)
        {
            map.HighlightPath(unit.GetTilesInMoveRange(map), Color.blue);
        }
    }

    private void ShowUnitAttackRange(Unit unit)
    {
        Debug.Log("Show attack range");
        map.ClearHighlights();
        if(isPlayerTurn)
        {
            map.HighlightPath(unit.GetTilesInAttackRange(map), Color.red);
        }
    }

    private void ShowUnitThreatRange(Unit unit)
    {
        map.ClearHighlights();
        if(isPlayerTurn)
        {
            map.HighlightPath(unit.GetTilesInThreatRange(map), Color.red);
        }
    }
}
