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
using UnityEngine.Rendering.Universal;

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
    private bool playerInput = false;       // Indicates a player's choice\

    public static bool isBossLevel = false;

    public GameObject previewLayer;
    public bool previewVisible = false;

    [HideInInspector]
    public static BattleManager instance;

    public GameObject tileOutlinePrefab;
    private GameObject tileOutline;


    public Vector3 mapPosition;

    public PostProcessingSettings postProcessingSettings;

    public bool gameIsPaused = false;

    public static int bossHealth = 6;

    //Tutorial stuff
    public Button ovisButton;
    public TutorialManager tutorialManager;
    public DialogueManager dialogueManager;
    public Vector3Int forcedUnitMovementTile = new Vector3Int(0, 0, -1);
    public bool pushDialogueAfterMove = false;
    public bool pushDialogueAfterEnemyTurn = false;
    public bool pushDialogueAfterAttack = false;
    public bool pushDialogueAfterBattleEnd = false;
    public bool disableAttack = false;

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

    /// <summary>
    /// Destroys the unit being placed.
    /// Called when un-minimizing the unit selection window.
    /// </summary>
    public void UndoUnitToPlace()
    {
        acceptingInput = false;
        if (unitToPlace)
        {
            Destroy(unitToPlace.gameObject);
        }
        DeselectTile();
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

    public static bool IsPointerOverGameObject()
    {
        // Check mouse
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        // Check touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            var touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        // For other clicks, we do not want to do anything if we are over an UI object.
        if (IsPointerOverGameObject() || (ui && ui.unitSelectionWindow && (ui.unitSelectionWindow.gameObject.activeSelf && !ui.unitSelectionWindow.minimized)))
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
                if (playerUnit.hasMoved)
                {
                    postProcessingSettings.DisableGlow(playerUnit);
                    yield return StartCoroutine(playerUnit.Dim());
                }
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
            if (isPlayerTurn && selectedUnit is PlayerUnit unit && !disableAttack)
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
                            Debug.Log("PUSH AFTER ATTACK");
                            dialogueManager.doSkipDialogue = true;
                        }
                        //Tutorial stuff
                        if (LevelManager.currentLevel == 1)
                        {
                            TileBase startTile = tileManager.map.GetTile(unit.location);
                            TileDataScriptableObject tile = tileManager.baseTileDatas[startTile];
                            if (tile.hazardous)
                            {
                                yield return StartCoroutine(dialogueManager.Say("System: Your unit took damage as it moved onto or attacked on a spike tile. Try to avoid this mistake in the future!"));
                                yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
                                Debug.Log("HEREY");
                            }
                        }
                        ui.ShowUnitInfoWindow(ui.unitWhoseWindowIsOpen);

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
                    ui.ShowUnitInfoWindow(ui.unitWhoseWindowIsOpen);
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
                TileBase startTile = tileManager.map.GetTile(tilePos);
                TileDataScriptableObject tile = tileManager.baseTileDatas[startTile];

                //Tutorial stuff
                if (LevelManager.currentLevel == 1)
                {
                    if (tile.hazardous)
                    {
                        yield return StartCoroutine(dialogueManager.Say("System: Your unit took damage as it moved onto or attacked on a spike tile. Try to avoid this mistake in the future!"));
                        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
                        Debug.Log("HEREY");
                    }
                }
                CheckIfBattleOver();
                if (!isBattleOver && unit && !unit.isDead && !unit.hasAttacked)
                {
                    ui.ShowUnitInfoWindow(ui.unitWhoseWindowIsOpen);
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
        } else
        {
            SelectUnit(selectedUnit);
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

        // Welcome message
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        //Advise user to watch for tiles
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        //Highlight hazardous and talk about them
        foreach (Vector3Int tileLocation in tileManager.dynamicTileDatas.Keys)
        {
            if (tileManager.IsHazardous(tileLocation))
            {
                tileManager.SetTileColor(tileLocation, Color.red);
            }
        }
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
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
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
        tileManager.ClearHighlights();

        // Place units waiting to be spawned on new map
        Debug.Log("Units to spawn: " + unitsToSpawn.Count);
        List<Coroutine> animations = new List<Coroutine>();
        foreach (Unit unit in unitsToSpawn.ToArray())
        {
            yield return StartCoroutine(SpawnUnit(unit.location, unit));
        }

        // NPC dialogue
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        // Handles unit selection tutorial
        StartCoroutine(ui.ShowSelectionWindow(false));
        tutorialManager.disableBattleInteraction = true;
        yield return StartCoroutine(tutorialManager.NextDialogue(false, 1));
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        if (!unitToPlace)
        {
            Debug.Log("HERE MAN");
            yield return StartCoroutine(tutorialManager.NextDialogue(false, 1));
            yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
        }
        else
        {
            tutorialManager.index++;
        }

        // Wait until user does what is asked. This is not the only thing stopping
        // progression. Dialogue system's isWaitingForUserInput also stops progression.
        // However, both are needed otherwise the system will break.
        while (ui.unitSelectionWindow.gameObject.activeSelf)
        {
            yield return new WaitForEndOfFrame();
        }

        unitToPlace.spriteRenderer.enabled = false;
        tutorialManager.disableBattleInteraction = false;
        disableAttack = true;

        //Tell user how to place unit
        unitToPlace.spriteRenderer.enabled = true;
        tutorialManager.disableBattleInteraction = false;        
        yield return StartCoroutine(tutorialManager.NextDialogue(true));
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        // Wait until user does what is asked.
        while (isPlacingUnit)
        {
            yield return new WaitForEndOfFrame();
        }

        tileManager.ClearHighlights();

        tutorialManager.disableBattleInteraction = true;

        yield return StartCoroutine(UpdateBattleState());

        animations.Clear();
        unitsToSpawn.Clear();

        if (playerUnits[0].currentHealth != playerUnits[0].health)
        {
            yield return StartCoroutine(dialogueManager.Say("System: Your unit took damage as it fell on a spike tile. Try to avoid this mistake in the future!"));
            yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
        }

        // Explain Ovis SOB ability
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        //NPC dialogue
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        //Discussing hovering
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        //prompt to click enemy unit
        tutorialManager.disableBattleInteraction = false;
        //Force impossible to click tile to prevent movement
        forcedUnitMovementTile = new Vector3Int(-1000, -1, 0);
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        // Discuss deselecting units
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        //prompt to click ally unit and move
        yield return StartCoroutine(StartOfPlayerTurn());
        pushDialogueAfterMove = true;
        pushDialogueAfterEnemyTurn = true;
        forcedUnitMovementTile.z = -1;
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
        pushDialogueAfterMove = false;
        Debug.Log("HERE::::");
        
        // Prompt to attack
        pushDialogueAfterAttack = true;
        disableAttack = false;
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
        pushDialogueAfterAttack = false;

        //Discussing attack mechanics
        Debug.Log("talk attack mechanics");
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        // Kill all enemy type beat
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        dialogueManager.HideDialogueWindow();
    }

    private IEnumerator InitializeBattleTutorial2()
    {
        isPlayerTurn = false;
        pushDialogueAfterEnemyTurn = false;
        // Done to delay coroutine to allow units to add themselves to unitsToSpawn
        yield return new WaitForFixedUpdate();

        // Discussing location and health being maintained
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

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
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        // Discussing why Itzel died
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        //Advising to be careful of next stage hazards
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        var curGeneratePreviews = Resources.FindObjectsOfTypeAll<generatePreviews>();
        if (!curGeneratePreviews[0])
        {
            Debug.LogError("Can't find preview layer q_q");
        }
        previewLayer = curGeneratePreviews[0].gameObject;

        //Ensure three neutral tiles to place enemies on
        LevelManager.instance.nextSceneTileInfo.Remove(new Vector3Int(0, 1, 0));
        LevelManager.instance.nextSceneTileInfo.Add(new Vector3Int(0, 1, 0), 
                    (LevelManager.instance.typesOfTilesToSpawn[2], 0));
        LevelManager.instance.nextSceneTileInfo.Remove(new Vector3Int(1, 1, 0));
        LevelManager.instance.nextSceneTileInfo.Add(new Vector3Int(1, 1, 0), 
                    (LevelManager.instance.typesOfTilesToSpawn[2], 0));
        LevelManager.instance.nextSceneTileInfo.Remove(new Vector3Int(2, 1, 0));
        LevelManager.instance.nextSceneTileInfo.Add(new Vector3Int(2, 1, 0), 
                    (LevelManager.instance.typesOfTilesToSpawn[2], 0));
        
        //Ensure no hazard tile under target enemy on this stage.
        LevelManager.instance.nextSceneTileInfo.Remove(new Vector3Int(4, 0, 0));
        LevelManager.instance.nextSceneTileInfo.Add(new Vector3Int(4, 0, 0), 
                    (LevelManager.instance.typesOfTilesToSpawn[2], 0));
        // Manually set enemy positions
        for (int i = 0; i < LevelManager.instance.nextSceneEnemyInfo.Count; i++)
        {
            var curInfo = LevelManager.instance.nextSceneEnemyInfo[i];
            curInfo.Item2 = new Vector3Int(i, 1, 0);
            LevelManager.instance.nextSceneEnemyInfo[i] = curInfo;
        }

        //Make middle tile impassable
        LevelManager.instance.nextSceneTileInfo.Remove(new Vector3Int(0, 0, 0));
        LevelManager.instance.nextSceneTileInfo.Add(new Vector3Int(0, 0, 0), 
                    (LevelManager.instance.typesOfTilesToSpawn[6], 0));

        //Make this tile hazardous
        LevelManager.instance.nextSceneTileInfo.Remove(new Vector3Int(-1, 0, 0));
        LevelManager.instance.nextSceneTileInfo.Add(new Vector3Int(-1, 0, 0), 
                    (LevelManager.instance.typesOfTilesToSpawn[3], 0));

        TurnOnPreview();

        //Talk about enemy overlay
        curGeneratePreviews[0].ShowEnemyPreview(LevelManager.instance.nextSceneEnemyInfo, GetState());
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
        
        // Disable enemy overview temporarily to talk about other ones
        foreach (Transform child in previewLayer.transform)
        {
            child.gameObject.SetActive(false);
        }

        // Talk about hazard overlay
        curGeneratePreviews[0].ShowHazardPreview(LevelManager.instance.nextSceneTileInfo, GetState());
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        // Disable hazard overview temporarily to talk about other ones
        foreach (Transform child in previewLayer.transform)
        {
            child.gameObject.SetActive(false);
        }

        //Talk about impassable overlay
        curGeneratePreviews[0].ShowImpassablePreview(LevelManager.instance.nextSceneTileInfo, GetState());
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        // Emphasize the effect of units falling on impassable tiles
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        // Talk about the overlay being active for the rest of the game
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

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
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        // Talk about in battle abilities
        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

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
        yield return StartCoroutine(ui.ShowSelectionWindow(false, true));

        yield return StartCoroutine(tutorialManager.NextDialogue(true, 1));
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());

        yield return new WaitUntil(() => !isPlacingUnit);
        dialogueManager.HideDialogueWindow();
    
        CheckIfBattleOver();

        yield return StartCoroutine(StartOfPlayerTurn());

        foreach (PlayerUnit unit in playerUnits)
        {
            int numHealthRestore = unit.health - unit.currentHealth;
            yield return StartCoroutine(unit.SpawnStatNumber("<sprite=\"heart\" name=\"heart\">", numHealthRestore, Color.green));
            unit.ChangeHealth(numHealthRestore);
            unit.currentCoolDown = 0;
        }
        foreach (EnemyUnit unit in enemyUnits)
        {
            int numHealthRestore = unit.health - unit.currentHealth;
            yield return StartCoroutine(unit.SpawnStatNumber("<sprite=\"heart\" name=\"heart\">", numHealthRestore, Color.green));
            unit.ChangeHealth(numHealthRestore);
            unit.currentCoolDown = 0;
        }

        tutorialManager.endTurnButton.SetInteractable(true);
        StartCoroutine(ui.EnableEndTurnButton());

        isBattleOver = false;
        if(selectedUnit && selectedUnit is PlayerUnit)
        {
            postProcessingSettings.ChangeColorToDeSelected((PlayerUnit)selectedUnit);
        }
        selectedUnit = null;
        isPlayerTurn = true;

        yield return StartCoroutine(tutorialManager.NextDialogue());
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());


        Debug.Log("LAST");
        yield return StartCoroutine(tutorialManager.NextDialogue(true));
        yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());     
    }

    private IEnumerator InitializeBattle()
    {
        TurnOnPreview();
        if (!isBossLevel)
        {
            ui.InitializeTurnCount(turnsPerBattle);
        }
        else
        {
            ui.InitializeTurnCount(TurnCountDown.totalHagfish);
        }
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

        if (isBossLevel)
        {
            foreach (var curInfo in LevelManager.instance.enemyInfo)
            {
                HagfishEnemy hagfish = (HagfishEnemy)tileManager.GetUnit(curInfo.Item2);
                //Animator animator = hagfish.GetComponent<Animator>();
                //animator.runtimeAnimatorController = hagfish.animatorController;
                //hagfish
                yield return StartCoroutine(hagfish.AppearAt(state, hagfish.location, true));
            }

            //foreach (var unit in playerUnits)
            //{
            //    yield return StartCoroutine(unit.RegenHealth());
            //}
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

            if (LevelManager.currentLevel == 1)
            {
                if (tileManager.IsHazardous(playerUnits[0].location) && !playerUnits[0].hasMoved)
                {
                    yield return StartCoroutine(dialogueManager.Say("System: Your unit took damage as it ended its turn on a spike tile. Try to avoid this mistake in the future!"));
                    yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
                }                
            }

            // Decrements turn counter as well
            if (!isBossLevel) {
                ui.DecrementTurnCount();
            }
            yield return StartCoroutine(performEnemyMoves());

            List<Unit> unitsToDestroy = new();
            var turnPast = ui.turnCountDown.totalTurn - ui.turnCountDown.currentTurn;
            if (turnPast == 1 && !tutorialManager)
            {
                tileManager.CrackTiles(turnPast);
            }
            else if (turnPast == 2 && !tutorialManager){
                yield return StartCoroutine(tileManager.ShatterTiles(turnPast - 1, unitsToDestroy));
                tileManager.CrackTiles(turnPast);
            }
            else if (turnPast == 3 && !tutorialManager)
            {
                yield return StartCoroutine(tileManager.ShatterTiles(turnPast - 1, unitsToDestroy));
            }

            foreach(var unit in unitsToDestroy)
            {
                RemoveUnit(unit);
                Destroy(unit.gameObject);
            }

            if (LevelManager.currentLevel == 2)
            {
                Debug.Log("curretn turn counter: " + ui.turnCountDown.currentTurn);
                foreach (PlayerUnit unit in playerUnits)
                {
                    int numHealthRestore = unit.health - unit.currentHealth;
                    yield return StartCoroutine(unit.SpawnStatNumber("<sprite=\"heart\" name=\"heart\">", numHealthRestore, Color.green));
                    unit.ChangeHealth(numHealthRestore);
                    unit.currentCoolDown = 0;
                }
                foreach (EnemyUnit unit in enemyUnits)
                {
                    int numHealthRestore = unit.health - unit.currentHealth;
                    yield return StartCoroutine(unit.SpawnStatNumber("<sprite=\"heart\" name=\"heart\">", numHealthRestore, Color.green));
                    unit.ChangeHealth(numHealthRestore);
                    unit.currentCoolDown = 0;
                }

                if (ui.turnCountDown.currentTurn == 2)
                {
                    dialogueManager.doSkipDialogue = true;
                    yield return StartCoroutine(dialogueManager.Say("System: You can use abilities by clicking a unit, clicking the \"Ability\" tab, clicking the \"Use Ability\" button, " +
                                "and double clicking an enemy in range. Locke's ability deals damage based on how far he's moved that turn.", default,  true));
                    yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());                 
                }
                
                if (ui.turnCountDown.currentTurn == 1)
                {
                    dialogueManager.doSkipDialogue = true;
                    yield return StartCoroutine(dialogueManager.Say("System: Combine Locke's ability with Ovis's basic attack. " +
                                    "Move Locke three spaces away from the enemy. End your turn. On the next turn, move Locke in range and use his ability along with Ovis's attack", default, true));
                    yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
                }
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

    public IEnumerator SpawnUnit(Vector3Int location, Unit unit, bool addToUnitList = true, bool inBattle = false)
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
            yield return StartCoroutine(unit.AppearAt(GetState(), spawnLocation, inBattle));
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
            yield return StartCoroutine(unit.AppearAt(GetState(), spawnLocation, inBattle));
        }

        tileManager.AddUnitToTile(spawnLocation, unit);
        Debug.Log("TESTING ITZEL: " + spawnLocation + " " + unit.location + " " + unit.transform.position);
        

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
        GameObject grave = null;
        Vector3Int spawnLocation = Vector3Int.zero;
        if (unit is PlayerUnit player)
        {
            grave = player.gravePrefab;
            spawnLocation = player.location;
        }
        RemoveUnit(unit);
        yield return unit.StartDeath();
        if (grave != null && !tileManager.IsImpassableTile(spawnLocation))
        {
            GameObject instance = Instantiate(grave, tileManager.CellToWorldPosition(spawnLocation), Quaternion.identity);
            tileManager.ClearTileDecoration(spawnLocation);
            tileManager.SetTileDecoration(spawnLocation, instance);
        }
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
        if (selectedUnit == unit)
        {
            selectedUnit = null;
        }
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
        else if (ui.isOutOfTurns() && LevelManager.currentLevel != 2 && !isBossLevel)
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
        if (dialogueManager && tutorialManager)
        {
            yield return StartCoroutine(dialogueManager.Say("System: When you run out of ally units, you lose the game. Don't let it get you down. Try again with your new knowledge!"));
            yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
        }
        yield return StartCoroutine(ui.SwitchScene("GameOverScreen"));
        foreach (EnemyUnit unit in enemyUnits.ToArray())
        {

            Destroy(unit.gameObject);
        }
        Destroy(gameObject); // Or similarly reset the battle manager
    }

    /// <summary>
    /// Invokes the NextLevel coroutine.
    /// Easy callback for other scripts
    /// </summary>
    public void StartNextLevel()
    {
        StartCoroutine(NextLevel());
    }

    IEnumerator NextLevel()
    {
        postProcessingSettings.DisableTheGlow(playerUnits);
        if (pushDialogueAfterBattleEnd)
        {
            yield return StartCoroutine(dialogueManager.Say("Itzel: What? What's going on?"));
            yield return StartCoroutine(dialogueManager.WaitToFinishSpeaking());
            pushDialogueAfterBattleEnd = false;
        }

        foreach (PlayerUnit player in playerUnits)
        {
            StartCoroutine(player.Undim());
        }

        instance.TurnOffPreview();
        if (isBossLevel == true)
        {
            BossController boss = FindObjectOfType<BossController>();
            yield return StartCoroutine(boss.SlamAnimation());
        }
        yield return StartCoroutine(tileManager.ShatterTiles(0));

        unitsToSpawn.AddRange(playerUnits);
        unitsToSpawn.AddRange(NPCUnits);
        int index = SceneManager.GetActiveScene().buildIndex;
        if (LevelManager.instance.isTutorial && tutorialManager)
        {
            index += 1;
            unitsToSpawn.AddRange(enemyUnits);
        }
        else
        {
            Debug.Log("current level: " + LevelManager.currentLevel);

            if (LevelManager.currentLevel < LevelManager.instance.totalLevels - LevelManager.instance.BossLevels)
            {
                unitsToSpawn.AddRange(enemyUnits);
            }
            else if (LevelManager.currentLevel == LevelManager.instance.totalLevels - LevelManager.instance.BossLevels)
            {
                index += 1;
                isBossLevel = true;
            }else if (LevelManager.currentLevel + 1 <= LevelManager.instance.totalLevels)
            {
                isBossLevel = true;
            }
            else
            {
                index = 7;
                ResetAll();
                foreach (EnemyUnit unit in enemyUnits.ToArray())
                {
                    Destroy(unit.gameObject);
                }
            }
            Debug.Log("index: " + index);
        }


        LevelManager.instance.IncrementLevel();

        acceptingInput = false;

        enemyUnits.Clear();
        playerUnits.Clear();
        NPCUnits.Clear();

        LevelManager.instance.PrepareNextBattle();

        yield return StartCoroutine(ui.SwitchScene(index));

        if (index == 7)
        {
            Debug.Log("Destroying battlemanager because of win screen");
            Destroy(gameObject);
        }

        Debug.Log("Next level finished loading");

        Debug.Log("KILL ME: " + LevelManager.instance.levelOffset + LevelManager.currentLevel);
        

        foreach (Unit unit in unitsToSpawn)
        {
            unit.Hide();
        }

        yield return null; // Need to wait a frame for the new level to load

        tileManager = FindObjectOfType<TileManager>();
        dialogueManager = FindObjectOfType<DialogueManager>();

        // In dialogue scene
        if (dialogueManager != null && tileManager == null)
        {
            CampfireDialogueManager cmdManager = FindObjectOfType<CampfireDialogueManager>();
            ui = FindObjectOfType<UIController>();
            Debug.Log("Dialogue scene");
            yield return StartCoroutine(cmdManager.SayDialogue());
            yield return StartCoroutine(ui.SwitchScene(SceneManager.GetActiveScene().buildIndex + 1));
            yield return null;
        }

        tileManager = FindObjectOfType<TileManager>();
        // Should refactor code so we don't need to find the tileManager. Should be returned by the level changing function
        tileManager.map = FindObjectOfType<Tilemap>();
        // This is probably also unnecessary if we structure things better
        dialogueManager = FindObjectOfType<DialogueManager>();
        ui = FindObjectOfType<UIController>();
        tutorialManager = FindObjectOfType<TutorialManager>();
        LevelManager.instance.map = FindObjectOfType<Tilemap>();
        Debug.Log("Found new TileManager: " + tileManager != null);

        // If statement below destroys everything if we reach the win screen
        // Should probably be handled more elegantly
        if (dialogueManager == null && (tileManager == null || ui == null))
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
            if (LevelManager.currentLevel != LevelManager.instance.totalLevels)
            {
                regeneratePreviews();
            }
            StartCoroutine(InitializeBattle());
            instance.TurnOnPreview();
        } 
        else if (LevelManager.currentLevel == 2)
        {
            StartCoroutine(InitializeBattleTutorial2());
            instance.TurnOnPreview();
        }
    }

    public void SkipTutorialButton()
    {
        StartCoroutine(SkipTutorial());
    }

    IEnumerator SkipTutorial()
    {
        forcedUnitMovementTile = new Vector3Int(0, 0, -1);
        pushDialogueAfterEnemyTurn = false;
        pushDialogueAfterMove = false;
        pushDialogueAfterAttack = false;
        disableAttack = false;
        pushDialogueAfterBattleEnd = false;
        isPlacingUnit = false;
        if (unitToPlace)
        {
            Destroy(unitToPlace.gameObject);
        }
        if (dialogueManager)
        {
            Destroy(dialogueManager.speechPanels[0]);
            Destroy(dialogueManager.speechPanels[1]);
        }
        unitToPlace = null;

        LevelManager.instance.SetLevelCounter(LevelManager.instance.NumTutorialLevels);

        postProcessingSettings.DisableTheGlow(playerUnits);

        foreach (PlayerUnit player in playerUnits)
        {
            StartCoroutine(player.Undim());
        }

        yield return StartCoroutine(tileManager.ShatterTiles(0));

        LevelManager.instance.levelOffset = 0;
        LevelManager.instance.LevelSetup();
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

        setEnemyData();
        regeneratePreviews();
        StartCoroutine(InitializeBattle());    
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
            if (enemy != null && !enemy.isDead)
            {
                postProcessingSettings.EnableEnemyGlow(enemy);
                yield return enemy.performAction(GetState());
                yield return StartCoroutine(UpdateBattleState());
                postProcessingSettings.DisableEnemyGlow(enemy);
                CheckIfBattleOver();
                if (isBattleOver)
                {
                    yield break;
                }
            }
        }

        if (isBossLevel && enemyUnits.Count < 6)
        {
            yield return SpawnEnemyInBattle();
        }

        if (pushDialogueAfterEnemyTurn)
        {
            if (dialogueManager.isWaitingForUserInput)
            {
                Debug.Log("PUSH AFTER ENEMY TURN");
                dialogueManager.doSkipDialogue = true;
                pushDialogueAfterEnemyTurn = false;
            }
        }

        yield return StartCoroutine(StartOfPlayerTurn());
        DeselectUnit();

        yield break;
    }

    private IEnumerator SpawnEnemyInBattle()
    {
        var location = LevelManager.instance.GetSpawnLocation();
        while (tileManager.GetUnit(location) != null)
        {
            location = LevelManager.instance.GetSpawnLocation();
        }
        var unitType = LevelManager.instance.GetSpawnUnit();
        Unit unit = Instantiate(unitType);
        yield return StartCoroutine(SpawnUnit(location, unit, true, true));
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
            if (!tileSelected || !tilePos.Equals(selectedTile))
            {
                SelectTile(tilePos);
                yield break;
            }
            DeselectTile();
            Debug.Log("Unit placement location: " + tilePos);
            PlayerUnit unit = unitToPlace;
            unit.location = tilePos;
            unitToPlace = null;
            ui.unitSelectionWindow.gameObject.SetActive(false);
            yield return StartCoroutine(SpawnUnit(tilePos, unit));
            unitsToSpawn.Remove(unit);
            isPlacingUnit = false;

            if (dialogueManager && LevelManager.currentLevel == 1)
            {
                Debug.Log("PUsh after placing");
                dialogueManager.doSkipDialogue = true;
            }
            Debug.Log(unitToPlace);
            yield return StartCoroutine(unit.StartOfBattleAbility(state));
            yield return StartCoroutine(UpdateBattleState());
        }
    }

    private IEnumerator MoveUnit(Unit unit, Vector3Int tilePos)
    {
        tileManager.ClearHighlights();
        yield return StartCoroutine(unit.DoMovement(state, tilePos));
        yield return StartCoroutine(UpdateBattleState());
        if (unit is PlayerUnit)
            postProcessingSettings.CanAttackGlow((PlayerUnit)unit);

        if (pushDialogueAfterMove)
        {
            Debug.Log("PUSH AFTER MOVING");
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
            Debug.Log("Destroying: " + unit);
            Destroy(unit.gameObject);
        }
        foreach(NPCUnit unit in NPCUnits)
        {
            Debug.Log("Destroying: " + unit);
            Destroy(unit.gameObject);
        }
        foreach (Unit unit in unitsToSpawn)
        {
            Debug.Log("Destroying: " + unit);
            if (unit)
            {
                Destroy(unit.gameObject); 
            }        
        }
        playerUnits.Clear();
        enemyUnits.Clear();
        NPCUnits.Clear();
        unitsToSpawn.Clear();
        isBossLevel = false;
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
