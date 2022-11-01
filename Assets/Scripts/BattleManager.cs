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
using UnityEngine.EventSystems;
using SpriteGlow;

/// <summary>
/// Stores lists of player units, enemy units, the battle manager, and the
/// tile manager
/// </summary>
[System.Serializable]
public class BattleState : ScriptableObject
{
    public List<PlayerUnit> playerUnits;
    public List<EnemyUnit> enemyUnits;
    public List<NPCUnit> NPCUnits;
    public TileManager tileManager;
    [HideInInspector] public BattleManager battleManager;

}

/// <summary>
/// Handles all things related to battle including start of battle unit
/// placement
/// </summary>
public class BattleManager : MonoBehaviour
{
    [HideInInspector]
    private BattleState state;
    public List<PlayerUnit> playerUnits;
    public List<EnemyUnit> enemyUnits;
    public List<NPCUnit> NPCUnits;
    public List<Unit> unitsToSpawn;
    public TileManager tileManager;
    public int turnsPerBattle = 5;
    public bool isBattleOver = false;
    public bool isPlayerTurn = true;
    public bool isPlacingUnit = false;
    public bool acceptingInput = true;
    public bool usingAbility = false;
    public Unit selectedUnit;
    public UIController ui;
    public PlayerUnit unitToPlace;
    [ReadOnly]
    public bool tileSelected = false;

    /// <summary>
    /// A list of tiles which are modified separately from coloredTiles in TileManager.
    /// Used to store tiles which can be reset separately from the tiles colored using
    /// TileManager functionality. The color stored is the color the tile will be reset to
    /// and not the present highlight color of the tile.
    /// </summary>
    [ReadOnly]
    public List<(Vector3Int, Color)> highlightedTiles;
    public Vector3Int selectedTile;
    [ReadOnly]
    public Color selectedTilePrevColor = Color.white;

    private bool playerGaveInput = false;   // Indicates if playerInput has been set
    private bool playerInput = false;       // Indicates a player's choice

    public GameObject previewLayer;
    public bool previewVisible = false;

    [HideInInspector]
    public static BattleManager instance;

    public GameObject tileOutlinePrefab;
    private GameObject tileOutline;

    public LevelManager levelManager;


    public Vector3 mapPosition;

    private PostProcessingSettings postProcessingSettings;

    public bool gameIsPaused = false;


    //Tutorial stuff
    public Button ovisButton;
    public TutorialManager tutorialManager;
    public DialogueManager dialogueManager;
    public Vector3Int forcedUnitPlacementTile = new Vector3Int(0, 0, -1);
    public Vector3Int forcedUnitMovementTile = new Vector3Int(0, 0, -1);
    public bool pushDialogueAfterEnemyTurn = false;
    public bool pushDialogueAfterAttack = false;
    public bool pushDialogueAfterBattleEnd = false;

    /// <summary>
    /// Instantiates a unit prefab which is updated in the update loop to follow the
    /// mouse of the player
    /// </summary>
    public void SetUnitToPlace(PlayerUnit prefab)
    {
        isPlacingUnit = true;
        acceptingInput = true;
        unitToPlace = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        unitToPlace.anim.SetBool("Hide", false); // Replace with method
    }

    private void Awake()
    {
        tileManager = FindObjectOfType<TileManager>();
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }

