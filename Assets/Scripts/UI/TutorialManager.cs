using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{

    private static string[] lines1 = {
        "System: Welcome to the Abyss! During your exploration of the abyss, there are a few important ideas to keep in mind.",
        "First off, be mindful of the various types of tiles on the map.",
        "Spike tiles (highlighted in red) will deal one damage to units who move onto them, land on them, or end their turn on them.",
        "Impassable tiles (highlighted in red) will prevent units from moving on them or being placed on them.",
        "Itzel: Huh?! Where am I? What is that thing?",
        "System: Before the beginning of every level, you will be prompted to add a unit to your team. You can learn about a unit by clicking the \"Stats\", \"Passive\", and \"Ability\" buttons below its portrait.",
        "Choose Ovis by left clicking it and clicking the confirm button.",
        "To place a unit, left click on a valid tile twice.",
        "After a unit is placed, they will use their \"Passive\". Ovis (the character you selected) does one damage to all units around him, ally or foe.",
        "Itzel: Whoa! Are you okay?",
        "System: Hovering over units and tiles will show their information.",
        "Left clicking an enemy will show its \"threat area\" in red. This is the range in which it can attack units (its movement speed + attack range).",
        "Left clicking an ally will select it, and show its movement range in blue. To unselect a unit click on it again.",
        "While an ally is selected, it can be moved by double left clicking on tiles in its movement range.",
        "While an ally unit is selected, double left clicking on enemies within attack range will attack them. If a unit is on a spike tile when it attacks, it will take damage.",
        "After a unit attacks, they can no longer move and will fade to indicate they have no more actions.",
        "Kill all enemies and complete the level.",
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
        "Ovis just used his \"Passive\" which deals one damage to all units adjacent to him, ally or enemy.",
        "All ally units activate their start of battle ability after they fall or are placed.",
        "Units also have in battle abilities which can be used by selecting an ally unit, navigating to the ability tab, and clicking the \"Ability\" button.",
        "Add Locke to your team.",
        "The health of all units will regenerate at the end of the turn for specifically this level. Find a way to kill the enemy during the span of one turn. (Hint, use your abilities, they have no cooldowns this level)."
    };

    private string[][] dialogue = {lines1, lines2};

    public float textSpeed;

    public GameObject unitSelection;
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

    public IEnumerator NextDialogue(bool disableContinue = false, int bubble = 0)
    {
        Debug.Log(index);
        if (index > dialogue[LevelManager.currentLevel - 1].Length - 1)
        {
            Debug.Log("BREAKING <.<");
            yield break;
        }
        yield return StartCoroutine(dialogueManager.Say(dialogue[LevelManager.currentLevel - 1][index++], false, textSpeed, disableContinue, bubble));
    }

    public IEnumerator SpecificDialogue(string text, bool disableContinue = false, int bubble = 0)
    {
        yield return StartCoroutine(dialogueManager.Say(text, false, textSpeed, disableContinue, bubble));
    }

    public int NumLines()
    {
        return dialogue[LevelManager.currentLevel - 1].Length;
    }
}
