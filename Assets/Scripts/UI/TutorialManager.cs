using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{

    public TextMeshProUGUI textComp;
    private string[] lines = {
        "Itzel: Hey! Hey! Over here! Are you alright? You just fell down here right? Were you looking for the treasure too?",
        "No need to be so surprised. We're not the only fools who ended up down here searching for that fable.",
        "The treasure? That's the last thing you should be worrying about. You'll realize soon enough that it's better to get out of here as soon as you can.",
        "Wait! Wait! I'm telling the truth here. You have to be careful down here. There's... things. Monsters. It'll be better for the both of us if we stick together. What do you say?",
        "That's great! Two's better than one. What's your specialization? I'm a ranger.",
        "System: A unit selection window will appear at the start of every level. Unit information can be seen by hovering your mouse over the units in this window.",
        "You can select a unit from the window to add to your team by left clicking it. Select Ovis for now.",
        "Before selecting a tile to place your unit on, be mindful of the various types of tiles on the map.",
        "Spike tiles (highlighted in red) will deal one damage to units who end their turn on them.",
        "Impassable tiles (highlighted in red) will prevent units from moving on them or being placed on them.",
        "To place a unit you must left click on a valid tile twice, once to select, and again to confirm your selection. For now, place the unit on the blue highlighted square.",
        "Itzel: A warrior huh? And a sheepman too? Well, we all have our circumstances. As long as you can fight, I won't ask.",
        "There's an enemy ahead. I said two's better than one, but I'm not looking for deadweight. Show me what you can do.",
        "System: Left clicking units will show its unit information as well as other information.",
        "Clicking an enemy will show its \"threat area\" highlighted in red. This is the range in which it can attack units (its movement speed + attack range).",
        "Clicking an ally will highlight possible squares it can move to in blue.  Double click squares to move to them. For now, move your unit to the red square.",
        "There are no units to attack in range, end your turn by clicking the \"End Turn\" button on the bottom right.",
        "Attack the enemy by selecting your unit and double clicking the enemy unit.",
        "Units may move and attack or attack in place. A unit may not attack and move. After a unit has attacked, they will fade to indicate they have no more actions.",
        "There are no other possible moves left. End your turn.",
        "Itzel: What? What's going on?"
    };
    public float textSpeed;

    public GameObject unitSelection;
    public GameObject continueButton;
    public GameObject endTurnButton;
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
        endTurnButton.SetActive(false);
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

    public IEnumerator NextDialogue()
    {
        Debug.Log(index);
        if (index > lines.Length - 1)
        {
            yield break;
        }
        yield return StartCoroutine(dialogueManager.Say(lines[index++], false, textSpeed));
    }

    public int NumLines()
    {
        return lines.Length;
    }
}
