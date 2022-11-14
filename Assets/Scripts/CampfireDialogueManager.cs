using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CampfireDialogueManager : MonoBehaviour
{
    DialogueManager dialogueManager;
    UIController uiController;

    Dictionary<string, string[]> lines = new Dictionary<string, string[]>();
    List<string> finalLines = new List<string>(); 
    string[] lockeLines = {"Locke: Finally some time to rest! Even for me there's a point where it's all too much.",
                            "Locke: Hah! Now this is a real adventure. I can barely raise my paws from the exhaustion which grips me."};
    
    string[] sozzyLines = {"Sozzy: Let's rest a while. I don't think we're ready to move on yet...",
                            "Sozzy: Wha-What do you think is in there? Let's come up with a plan before doing anything rash."};

    string[] ovisLines = {"Ovis: After I've rested my hooves, let's get this done and finished with.",
                            "Ovis: I cannot yet leave this world. Let me gather my strength before we continue.",
                            "Ovis: I sense a formidable enemy ahead. Let us not falter before green meadows."};
    
    string[] moriLines = {"Mori: Accept the love of thine God. Sleep.",
                            "Mori: To reject the warm embrace of slumber is to reject the love of God itself.",
                            "Mori: I must prepare the tools of God."};

    // Start is called before the first frame update
    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        uiController = FindObjectOfType<UIController>();
        
        if (!dialogueManager)
        {
            Debug.LogError("No dialogue manager");
        }

        lines.Add("Locke", lockeLines);
        lines.Add("Sozzy", sozzyLines);
        lines.Add("Ovis", ovisLines);
        lines.Add("Mori", moriLines);

        List<string> aliveUnits = new List<string>();

        foreach (Unit unit in BattleManager.instance.unitsToSpawn)
        {
            if (unit is Sozzy)
            {
                if (!aliveUnits.Contains("Sozzy"))
                {
                    aliveUnits.Add("Sozzy");
                }
                unit.currentHealth = unit.health;
            }
            else if (unit is Locke playerUnit)
            {
                if (!aliveUnits.Contains("Locke"))
                {
                    aliveUnits.Add("Locke");
                }
                unit.currentHealth = unit.health;
            }
            else if (unit is Ovis)
            {
                if (!aliveUnits.Contains("Ovis"))
                {
                    aliveUnits.Add("Ovis");
                }
                unit.currentHealth = unit.health;
            }
            else if (unit is Mori)
            {
                if (!aliveUnits.Contains("Mori"))
                {
                    aliveUnits.Add("Mori");
                }
                unit.currentHealth = unit.health;
            }
        }


        if (aliveUnits.Count == 1 && BattleManager.instance.unitsToSpawn.Count == 1)
        {
            if (aliveUnits.Contains("Sozzy"))
            {
                finalLines.Add("Sozzy: I never should have gone here.");
            }
            else if (aliveUnits.Contains("Locke"))
            {
                finalLines.Add("Locke: This isn't how its supposed to be. Why... Why did it turn out like this?");
            }
            else if (aliveUnits.Contains("Ovis"))
            {
                finalLines.Add("Ovis: A fate akin to the others of my kind. I shall face it with honor.");
            }
            else if (aliveUnits.Contains("Mori"))
            {
                finalLines.Add("Mori: If this is God's will, then so be it.");
            }
        }

        else
        {
            // Choose random alive unit
            int index = Random.Range(0, aliveUnits.Count);
            // Get his lines
            string[] tempLines = lines[aliveUnits[index]];
            // Choose random line
            int subIndex = Random.Range(0, lines[aliveUnits[index]].Length);
            // Add line
            finalLines.Add(lines[aliveUnits[index]][subIndex]);
            //Remove unit from selection
            aliveUnits.RemoveAt(index);

            index = Random.Range(0, aliveUnits.Count);
            tempLines = lines[aliveUnits[index]];
            subIndex = Random.Range(0, lines[aliveUnits[index]].Length);
            finalLines.Add(lines[aliveUnits[index]][subIndex]);
        }

        finalLines.Add("System: The health of all your units has been fully restored");
    }

    public IEnumerator SayDialogue(bool disableContinue = false)
    {
        int index = 0;
        yield return StartCoroutine(dialogueManager.Say(finalLines[index++], false, .015f, disableContinue));

        if (finalLines.Count == 3)
        {
            yield return StartCoroutine(dialogueManager.Say(finalLines[index++], false, .015f, disableContinue));
        }
        yield return StartCoroutine(dialogueManager.Say(finalLines[index++], false, .015f, disableContinue));

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
