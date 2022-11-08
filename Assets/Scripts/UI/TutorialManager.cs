using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{

    public TextMeshProUGUI textComp;
    private static string[] lines1 = {
        "System: A unit selection window will appear at the start of every level. Left click Ovis to add him to your team.",
        "Before selecting a tile to place your unit on, be mindful of the various types of tiles on the map.",
        "Spike tiles (highlighted in red) will deal one damage to units who end their turn on them.",
        "Impassable tiles (highlighted in red) will prevent units from moving on them or being placed on them.",
        "To place a unit, left click on a valid tile twice. Place your unit on the blue highlighted hexagon.",
        "Itzel: Huh?! What happened? What is that thing? Did you fall in here too?",
        "System: Hovering over units and tiles will show their information.",
        "Left clicking an enemy will show its \"threat area\" in red. This is the range in which it can attack units (its movement speed + attack range).",
        "Left clicking an ally will select it, and show its movement range in blue. To unselect a unit click on it again.",
        "While an ally is selected, it can be moved by double left clicking on tiles in its movement range. Move your unit to the red hexagon.",
        "End your turn by clicking the \"End Turn\" button on the bottom right.",
        "The enemy attacked your unit, but had to walk onto the spike hazard tile to do so, taking one damage in the process.",
        "Attack the enemy by selecting your unit and double clicking the enemy unit.",
        "Units may move and attack or attack in place. A unit may not attack and move. After a unit has attacked, they will fade to indicate they have no more actions.",
        "There are no other possible moves left. End your turn.",
        "Itzel: What? What's going on?"
    };

    private static string[] lines2 = {
        "System: After all enemies are defeated, allies will drop to the next level while maintaining their position and health.",
        "Itzel: No... It can't end like this...",
        "System: Itzel has died from falling on the spike hazard tile.",
        "It is imperative to be mindful of hazards on the next stage.",
        "Tiles overlayed with orange (shown on the map) indicate that there will be an enemy on that tile on the NEXT stage.",
        "Tiles overlayed with black and yellow stripes (shown on the map) indicate that there will be a hazard on that tile on the NEXT stage.",
        "Tiles overlayed with red (shown on the map) indicate that there will be an impassable tile there on the NEXT stage.",
        "Allies and enemies which drop onto these tiles during the level switch will DIE. Be particularly mindful of these tiles.",
        "From now on, a next battle preview will be overlayed on the map during battle.",
        "Ovis just used his start of battle ability.",
        "All ally units have a start of battle ability which they will use after they fall down to the next level.",
        "Ovis's start of battle ability does one damage to all units adjacent to him, ALLY or ENEMY.",
        "Units also have in battle abilities which can be used by selecting an ally unit and clicking the \"Ability\" button.",
        "Kill all the enemies and progress through the rest of the game."
    };

    private string[][] dialogue = {lines1, lines2};

    public float textSpeed;

    public GameObject unitSelection;
    public GameObject continueButton;
    public EndTurnButton endTurnButton;
    public GameObject battlePreviewButton;
   // public GameObject hazards;
    public GameObject help;
    public GameObject turnCounter;
    public LevelManager levelManager;
    public DialogueManager dialogueManager;
    public bool disableBattleInteraction = false;
    public int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (!LevelManager.instance.isTutorial)
        {
            Debug.LogError("Tutorial manager in a non-tutorial level");
            return;
        }

        turnCounter.SetActive(false);
       // hazards.SetActive(false);
        unitSelection.SetActive(false);
        endTurnButton.SetInteractable(false);
        battlePreviewButton.SetActive(false);
        index = 0;
        disableBattleInteraction = false;
        
    }

   // public void Continue()
   // {
      //  if (textComp.text == lines[index])
       // {
            
      //      NextLine();
      //  }
      //  else
      //  {
      //      StopAllCoroutines();
      //      textComp.text = lines[index];
       // }

    //}

    public IEnumerator NextDialogue(bool disableContinue = false)
    {
        Debug.Log(index);
        if (index > dialogue[LevelManager.currentLevel - 1].Length - 1)
        {
            Debug.Log("BREAKING <.<");
            yield break;
        }
        yield return StartCoroutine(dialogueManager.Say(dialogue[LevelManager.currentLevel - 1][index++], false, textSpeed, disableContinue));
    }

    public int NumLines()
    {
        return dialogue[LevelManager.currentLevel - 1].Length;
    }
}
