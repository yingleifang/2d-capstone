using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{

    public TextMeshProUGUI textComp;
    public string[] lines;
    public float textSpeed;

    public GameObject unitSelection;
    public GameObject continueButton;
    public GameObject endTurnButton;
    public GameObject battlePreviewButton;
    public GameObject sozyExample;
   // public GameObject hazards;
    public GameObject help;
    public GameObject turnCounter;

    private int index;

    // Start is called before the first frame update
    void Start()
    {
        help.SetActive(false);
        turnCounter.SetActive(false);
       // hazards.SetActive(false);
        unitSelection.SetActive(false);
        endTurnButton.SetActive(false);
        battlePreviewButton.SetActive(false);
        sozyExample.SetActive(false);
        textComp.text = string.Empty;
        StartDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        if (textComp.text == lines[index])
        {
            continueButton.SetActive(true);
        }
        if (textComp.text == lines[2])
        {
            unitSelection.SetActive(true);
        }
        if (textComp.text == lines[4])
        {
            unitSelection.SetActive(false);
            sozyExample.SetActive(true);
        }
        if (textComp.text == lines[8])
        {
            endTurnButton.SetActive(true);
        }
        if (textComp.text == lines[9])
        {
            battlePreviewButton.SetActive(true);
        }
        if (textComp.text == lines[11])
        {
            turnCounter.SetActive(true);
        }
        if (textComp.text == lines[12])
        {
            help.SetActive(true);
        }
        




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

    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());

    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComp.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    public void NextLine()
    {
        continueButton.SetActive(false);

        if(index < lines.Length - 1)
        {
            
            index ++;
            Debug.Log("line " + index);
            textComp.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            gameObject.SetActive(false);
            continueButton.SetActive(false);

        }
    }
}