        state = new BattleState();
        mapPosition = tileManager.transform.position;
        highlightedTiles = new List<(Vector3Int, Color)>();
        postProcessingSettings = FindObjectOfType<PostProcessingSettings>();
    }

    public void EndTurn()
    {
        StartCoroutine(instance.OnPlayerEndTurn());
    }

    public void TogglePreview()
    {
        if (!acceptingInput)
        {
            return;
        }
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
            previewLayer.transform.position = mapPosition;
            previewLayer.SetActive(true);
            previewVisible = true;
        } 
        else
        {
            previewVisible = false;
        }
    }

    public void TurnOffPreview()
    {
        previewVisible = false;
        if(previewLayer)
        {
            previewLayer.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ui.HideSelectionWindow());
        ui.HideUnitInfoWindow();
        if (!LevelManager.instance.isTutorial)
        {
            Debug.Log("SETTING DATA");
            setEnemyData();
            StartCoroutine(InitializeBattle());
            regeneratePreviews();
        }
        else
        {
            StartCoroutine(InitializeBattleTutorial());
        }
    }

    /// <summary>
    /// Update preview render to show next stage hazards and enemies
    /// </summary>
    private void regeneratePreviews()
    {
        var curGeneratePreviews = Resources.FindObjectsOfTypeAll<generatePreviews>();
        Debug.Log(curGeneratePreviews);
        if (!curGeneratePreviews[0])
        {
            Debug.LogError("Can't find preview layer q_q");
        }
        previewLayer = curGeneratePreviews[0].gameObject;
        
        curGeneratePreviews[0].ShowEnemyPreview(LevelManager.instance.nextSceneEnemyInfo, GetState());
        curGeneratePreviews[0].ShowHazzardAndImpassablePreview(LevelManager.instance.nextSceneTileInfo, GetState());
    }

    // Update is called once per frame
    void Update()
    {
        // For other clicks, we do not want to do anything if we are over an UI object.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (tileManager)
        {
            Vector3Int tilePos = tileManager.GetTileAtScreenPosition(Input.mousePosition);
            OutlineTile(tilePos);

            // Indicate tile/unit player is hovering over
            if (acceptingInput && !selectedUnit)
            {
                if (!tileManager.InBounds(tilePos))
                {
                    ui.HideTileWindow();
                    ui.HideUnitInfoWindow();
                }
                else
                {
                    Unit unit = tileManager.GetUnit(tilePos);
                    if (unit)
                    {
                        ui.ShowUnitInfoWindow(unit);
                    }
                    else
                    {
                        try
                        {
                            DisplayTile(tilePos);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            if (tutorialManager && tutorialManager.disableBattleInteraction)
            {
                return;
            }

            // Always want to update position of the unit prefab being placed.
            if (isPlacingUnit && unitToPlace)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0;
                unitToPlace.transform.position = worldPos;
            }

            if (acceptingInput && Input.GetMouseButtonDown(0))
            {
                Debug.Log(tilePos);

                Unit curUnit = tileManager.GetUnit(tilePos);

                if (isPlacingUnit)
                {
                    if (!tileManager.IsImpassableTile(tilePos))
                        StartCoroutine(HandlePlacingClicks(tilePos, curUnit));
                }
                else if (acceptingInput)
                {
                    acceptingInput = false;
                    StartCoroutine(HandleBattleClicks(tilePos, curUnit));
                }
            }
        }
    }

    private bool HasMoves()
    {
        foreach (var unit in playerUnits)
        {
            if (unit.UnitsToAttackInRange(enemyUnits) && !unit.hasAttacked)
            {
                return true;
            }
            //if (unit.coolDown <= 0)
            //{
            //    return true;
            //}
            if (!unit.hasMoved)
            {
                return true;
            }
        }
        return false;
    }
    private IEnumerator HandleBattleClicks(Vector3Int tilePos, Unit curUnit)
    {
        // Handle clicking off the map
        if(!tileManager.InBounds(tilePos))
        {
            DeselectUnit();
            
        }
        else if (selectedUnit == curUnit)
        {
            DeselectUnit();
        }
        // Handle clicking while aiming ability
        else if (usingAbility && selectedUnit is PlayerUnit playerUnit)
        {
            if (!playerUnit.IsTileInAbilityRange(tilePos))
            {
                DeselectUnit();
                acceptingInput = true;
                yield break;
            }
            if (!tileSelected || !tilePos.Equals(selectedTile))
            {
                SelectTile(tilePos);
                acceptingInput = true;
                yield break;
            }
            DeselectTile();
            yield return StartCoroutine(playerUnit.UseAbility(tilePos, state));
            if (playerUnit.currentCoolDown == playerUnit.coolDown)
            {
                DeselectUnit();
                usingAbility = false;
                playerUnit.hasAttacked = true;
                yield return StartCoroutine(UpdateBattleState());
                CheckIfBattleOver();
            }   
        }
        // Handle clicking on a player unit
        else if (curUnit is PlayerUnit)
        {
            DeselectTile();
            SelectUnit(curUnit);         
        }
        // Handle clicking on enemy unit
        else if (curUnit is EnemyUnit)
        {
            // Handle attacking before moving

            // Handle selecting attack target
            if (isPlayerTurn && selectedUnit is PlayerUnit unit)
            {
                // Handle attacking before moving
                Vector3Int attackPosition;
                if (!unit.hasMoved && !unit.hasAttacked && unit.IsTileInThreatRange(tilePos, out attackPosition))
                {
                    if (!tileSelected || !tilePos.Equals(selectedTile))
                    {
                        SelectTile(tilePos);
                        highlightedTiles.Add((attackPosition, tileManager.GetTileColor(attackPosition)));
                        tileManager.SetTileColor(attackPosition, Color.cyan);
                        ui.ShowUnitInfoWindow(curUnit);
                        acceptingInput = true;
                        yield break;
                    }
                    DeselectTile();
                    tileManager.ClearHighlights();
                    yield return StartCoroutine(MoveUnit(unit, attackPosition));
                    CheckIfBattleOver();
                    if (!isBattleOver && unit && !unit.isDead)
                    {
                        yield return StartCoroutine(unit.DoAttack(curUnit));

                        if (pushDialogueAfterAttack)
                        {
                            dialogueManager.doSkipDialogue = true;
                        }

                        yield return StartCoroutine(UpdateBattleState());
                        CheckIfBattleOver();
                    }
                }

                // Hanlde selecting attack target
                if(unit.hasMoved && !unit.hasAttacked && unit.IsTileInAttackRange(tilePos))
                {
                    if (!tileSelected || !tilePos.Equals(selectedTile))
                    {
                        SelectTile(tilePos);
                        ui.ShowUnitInfoWindow(curUnit);
                        acceptingInput = true;
                        yield break;
                    }
                    DeselectTile();
                    tileManager.ClearHighlights();
                    yield return StartCoroutine(unit.DoAttack(curUnit));
                    yield return StartCoroutine(UpdateBattleState());
                    CheckIfBattleOver();
                }
            }
            else
            {
                SelectUnit(curUnit);
            }
        }
        // Handle clicking on an empty tile
        else if (curUnit == null)
        {
            if(isPlayerTurn && selectedUnit is PlayerUnit unit
                && !unit.hasMoved && !tileManager.IsImpassableTile(tilePos) && unit.IsTileInMoveRange(tilePos))
            {
                if (forcedUnitMovementTile.z == 0 && tilePos != forcedUnitMovementTile)
                {
                    DeselectUnit();
                    acceptingInput = true;
                    yield break;
                }
                if (!tileSelected || !tilePos.Equals(selectedTile))
                {
                    SelectTile(tilePos);
                    ui.ShowUnitInfoWindow(selectedUnit);
                    acceptingInput = true;
                    yield break;
                }
                DeselectTile();
                yield return StartCoroutine(MoveUnit(unit, tilePos));
                CheckIfBattleOver();
                if (!isBattleOver && unit && !unit.isDead && !unit.hasAttacked)
                {
                    ShowUnitAttackRange(unit);
                }
                else
                {
                    StartCoroutine(unit.Dim());
                    DeselectUnit();
                }
            }
            else
            {
                DeselectUnit();
                DisplayTile(tilePos);
            }
            
        }
        else
        {
            DeselectUnit();
        }
        acceptingInput = true;
        if (isPlayerTurn && !isBattleOver && !HasMoves() && playerUnits.Count != 0)
        {
            StartCoroutine(ui.SetEndTurnButtonHighlight(true));
        }
    }

    public void AbilityButton()
    {
        Debug.Log("ABILITY");
        usingAbility = !usingAbility;
        tileManager.ClearHighlights();

        if (usingAbility)
        {
            if (selectedUnit is PlayerUnit unit)
            {
                tileManager.HighlightPath(unit.GetTilesInAbilityRange(), Color.red);
            }
        }
    }

    private void setEnemyData()
    {
        foreach (var curInfo in LevelManager.instance.enemyInfo)
        {
            EnemyUnit curEnemy = Instantiate(LevelManager.instance.typesOfEnemiesToSpawn[curInfo.Item1]);
            curEnemy.SetLocation(GetState(), curInfo.Item2);
            enemyUnits.Add(curEnemy);
            curEnemy.Show();
            tileManager.AddUnitToTile(curInfo.Item2, curEnemy);
        }
    }

    private IEnumerator InitializeBattleTutorial()
    {
        pushDialogueAfterBattleEnd = true;
        // Done to delay coroutine to allow units to add themselves to unitsToSpawn
        yield return new WaitForFixedUpdate();

        while (tutorialManager.index < 5)
        {
            yield return StartCoroutine(tutorialManager.NextDialogue());
        }

        // Handles unit selection tutorial
        StartCoroutine(ui.ShowSelectionWindow(false));
        tutorialManager.disableBattleInteraction = true;
        yield return StartCoroutine(tutorialManager.NextDialogue(true));

        // Wait until user does what is asked. This is not the only thing stopping
        // progression. Dialogue system's isWaitingForUserInput also stops progression.
        // However, both are needed otherwise the system will break.
        while (ui.unitSelectionWindow.gameObject.activeSelf)
        {
            yield return new WaitForEndOfFrame();
        }

        //Advise user to watch for tiles
        unitToPlace.spriteRenderer.enabled = false;
        Debug.Log("WE TALKING");
        yield return StartCoroutine(tutorialManager.NextDialogue());


        //Highlight hazardous and talk about them
        foreach (Vector3Int tileLocation in tileManager.dynamicTileDatas.Keys)
        {
            if (tileManager.IsHazardous(tileLocation))
            {
                tileManager.SetTileColor(tileLocation, Color.red);
            }
        }
        yield return StartCoroutine(tutorialManager.NextDialogue());
        tileManager.ClearHighlights();        

        //Highlight impassable and talk about them
        foreach (Vector3Int tileLocation in tileManager.dynamicTileDatas.Keys)
        {
            if (tileManager.IsImpassableTile(tileLocation, false))
            {
                tileManager.SetTileColor(tileLocation, Color.red);
            }
        }
        yield return StartCoroutine(tutorialManager.NextDialogue());
        tileManager.ClearHighlights();

        //Tell user how to place unit
        forcedUnitPlacementTile = new Vector3Int(-3, -1, 0);
        tileManager.SetTileColor(forcedUnitPlacementTile, Color.blue);
        unitToPlace.spriteRenderer.enabled = true;
        tutorialManager.disableBattleInteraction = false;        
        yield return StartCoroutine(tutorialManager.NextDialogue(true));

        // Wait until user does what is asked.
        while (isPlacingUnit)
        {
            yield return new WaitForEndOfFrame();
        }

        //Unrestrict placement
        forcedUnitPlacementTile.z = -1;
        tileManager.ClearHighlights();

        tutorialManager.disableBattleInteraction = true;

        //NPC dialogue
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(tutorialManager.NextDialogue());

        // Place units waiting to be spawned on new map
        Debug.Log("Units to spawn: " + unitsToSpawn.Count);
        List<Coroutine> animations = new List<Coroutine>();
        foreach (Unit unit in unitsToSpawn.ToArray())
        {
            yield return StartCoroutine(SpawnUnit(unit.location, unit));
        }

        yield return StartCoroutine(UpdateBattleState());

        animations.Clear();
        unitsToSpawn.Clear();

        //Discussing hovering
        yield return StartCoroutine(tutorialManager.NextDialogue());
        

        //prompt to click enemy unit
        tutorialManager.disableBattleInteraction = false;
        //Force impossible to click tile to prevent movement
        forcedUnitMovementTile = new Vector3Int(-1000, -1, 0);
        yield return StartCoroutine(tutorialManager.NextDialogue());

        // Discuss deselecting units
        yield return StartCoroutine(tutorialManager.NextDialogue());

        //prompt to click ally unit and move
        forcedUnitMovementTile = new Vector3Int(-2, -1, 0);
        tileManager.SetTileColor(forcedUnitMovementTile, Color.red, true);

        yield return StartCoroutine(StartOfPlayerTurn());
        tutorialManager.endTurnButton.SetInteractable(false);
        if(selectedUnit && selectedUnit is PlayerUnit)
        {
            postProcessingSettings.ChangeColorToDeSelected((PlayerUnit)selectedUnit);
        }

        yield return StartCoroutine(tutorialManager.NextDialogue(true));  

        bool notMoved = true;
        while (notMoved)
        {
            foreach (PlayerUnit playerUnit in playerUnits)
            {
                if (playerUnit.hasMoved)
                {
                    notMoved = false;
                }
            }
            yield return new WaitForEndOfFrame();
        }
        tileManager.ClearHighlights(true);

        //prompt to end turn
        ui.HideUnitInfoWindow();
        isPlayerTurn = true;
        tutorialManager.endTurnButton.SetInteractable(true);
        StartCoroutine(ui.EnableEndTurnButton());
        pushDialogueAfterEnemyTurn = true;
        yield return StartCoroutine(tutorialManager.NextDialogue(true));

        // Wait until player turn comes around again.
        while (isPlayerTurn)
        {
            yield return new WaitForEndOfFrame();
        }

        while (!isPlayerTurn)
        {
            yield return new WaitForEndOfFrame();
        }

        //Force impossible to click tile to prevent movement
        forcedUnitMovementTile = new Vector3Int(-1000, -1, 0);

        //Make sure user does not skip ahead.
        tutorialManager.disableBattleInteraction = true;

        pushDialogueAfterEnemyTurn = false;

        // Discuss what happened during enemy turn
        yield return StartCoroutine(tutorialManager.NextDialogue());
        
        // Prompt to attack
        pushDialogueAfterAttack = true;
        tutorialManager.disableBattleInteraction = false;
        yield return StartCoroutine(ui.DisableEndTurnButton());
        yield return StartCoroutine(tutorialManager.NextDialogue(true));

        bool notAttacked = true;
        while (notAttacked)
        {
            foreach (PlayerUnit playerUnit in playerUnits)
            {
                if (playerUnit.hasAttacked)
                {
                    notAttacked = false;
                }
            }
            yield return new WaitForEndOfFrame();
        }      

        // Unrestrict future unit movements
        forcedUnitMovementTile.z = -1;

        ui.HideUnitInfoWindow();
        yield return StartCoroutine(ui.EnableEndTurnButton());
        pushDialogueAfterAttack = false;

        //Discussing attack mechanics
        Debug.Log("talk attack mechanics");
        yield return StartCoroutine(tutorialManager.NextDialogue());

        if (isBattleOver)
        {
            tutorialManager.index = tutorialManager.NumLines() - 1;
            yield break;
        }

        //prompt end turn
        Debug.Log("prompt end turn");
        yield return StartCoroutine(tutorialManager.NextDialogue(true));

        while (!isBattleOver)
        {
            yield return new WaitForEndOfFrame();
        }

        tutorialManager.index = tutorialManager.NumLines() - 1;
    }

    private IEnumerator InitializeBattleTutorial2()
    {
        isPlayerTurn = false;
        // Done to delay coroutine to allow units to add themselves to unitsToSpawn
        yield return new WaitForFixedUpdate();

        // Discussing location and health being maintained
        yield return StartCoroutine(tutorialManager.NextDialogue());

        // Place units waiting to be spawned on new map
        Debug.Log("Units to spawn: " + unitsToSpawn.Count);
        List<Coroutine> animations = new List<Coroutine>();
        foreach (Unit unit in unitsToSpawn.ToArray())
        {
            Debug.Log(unit);
            yield return StartCoroutine(SpawnUnit(unit.location, unit));
        }
        yield return StartCoroutine(UpdateBattleState());

        // NPC dialogue
        yield return StartCoroutine(tutorialManager.NextDialogue());

        // Discussing why Itzel died
        yield return StartCoroutine(tutorialManager.NextDialogue());

        //Advising to be careful of next stage hazards
        yield return StartCoroutine(tutorialManager.NextDialogue());

        var curGeneratePreviews = Resources.FindObjectsOfTypeAll<generatePreviews>();
        if (!curGeneratePreviews[0])
        {
            Debug.LogError("Can't find preview layer q_q");
        }
        previewLayer = curGeneratePreviews[0].gameObject;

        TurnOnPreview();

        //Talk about enemy overlay
        curGeneratePreviews[0].ShowEnemyPreview(LevelManager.instance.nextSceneEnemyInfo, GetState());
        yield return StartCoroutine(tutorialManager.NextDialogue());
        
        // Disable enemy overview temporarily to talk about other ones
        foreach (Transform child in previewLayer.transform)
        {
            child.gameObject.SetActive(false);
        }

        // Talk about hazard overlay
        curGeneratePreviews[0].ShowHazardPreview(LevelManager.instance.nextSceneTileInfo, GetState());
        yield return StartCoroutine(tutorialManager.NextDialogue());

        // Disable hazard overview temporarily to talk about other ones
        foreach (Transform child in previewLayer.transform)
        {
            child.gameObject.SetActive(false);
        }

        //Talk about impassable overlay
        curGeneratePreviews[0].ShowImpassablePreview(LevelManager.instance.nextSceneTileInfo, GetState());
        yield return StartCoroutine(tutorialManager.NextDialogue());

        // Emphasize the effect of units falling on impassable tiles
        yield return StartCoroutine(tutorialManager.NextDialogue());

        // Talk about the overlay being active for the rest of the game
        yield return StartCoroutine(tutorialManager.NextDialogue());

        // Activate all overlay elements
        foreach (Transform child in previewLayer.transform)
        {
            child.gameObject.SetActive(true);
        }
        
        animations.Clear();

        foreach(Unit unit in unitsToSpawn)
        {
            unit.StartOfBattle();
        }

        // Activate any start of battle abilities
        foreach (Unit unit in unitsToSpawn.ToArray())
        {
            // Check that unit didn't die already
            if (unit)
            {
                yield return StartCoroutine(unit.StartOfBattleAbility(GetState()));
            }
        }

        // Talk about start of battle abilities
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(tutorialManager.NextDialogue());

        // Talk about in battle abilities
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(tutorialManager.NextDialogue());

        dialogueManager.HideDialogueWindow();

        /*foreach (Coroutine anim in animations)
        {
            yield return anim;
        }*/

        yield return StartCoroutine(UpdateBattleState());

        unitsToSpawn.Clear();

        yield return new WaitForSeconds(0.5f);

        // Place unit at start of round
        isPlacingUnit = true;
        unitToPlace = null;
        acceptingInput = true;
        yield return StartCoroutine(ui.ShowSelectionWindow());

        yield return new WaitUntil(() => !isPlacingUnit);

        CheckIfBattleOver();

        yield return StartCoroutine(StartOfPlayerTurn());
        tutorialManager.endTurnButton.SetInteractable(true);
        StartCoroutine(ui.EnableEndTurnButton());

        isBattleOver = false;
        if(selectedUnit && selectedUnit is PlayerUnit)
        {
            postProcessingSettings.ChangeColorToDeSelected((PlayerUnit)selectedUnit);
        }
        selectedUnit = null;
        isPlayerTurn = true;        
    }

    private IEnumerator InitializeBattle()
    {
        TurnOnPreview();
        ui.InitializeTurnCount(turnsPerBattle);
        isPlayerTurn = false;
        acceptingInput = false;

        // Done to delay coroutine to allow units to add themselves to unitsToSpawn
        yield return new WaitForFixedUpdate();

        // Place units waiting to be spawned on new map
        Debug.Log("Units to spawn: " + unitsToSpawn.Count);
        List<Coroutine> animations = new List<Coroutine>();
        foreach (Unit unit in unitsToSpawn.ToArray())
        {
            yield return StartCoroutine(SpawnUnit(unit.location, unit));
        }

        // Wait for damage animation to finish
        yield return new WaitForSeconds(.5f);

        yield return StartCoroutine(UpdateBattleState());

        animations.Clear();

        foreach(Unit unit in unitsToSpawn)
        {
            unit.StartOfBattle();
        }

        // Activate any start of battle abilities
        foreach (Unit unit in unitsToSpawn.ToArray())
        {
            // Check that unit didn't die already
            if (unit)
            {
                yield return StartCoroutine(unit.StartOfBattleAbility(GetState()));
            }
        }

        /*foreach (Coroutine anim in animations)
        {
            yield return anim;
        }*/

        yield return StartCoroutine(UpdateBattleState());

        unitsToSpawn.Clear();

        yield return new WaitForSeconds(0.5f);

        // Place unit at start of round
        isPlacingUnit = true;
        unitToPlace = null;
        yield return StartCoroutine(ui.ShowSelectionWindow());

        yield return new WaitUntil(() => !isPlacingUnit);

        CheckIfBattleOver();

        yield return StartCoroutine(StartOfPlayerTurn());
        isBattleOver = false;
        if(selectedUnit && selectedUnit is PlayerUnit)
        {
            postProcessingSettings.ChangeColorToDeSelected((PlayerUnit)selectedUnit);
        }
        selectedUnit = null;
        isPlayerTurn = true;
    }

    public IEnumerator OnPlayerEndTurn()
    {
        if(isPlayerTurn && acceptingInput)
        {
            DeselectUnit();
            isPlayerTurn = false;

            if (AreUnmovedPlayerUnits())
            {
                Debug.Log("Unmoved units left! Displaying warning");
                StartCoroutine(ui.ShowEarlyEndTurnWarning());
                yield return new WaitUntil(() => playerGaveInput);
                playerGaveInput = false;
                if (playerInput == false)
                {
                    // Abort end turn
                    isPlayerTurn = true;
                    yield break;
                }
            }

            StartCoroutine(ui.DisableEndTurnButton());

            // Player units still need to take damage from hazards even if they have not moved
            foreach (var unit in playerUnits)
            {
                if (unit.hasMoved == false && tileManager.IsHazardous(unit.location))
                {
                    unit.ChangeHealth(-1);
                }
            }

            yield return StartCoroutine(performEnemyMoves());



            List<Unit> unitsToDestroy = new();
            var turnPast = ui.turnCountDown.totalTurn - ui.turnCountDown.currentTurn;
            if (turnPast == 2){
                yield return StartCoroutine(tileManager.ShatterTiles(1, unitsToDestroy));
            }else if (turnPast == 3)
            {
                yield return StartCoroutine(tileManager.ShatterTiles(2, unitsToDestroy));
            }

            foreach(var unit in unitsToDestroy)
            {
                RemoveUnit(unit);
                Destroy(unit);
            }

            StartCoroutine(UpdateBattleState());
            CheckIfBattleOver();
        }
    }

    /// <summary>
    /// Determines if there are any units that haven't moved or attacked
    /// </summary>
    /// <returns>true if there are player units that haven't moved or attacked. False otherwise</returns>
    public bool AreUnmovedPlayerUnits()
    {
        foreach (PlayerUnit unit in playerUnits)
        {
            if (!unit.hasMoved && !unit.hasAttacked)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if all player units have moved and acted
    /// </summary>
    /// <returns>Returns true if all player units have moved and acted. False otherwise.</returns>
    public bool AllPlayerUnitsMoved()
    {
        foreach (PlayerUnit unit in playerUnits)
        {
            if (!unit.hasMoved || !unit.hasAttacked)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Sets the player input variable on the current battle manager.
    /// Intended to be called by ui elements
    /// </summary>
    /// <param name="input">the input given by the player</param>
    public void SetPlayerInput(bool input)
    {
        instance.playerInput = input;
        instance.playerGaveInput = true;
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
                Debug.Log("ADDING: " + unit.isDead);
                enemyUnits.Add((EnemyUnit)unit);
            }
            else if (unit is NPCUnit)
            {
                NPCUnits.Add((NPCUnit)unit);
            }
            else
            {
                Debug.LogError("Adding a null unit");
                yield break;
            }
        }

        Vector3Int spawnLocation = location;
        Debug.Log(spawnLocation);
        Unit mapUnit = tileManager.GetUnit(spawnLocation);
        if (mapUnit)
        {
            // Unit fell on another unit!
            Debug.Log("Falling unit collision!");
            // Unit collision animation
            yield return StartCoroutine(unit.AppearAt(GetState(), spawnLocation));
            mapUnit.ChangeHealth(-1);
            if (!tileManager.FindClosestFreeTile(spawnLocation, out spawnLocation))
            {
                // No empty space on map for falling unit
                yield return StartCoroutine(KillUnit(unit));

                //Is this correct? Does not seem so. We want to get rid of the unit from the other
                // data structs as well as destroying the unit. Should just deactivate the sprite.
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

        tileManager.AddUnitToTile(spawnLocation, unit);
        

        yield return StartCoroutine(tileManager.OnUnitFallOnTile(GetState(), unit, spawnLocation));
        if (!tileManager.IsImpassableTile(unit.location, false))
        {
        }
        else
        {
            yield return StartCoroutine(KillUnit(unit));
        }
        yield return StartCoroutine(UpdateBattleState());
    }

    public IEnumerator KillUnit(Unit unit)
    {
        RemoveUnit(unit);
        yield return StartCoroutine(unit.Die());    
    }

    public void RemoveUnit(Unit unit)
    {
        if (unit is PlayerUnit)
        {
            playerUnits.Remove((PlayerUnit)unit);
        }
        else if (unit is EnemyUnit)
        {
            enemyUnits.Remove((EnemyUnit)unit);
        }
        else if (unit is NPCUnit)
        {
            NPCUnits.Remove((NPCUnit)unit);
        }
        else
        {
            Debug.LogError("Removing a null unit");
        }
        Debug.Log("ENEMY UNITS: " + enemyUnits.Count);
        tileManager.RemoveUnitFromTile(unit.location);
    }

    public void CheckIfBattleOver()
    {
        if (isBattleOver)
        {
            return;
        }
        if (playerUnits.Count <= 0)
        {
            isBattleOver = true;
            Debug.Log("In GameOver");
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
        state.NPCUnits = NPCUnits;
        state.tileManager = tileManager;
        state.battleManager = this;
        return state;
    }

    IEnumerator ShowGameOver()
    {
        LevelManager.instance.RefreshNewGame();
        yield return StartCoroutine(ui.SwitchScene("GameOverScreen"));
        foreach (EnemyUnit unit in enemyUnits.ToArray())
        {
            Destroy(unit.gameObject);
        }
        Destroy(gameObject); // Or similarly reset the battle manager
    }

    IEnumerator NextLevel()
    {
        postProcessingSettings.DisableTheGlow(playerUnits);
        if (pushDialogueAfterBattleEnd)
        {
            dialogueManager.doSkipDialogue = true;
            yield return new WaitForSeconds(0.25f);
            dialogueManager.doSkipDialogue = false;
            yield return StartCoroutine(tutorialManager.NextDialogue());
            pushDialogueAfterBattleEnd = false;
        }

        foreach (PlayerUnit player in playerUnits)
        {
            StartCoroutine(player.Undim());
        }

        yield return StartCoroutine(tileManager.ShatterTiles(0));


        int index;
        if (LevelManager.instance.isTutorial && tutorialManager)
        {
            index = SceneManager.GetActiveScene().buildIndex + 1;
        }
        else
        {
            Debug.Log("current level: " + LevelManager.currentLevel);
            index = LevelManager.currentLevel - LevelManager.instance.NumTutorialLevels < 
                    LevelManager.instance.totalLevels ? SceneManager.GetActiveScene().buildIndex : 8;
            Debug.Log("index: " + index);
        }
        LevelManager.instance.IncrementLevel();

        if (!LevelManager.instance.isTutorial && tutorialManager)
        {
            ResetAll();
        }

        acceptingInput = false;

        unitsToSpawn.AddRange(enemyUnits);
        unitsToSpawn.AddRange(playerUnits);
        unitsToSpawn.AddRange(NPCUnits);

        enemyUnits.Clear();
        playerUnits.Clear();
        NPCUnits.Clear();

        yield return StartCoroutine(ui.SwitchScene(index));
        Debug.Log("Next level finished loading");

        LevelManager.instance.PrepareNextBattle();

        foreach (Unit unit in unitsToSpawn)
        {
            unit.Hide();
        }

        yield return null; // Need to wait a frame for the new level to load

        // Should refactor code so we don't need to find the tileManager. Should be returned by the level changing function
        tileManager = FindObjectOfType<TileManager>();
        tileManager.map = FindObjectOfType<Tilemap>();
        // This is probably also unnecessary if we structure things better
        ui = FindObjectOfType<UIController>();
        tutorialManager = FindObjectOfType<TutorialManager>();
        dialogueManager = FindObjectOfType<DialogueManager>();
        LevelManager.instance.map = FindObjectOfType<Tilemap>();
        Debug.Log("Found new TileManager: " + tileManager != null);

        // If statement below destroys everything if we reach the win screen
        // Should probably be handled more elegantly
        if (tileManager == null || ui == null)
        {
            LevelManager.instance.RefreshNewGame();
            Debug.Log("No TileManager or UIController found. Destroying GameManager");
            foreach (Unit unit in unitsToSpawn)
            {
                Destroy(unit.gameObject);
            }

            Destroy(gameObject);
            yield break;
        }
        if (!LevelManager.instance.isTutorial)
        {
            Debug.Log("SETTING DATA");
            setEnemyData();
            regeneratePreviews();
            StartCoroutine(InitializeBattle());
        } 
        else 
        {
            switch (LevelManager.currentLevel)
            {
                case 2:
                    StartCoroutine(InitializeBattleTutorial2());
                    break;
                default:
                    Debug.LogError("SHouldn't be in here " + LevelManager.currentLevel);
                    break;
            }

        }
        
    }

    public void SkipTutorialButton()
    {
        StartCoroutine(SkipTutorial());
    }

    IEnumerator SkipTutorial()
    {
        forcedUnitPlacementTile = new Vector3Int(0, 0, -1);
        forcedUnitMovementTile = new Vector3Int(0, 0, -1);
        pushDialogueAfterEnemyTurn = false;
        pushDialogueAfterAttack = false;
        pushDialogueAfterBattleEnd = false;
        isPlacingUnit = false;
        if (unitToPlace)
        {
            Destroy(unitToPlace.gameObject);
        }
        if (dialogueManager)
        {
            Destroy(dialogueManager.speechPanel);
        }
        unitToPlace = null;

        LevelManager.instance.SetLevelCounter(LevelManager.instance.NumTutorialLevels);

        postProcessingSettings.DisableTheGlow(playerUnits);

        foreach (PlayerUnit player in playerUnits)
        {
            StartCoroutine(player.Undim());
        }

        yield return StartCoroutine(tileManager.ShatterTiles(0));

        LevelManager.instance.IncrementLevel();
        ResetAll();


        acceptingInput = false;

        enemyUnits.Clear();
        playerUnits.Clear();
        NPCUnits.Clear();

        yield return StartCoroutine(ui.SwitchScene(LevelManager.instance.NumTutorialLevels + 2));
        Debug.Log("Next level finished loading");

        LevelManager.instance.PrepareNextBattle();

        foreach (Unit unit in unitsToSpawn)
        {
            unit.Hide();
        }

        yield return null; // Need to wait a frame for the new level to load

        // Should refactor code so we don't need to find the tileManager. Should be returned by the level changing function
        tileManager = FindObjectOfType<TileManager>();
        // This is probably also unnecessary if we structure things better
        ui = FindObjectOfType<UIController>();
        tutorialManager = FindObjectOfType<TutorialManager>();
        LevelManager.instance.map = FindObjectOfType<Tilemap>();
        Debug.Log("Found new TileManager: " + tileManager != null);

        // If statement below destroys everything if we reach the win screen
        // Should probably be handled more elegantly
        if (tileManager == null || ui == null)
        {
            LevelManager.instance.RefreshNewGame();
            Debug.Log("No TileManager or UIController found. Destroying GameManager");
            foreach (Unit unit in unitsToSpawn)
            {
                Destroy(unit.gameObject);
            }

            Destroy(gameObject);
            yield break;
        }
        if (!LevelManager.instance.isTutorial)
        {
            Debug.Log("SETTING DATA");
            setEnemyData();
            regeneratePreviews();
            StartCoroutine(InitializeBattle());
        } 
        else 
        {
            switch (LevelManager.currentLevel)
            {
                case 2:
                    StartCoroutine(InitializeBattleTutorial2());
                    break;
                default:
                    Debug.LogError("SHouldn't be in here");
                    break;
            }

        }        
    }

    
    IEnumerator performEnemyMoves()
    {
        postProcessingSettings.DisableTheGlow(playerUnits);
        foreach (PlayerUnit unit in playerUnits)
        {
            unit.decreaseCoolDown();
            StartCoroutine(unit.Undim());
        }

        EnemyUnit[] enemies = enemyUnits.ToArray();

        yield return StartCoroutine(ui.ShowEnemyTurnAnim());

        foreach(EnemyUnit enemy in enemies)
        {
            postProcessingSettings.EnableEnemyGlow(enemy);
            yield return enemy.performAction(GetState());
            yield return StartCoroutine(UpdateBattleState());
            postProcessingSettings.DisableEnemyGlow(enemy);
            //CheckIfBattleOver();
            //if(isBattleOver)
            //{
            //    yield break;
            //}
        }

        if (pushDialogueAfterEnemyTurn)
        {
            if (dialogueManager.isWaitingForUserInput)
            {
                dialogueManager.doSkipDialogue = true;
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
                tileManager.SetTileDeadUnit(unit.location, unit);
                animations.Add(StartCoroutine(KillUnit(unit)));
            }
        }

        foreach (Unit unit in enemyUnits.ToArray())
        {
            if (unit.isDead)
            {
                tileManager.SetTileDeadUnit(unit.location, unit);
                animations.Add(StartCoroutine(KillUnit(unit)));
            }
        }

        foreach (Unit unit in NPCUnits.ToArray())
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

        yield return StartCoroutine(ui.ShowPlayerTurnAnim());

        StartCoroutine(ui.EnableEndTurnButton());
        isPlayerTurn = true;

        postProcessingSettings.ShowTheGlow(playerUnits);
        yield break;
    }

    private IEnumerator HandlePlacingClicks(Vector3Int tilePos, Unit curUnit)
    {
        if (tileManager.InBounds(tilePos) && curUnit == null && unitToPlace)
        {
            if (forcedUnitPlacementTile.z == 0 && tilePos != forcedUnitPlacementTile)
            {
                yield break;
            }
            if (!tileSelected || !tilePos.Equals(selectedTile))
            {
                SelectTile(tilePos);
                yield break;
            }
            DeselectTile();
            Debug.Log("Unit placement location: " + tilePos);
            PlayerUnit unit = unitToPlace;
            unitToPlace = null;
            unit.location = tilePos;
            yield return StartCoroutine(SpawnUnit(tilePos, unit));
            unitsToSpawn.Remove(unit);
            isPlacingUnit = false;

            if (dialogueManager)
            {
                dialogueManager.doSkipDialogue = true;
            }
        }
    }

    private IEnumerator MoveUnit(Unit unit, Vector3Int tilePos)
    {
        tileManager.ClearHighlights();
        yield return StartCoroutine(unit.DoMovement(state, tilePos));
        yield return StartCoroutine(UpdateBattleState());
        if (unit is PlayerUnit)
            postProcessingSettings.CanAttackGlow((PlayerUnit)unit);

        if (forcedUnitMovementTile.z == 0)
        {
            dialogueManager.doSkipDialogue = true;
        }
    }

    /// <summary>
    /// Displays the given tiles details in the info window
    /// </summary>
    /// <param name="tilePos">the position of the tile to display</param>
    public void DisplayTile(Vector3Int tilePos)
    {
        ui.ShowTileInWindow(tileManager.GetTileData(tilePos));
    }

    /// <summary>
    /// Stores the given tile position in the selectedTiles variable.
    /// Sets tileSelected to true.
    /// </summary>
    /// <param name="tilePos">the tile to select.</param>
    public void SelectTile(Vector3Int tilePos)
    {
        DeselectTile();
        selectedTilePrevColor = tileManager.GetTileColor(tilePos);
        tileManager.SetTileColor(tilePos, Color.yellow);
        highlightedTiles.Add((tilePos, selectedTilePrevColor));
        selectedTile = tilePos;
        tileSelected = true;
    }

    /// <summary>
    /// Deselects any tiles.
    /// Sets tileSelected to false.
    /// </summary>
    public void DeselectTile()
    {
        if (tileSelected)
        {
            tileSelected = false;
            foreach (var item in highlightedTiles)
            {
                tileManager.SetTileColor(item.Item1, item.Item2);
            }
            highlightedTiles.Clear();
        }
    }

    /// <summary>
    /// Moves the tile outline object over the given tile.
    /// Creates an instance of the outline if necessary
    /// </summary>
    /// <param name="tilePos">the position for the tile outline</param>
    public void OutlineTile(Vector3Int tilePos)
    {
        if (tileOutlinePrefab)
        {
            if (tileManager.InBounds(tilePos))
            {
                if (!tileOutline)
                {
                    tileOutline = Instantiate(tileOutlinePrefab);
                }
                tileOutline.SetActive(true);
                tileOutline.transform.position = tileManager.CellToWorldPosition(tilePos);
            }
            else
            {
                if (tileOutline)
                {
                    tileOutline.SetActive(false);
                }
            }
        }        
    }

    public void SelectUnit(Unit unit)
    {
        tileManager.ClearHighlights();
        postProcessingSettings.ChangeAllColorToDeSelected(playerUnits);
        if (selectedUnit is PlayerUnit)
        {
            postProcessingSettings.ChangeColorToDeSelected((PlayerUnit)selectedUnit);
        }
        selectedUnit = unit;
        ui.ShowUnitInfoWindow(unit);
        if(unit is PlayerUnit player && isPlayerTurn)
        {
            postProcessingSettings.ChangeColorToSelected((PlayerUnit)unit);
            if (!player.hasMoved)
            {
                ShowUnitMoveRange(player);
                if (forcedUnitMovementTile.z == 0)
                {
                    tileManager.SetTileColor(forcedUnitMovementTile, Color.red);
                }
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
        DeselectTile();
        ui.HideUnitInfoWindow();
        tileManager.ClearHighlights();
        if (selectedUnit is PlayerUnit)
            postProcessingSettings.ChangeColorToDeSelected((PlayerUnit)selectedUnit);
        selectedUnit = null;
        usingAbility = false;
    }

    public void ResetAll()
    {
        foreach (PlayerUnit unit in playerUnits)
        {
            Debug.Log("Destroying: " + unit);
            Destroy(unit.gameObject);
        }
        foreach (EnemyUnit unit in enemyUnits)
        {
            Destroy(unit.gameObject);
        }
        foreach(NPCUnit unit in NPCUnits)
        {
            Destroy(unit.gameObject);
        }
        playerUnits.Clear();
        enemyUnits.Clear();
        NPCUnits.Clear();
        unitsToSpawn.Clear();
    }

    private void ShowUnitMoveRange(PlayerUnit unit)
    {
        Debug.Log("Show move range");
        tileManager.ClearHighlights();
        if(isPlayerTurn)
        {
            tileManager.HighlightPath(unit.GetTilesInMoveRange(), Color.blue);
            if(!unit.hasAttacked)
            {
                foreach (EnemyUnit enemy in enemyUnits)
                {
                    Vector3Int dummy;
                    if (unit.IsTileInThreatRange(enemy.location, out dummy))
                    {
                        tileManager.SetTileColor(enemy.location, Color.red);
                    }
                }
            }
        }
    }

    private void ShowUnitAttackRange(Unit unit)
    {
        Debug.Log("Show attack range");
        tileManager.ClearHighlights();
        if(isPlayerTurn)
        {
            tileManager.HighlightPath(unit.GetTilesInAttackRange(), Color.red);
        }
    }

    private void ShowUnitThreatRange(Unit unit)
    {
        tileManager.ClearHighlights();
        if (isPlayerTurn)
        {
            tileManager.HighlightPath(unit.GetTilesInThreatRange(), Color.red);
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
            Debug.LogError("Can't find file");
        }
    }

    private void OnDestroy()
    {
        if (tileOutline)
        {
            Destroy(tileOutline);
        }
    }



}
