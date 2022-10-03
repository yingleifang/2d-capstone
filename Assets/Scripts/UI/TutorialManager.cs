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

    private int index;

    // Start is called before the first frame update
    void Start()
    {
        unitSelection.SetActive(false);
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
