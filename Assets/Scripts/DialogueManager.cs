using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject[] speechPanels;
    public TextMeshProUGUI[] speakers;
    public string speakerString;
    public TextMeshProUGUI[] speechTexts;
    public string speechString;
    public bool isSpeaking {get {return speakingFxn != null;}}
    public Image portrait;
    private Coroutine speakingFxn;
    private int index = 0;
    [HideInInspector] public bool isWaitingForUserInput = false;
    [HideInInspector] public bool doSkipDialogue = false;
    public Button[] continueButtons;
    public bool continueDisabled;
    public Sprite[] portraits;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < speechPanels.Length; i++)
        {
            speakers[i].text = "";
            speechTexts[i].text = "";
        }

    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Space) && !continueDisabled)
        {
            isWaitingForUserInput = false;
        }
    }

    /// <summary>
    /// Takes a textString and outputs it into the dialogue box. If additive is true, original
    /// text is box remains there. Grabs speaker information from textString. textString should be
    /// in form: speakerString: speechString
    /// If no speakerString is given, then function infers the speaker from past speaker.
    /// </summary>
    public IEnumerator Say(string textString, bool additive = false, float textSpeed = .015f, bool disableContinue = false, int bubble = 0)
    {
        // If we are adding and there is text currently being spoken prematurely end the coroutine.
        if (additive && isSpeaking)
        {
            StopSpeaking();
            speechTexts[bubble].text = speechString;
        }
        // Otherwise, we are trying to start another coroutine at the same time. Error.
        else if (isSpeaking)
        {
            Debug.LogError("Old dialogue still running");
            yield break;
        }

        // Split the speaker data from the speech data
        string[] parts = textString.Split(':');

        // No speaker data, infer
        if (parts.Length < 2)
        {
            if (speakers[bubble].text.Length <= 0)
            {
                speakerString = "System";
            }
            else
            {
                speakerString = speakers[bubble].text;
            }
            speechString = parts[0];
        }
        else
        {
            speakerString = parts[0];
            speechString = parts[1];
        }

        if (speakerString.Equals("System"))
        {
            portrait.gameObject.SetActive(false);            
        }
        else if (speakerString.Equals("Itzel"))
        {
            portrait.gameObject.SetActive(true);
        }
        else if (speakerString.Equals("Mori"))
        {
            portrait.gameObject.SetActive(true);
            portrait.gameObject.GetComponent<Image>().sprite = portraits[0];
        }
        else if (speakerString.Equals("Sozzy"))
        {
            portrait.gameObject.SetActive(true);
            portrait.gameObject.GetComponent<Image>().sprite = portraits[1];
        }
        else if (speakerString.Equals("Ovis"))
        {
            portrait.gameObject.SetActive(true);
            portrait.gameObject.GetComponent<Image>().sprite = portraits[2];
        }   
        else if (speakerString.Equals("Locke"))
        {
            portrait.gameObject.SetActive(true);
            portrait.gameObject.GetComponent<Image>().sprite = portraits[3];
        }

        speakingFxn = StartCoroutine(StartSpeaking(speakerString, speechString, additive, textSpeed, disableContinue, bubble));
        yield return speakingFxn;
    }

    /// <summary>
    /// Activates the speechPanel and prints out the text to it. Waits for user input after it finishes
    /// </summary>
    public IEnumerator StartSpeaking(string speakerString, string textString, bool additive, float textSpeed = .015f, bool disableContinue = false, int bubble = 0)
    {
        speechPanels[bubble].SetActive(true);
        for (int z = 0; z < speechPanels.Length; z++)
        {
            if (z != bubble)
            {
                speechPanels[z].SetActive(false);
            }
        }

        speakers[bubble].text = speakerString;
        
        if (!additive)
        {
            speechTexts[bubble].text = "";
        }

        isWaitingForUserInput = true;

        int i = 0;
        while (i < textString.Length)
        {
            speechTexts[bubble].text += textString[i];
            i++;
            yield return new WaitForSeconds(textSpeed);
            if (!isWaitingForUserInput || doSkipDialogue)
            {
                speechTexts[bubble].text = speechString; 
                break;
            }
        }

        if (disableContinue)
        {
            Debug.Log("HERE");
            continueButtons[bubble].gameObject.SetActive(false);
            Debug.Log(continueButtons[bubble].gameObject.activeSelf);
            continueDisabled = true;
        }
        
        isWaitingForUserInput = true;
        while (isWaitingForUserInput && !doSkipDialogue)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log(isWaitingForUserInput);
        Debug.Log(doSkipDialogue);
        Debug.Log(speechTexts[bubble].text);
        Debug.Log("FINISHED speaking1");

        doSkipDialogue = false;
        continueDisabled = false;
        continueButtons[bubble].gameObject.SetActive(true);
        StopSpeaking();
    }

    public void StopSpeaking()
    {
        if (isSpeaking)
        {
            StopCoroutine(speakingFxn);
        }
        speakingFxn = null;
    }

    public void HideDialogueWindow()
    {
        speechPanels[0].SetActive(false);
        speechPanels[1].SetActive(false);
    }

    public void ContinueButton()
    {
        isWaitingForUserInput = false;
    }
}
