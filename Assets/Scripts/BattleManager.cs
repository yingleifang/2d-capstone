using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.CanvasScaler;
using UnityEngine.Tilemaps;


[System.Serializable]
public class BattleState : ScriptableObject
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
    public int turnsPerBattle = 5;
    public bool isBattleOver = false;
    public bool isPlayerTurn = true;
    public bool isPlacingUnit = false;
    public bool acceptingInput = true;
    public Unit selectedUnit;
    public UIController ui;
    public PlayerUnit unitToPlace;

    public GameObject previewLayer;
    public bool previewVisible = false;

    [HideInInspector]
    public static BattleManager instance;

    public LevelManager levelManager;

    public Vector3 mapPosition;
    public void SetUnitToPlace(PlayerUnit prefab)
    {
        isPlacingUnit = true;
        unitToPlace = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        unitToPlace.anim.SetBool("Hide", false); // Replace with method
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }

        state = new BattleState();
        mapPosition = map.transform.position;
    }

    public void EndTurn()
    {
        instance.OnPlayerEndTurn();
    }

    public void TogglePreview()
    {
        if (instance.previewVisible)
        {
            instance.TurnOffPreview();
        }
        else
        {
            instance.TurnOnPreview();
        }
    }

    public void TurnOnPreview()
    {
        if(previewLayer)
        {
            previewLayer.SetActive(true);
            previewVisible = true;
        } else
        {
            previewVisible = false;
        }
    }

    public void TurnOffPreview()
    {
        Debug.Log("????????????");
        previewVisible = false;
        if(previewLayer)
        {
            previewLayer.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!levelManager.isTutorial)
        {
            Debug.Log("SETTING DATA");
            setEnemyData();
        }
        StartCoroutine(ui.HideSelectionWindow());
        StartCoroutine(InitializeBattle());
        ui.HideUnitInfoWindow();
        //Save();
        //Load(SceneManager.GetActiveScene().buildIndex + 1);
        regeneratePreviews();
    }

    private void regeneratePreviews()
    {
        previewLayer = GameObject.Find("PreviewLayer");
        var curGeneratePreviews = previewLayer.GetComponent<generatePreviews>();
        curGeneratePreviews.transform.position = mapPosition;
        curGeneratePreviews.ShowEnemyPreview(levelManager.nextSceneEnemyInfo, GetState());
        curGeneratePreviews.ShowHazzardPreview(levelManager.nextSceneTileInfo, GetState());
        TurnOffPreview();
    }

    // Update is called once per frame
    void Update()
    {
        if (acceptingInput && Input.GetMouseButtonDown(0))
        {
            Vector3Int tilePos = map.GetTileAtScreenPosition(Input.mousePosition);
            Debug.Log(tilePos);

            Unit curUnit = map.GetUnit(tilePos);

            if (isPlacingUnit)
            {
                StartCoroutine(HandlePlacingClicks(tilePos, curUnit));
            }
            else if (!isBattleOver && acceptingInput)
            {
                acceptingInput = false;
                StartCoroutine(HandleBattleClicks(tilePos, curUnit));
            }
        }
        if (isPlacingUnit && unitToPlace)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            unitToPlace.transform.position = worldPos;
        }

    }
    private void setEnemyData()
    {
        levelManager = FindObjectOfType<LevelManager>();
        foreach (var curInfo in levelManager.enemyInfo)
        {
            EnemyUnit curEnemy = Instantiate(levelManager.typesOfEnemiesToSpawn[curInfo.Item1]);
            curEnemy.SetLocation(GetState(), curInfo.Item2);
            enemyUnits.Add(curEnemy);
            map.AddUnitToTile(curInfo.Item2, curEnemy);
        }
    }
    private IEnumerator InitializeBattle()
    {
        TurnOnPreview();
        ui.InitializeTurnCount(turnsPerBattle);

        // Done to delay coroutine to allow units to add themselves to unitsToSpawn
        yield return new WaitForFixedUpdate();

        // Place units waiting to be spawned on new map
        Debug.Log("Units to spawn: " + unitsToSpawn.Count);
        List<Coroutine> animations = new List<Coroutine>();
        foreach (Unit unit in unitsToSpawn.ToArray())
        {
            yield return StartCoroutine(SpawnUnit(unit.location, unit));
        }

        yield return StartCoroutine(UpdateBattleState());

        animations.Clear();

        foreach(Unit unit in unitsToSpawn)
        {
            unit.StartOfBattle();
        }

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

        yield return new WaitForSeconds(0.5f);

        // Place unit at start of round
        isPlayerTurn = false;
        isPlacingUnit = true;
        unitToPlace = null;
        yield return StartCoroutine(ui.ShowSelectionWindow());

        yield return new WaitUntil(() => !isPlacingUnit);

        CheckIfBattleOver();

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
            ui.DisableEndTurnButton();
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
            // Unit collision animation
            yield return StartCoroutine(unit.AppearAt(GetState(), spawnLocation));
            mapUnit.ChangeHealth(-1);
            if (!map.FindClosestFreeTile(spawnLocation, out spawnLocation))
            {
                // No empty space on map for falling unit
                yield return StartCoroutine(KillUnit(unit));
                Destroy(unit.gameObject);
                yield break;
            }
            unit.ChangeHealth(-1);
            yield return StartCoroutine(unit.BounceTo(GetState(), spawnLocation, 0.1f));
        }
        else
        {
            yield return StartCoroutine(unit.AppearAt(GetState(), spawnLocation));
        }

        map.AddUnitToTile(spawnLocation, unit);
        

        yield return StartCoroutine(map.OnUnitFallOnTile(GetState(), unit, spawnLocation));
        if (map.IsImpassableTile(unit.location))
        {
            yield return StartCoroutine(KillUnit(unit));
        }
        yield return StartCoroutine(UpdateBattleState());
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
        Debug.Log("ENEMY UNITS: " + enemyUnits.Count);
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
        else if (enemyUnits.Count <= 0)
        {
            isBattleOver = true;
            StartCoroutine(NextLevel());
        }
        else if (ui.isOutOfTurns())
        { 
            isBattleOver = true;
            StartCoroutine(NextLevel());
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
        levelManager.RefreshNewGame();
        yield return StartCoroutine(ui.SwitchScene("GameOverScreen"));
        foreach (EnemyUnit unit in enemyUnits.ToArray())
        {
            Destroy(unit.gameObject);
        }
        Destroy(gameObject); // Or similarly reset the battle manager
    }

    IEnumerator NextLevel()
    {
        levelManager.PrepareNextBattle();

        acceptingInput = false;

        unitsToSpawn.AddRange(enemyUnits);
        unitsToSpawn.AddRange(playerUnits);

        enemyUnits.Clear();
        playerUnits.Clear();

        int index = levelManager.currentLevel < levelManager.totalLevels ? SceneManager.GetActiveScene().buildIndex : 2;

        if (index != SceneManager.GetActiveScene().buildIndex)
        {
            StartCoroutine(ui.SwitchScene(index));
        }

        yield return StartCoroutine(ui.SwitchScene(index));
        Debug.Log("Next level finished loading");

        yield return null; // Need to wait a frame for the new level to load

        // Should refactor code so we don't need to find the tileManager. Should be returned by the level changing function
        map = FindObjectOfType<TileManager>();
        // This is probably also unnecessary if we structure things better
        ui = FindObjectOfType<UIController>();
        levelManager.map = FindObjectOfType<Tilemap>();
        Debug.Log("Found new TileManager: " + map != null);

        // If statement below destroys everything if we reach the win screen
        // Should probably be handled more elegantly
        if (map == null || ui == null)
        {
            levelManager.RefreshNewGame();
            Debug.Log("No TileManager or UIController found. Destroying GameManager");
            foreach (Unit unit in unitsToSpawn)
            {
                Destroy(unit.gameObject);
            }

            Destroy(gameObject);
            yield break;
        }
        if (!levelManager.isTutorial)
        {
            Debug.Log("SETTING DATA");
            setEnemyData();
        } 
        regeneratePreviews();
        StartCoroutine(InitializeBattle());
    }

    
    IEnumerator performEnemyMoves()
    {
        EnemyUnit[] enemies = enemyUnits.ToArray();
        foreach(EnemyUnit enemy in enemies)
        {   
            yield return enemy.performAction(GetState());
            yield return StartCoroutine(UpdateBattleState());
            CheckIfBattleOver();
            if(isBattleOver)
            {
                yield break;
            }
        }

        yield return StartCoroutine(StartOfPlayerTurn());
        DeselectUnit();
        ui.DecrementTurnCount();
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
    }

    public IEnumerator StartOfPlayerTurn()
    {
        foreach(PlayerUnit unit in playerUnits)
        {
            unit.StartOfTurn();
        }

        ui.DisableEndTurnButton();
        isPlayerTurn = true;
        yield break;
    }

    private IEnumerator HandlePlacingClicks(Vector3Int tilePos, Unit curUnit)
    {
        if (map.InBounds(tilePos) && curUnit == null && unitToPlace)
        {
            Debug.Log("Unit placement location: " + tilePos);
            PlayerUnit unit = unitToPlace;
            unitToPlace = null;
            unit.location = tilePos;
            yield return StartCoroutine(SpawnUnit(tilePos, unit));
            unitsToSpawn.Remove(unit);
            isPlacingUnit = false;
        }
    }

    private IEnumerator MoveUnit(Unit unit, Vector3Int tilePos)
    {
        map.ClearHighlights();
        yield return StartCoroutine(unit.DoMovement(state, tilePos));
        yield return StartCoroutine(UpdateBattleState());
    }

    private IEnumerator HandleBattleClicks(Vector3Int tilePos, Unit curUnit)
    {
        if(!map.GetTile(tilePos))
        {
            DeselectUnit();
        } else if (curUnit is PlayerUnit)
        {
            if (isPlayerTurn && selectedUnit is PlayerUnit unit
                && !unit.hasMoved && unit == curUnit)
            {
                yield return StartCoroutine(MoveUnit(unit, tilePos));
                CheckIfBattleOver();
                if(!isBattleOver && unit && !unit.isDead)
                {
                    ShowUnitAttackRange(unit);
                }
            }
            SelectUnit(curUnit);
        }
        else if (curUnit is EnemyUnit)
        {
            if (isPlayerTurn && selectedUnit is PlayerUnit unit 
                && unit.hasMoved && !unit.hasAttacked && unit.IsTileInAttackRange(tilePos))
            {
                map.ClearHighlights();
                yield return StartCoroutine(unit.DoAttack(curUnit));
                if(curUnit.isDead)
                {
                    yield return StartCoroutine(KillUnit(curUnit));
                    yield return StartCoroutine(UpdateBattleState());
                    CheckIfBattleOver();
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
                && !unit.hasMoved && !map.IsImpassable(tilePos) && unit.IsTileInMoveRange(tilePos))
            {
                yield return StartCoroutine(MoveUnit(unit, tilePos));
                CheckIfBattleOver();
                if(!isBattleOver && unit && !unit.isDead)
                {
                    ShowUnitAttackRange(unit);
                }
            }
                else
            {
                DeselectUnit();
                SelectTile(tilePos);
            }
            
        }
        else
        {
            DeselectUnit();
        }
        acceptingInput = true;
    }

    /// <summary>
    /// Selects the given tile and displays its details
    /// </summary>
    /// <param name="tilePos">the position of the tile to display</param>
    public void SelectTile(Vector3Int tilePos)
    {
        ui.ShowTileInWindow(map.GetTileData(tilePos));
    }

    public void SelectUnit(Unit unit)
    {
        map.ClearHighlights();
        selectedUnit = unit;
        if(unit is PlayerUnit player && isPlayerTurn)
        {
            if (!player.hasMoved)
            {
                ui.ShowUnitInfoWindow(unit);
                ShowUnitMoveRange(unit);
            }
            else if (!player.hasAttacked)
            {
                ui.ShowUnitInfoWindow(unit);
                ShowUnitAttackRange(unit);
            }
        } 
        else if(unit is EnemyUnit)
        {
            ui.ShowUnitInfoWindow(unit);
            ShowUnitThreatRange(unit);
        }
    }

    public void DeselectUnit()
    {
        ui.HideUnitInfoWindow();
        map.ClearHighlights();
        selectedUnit = null;
    }

    private void ShowUnitMoveRange(Unit unit)
    {
        Debug.Log("Show move range");
        map.ClearHighlights();
        if(isPlayerTurn)
        {
            map.HighlightPath(unit.GetTilesInMoveRange(), Color.blue);
        }
    }

    private void ShowUnitAttackRange(Unit unit)
    {
        Debug.Log("Show attack range");
        map.ClearHighlights();
        if(isPlayerTurn)
        {
            map.HighlightPath(unit.GetTilesInAttackRange(), Color.red);
        }
    }

    private void ShowUnitThreatRange(Unit unit)
    {
        map.ClearHighlights();
        if (isPlayerTurn)
        {
            map.HighlightPath(unit.GetTilesInThreatRange(), Color.red);
        }
    }

    public class EnemyPosNextScene
    {
        public List<Vector3Int> locations = new List<Vector3Int>();
    }

    private void Save()
    {
        EnemyPosNextScene enemyPos = new EnemyPosNextScene();        
        foreach (var curUnit in unitsToSpawn)
        {
            if (curUnit is EnemyUnit)
            {
                enemyPos.locations.Add(curUnit.location);
            }
        }
        string json = JsonUtility.ToJson(enemyPos);
        File.WriteAllText(Application.dataPath + string.Format("/posData{0}.json", SceneManager.GetActiveScene().buildIndex), json);
    }

    private void Load(int sceneIndex)
    {
        if (File.Exists(Application.dataPath + string.Format("/posData{0}.json", SceneManager.GetActiveScene().buildIndex + 1))){
            string savestring = File.ReadAllText(Application.dataPath + string.Format("/posData{0}.json", SceneManager.GetActiveScene().buildIndex + 1));
            //enemyPosNextScene = JsonUtility.FromJson<EnemyPosNextScene>(savestring);
        }
        else
        {
            Debug.Log("Can't find file");
        }
    }
}
