using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{

    public TextMeshProUGUI textComp;
    [TextArea(5,10)]
    public string[] lines;
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
        if (!levelManager.isTutorial)
        {
            Debug.LogError("Tutorial manager in a non-tutorial level");
            return;
        }

        turnCounter.SetActive(false);
       // hazards.SetActive(false);
        unitSelection.SetActive(false);
        endTurnButton.SetActive(false);
        battlePreviewButton.SetActive(false);
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
        if (index >= lines.Length - 1)
        {
            yield break;
        }
        yield return StartCoroutine(dialogueManager.Say(lines[index++]));
    }
}
