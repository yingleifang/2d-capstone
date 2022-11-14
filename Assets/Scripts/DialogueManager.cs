using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject speechPanel;
    public TextMeshProUGUI speaker;
    public string speakerString;
    public TextMeshProUGUI speechText;
    public string speechString;
    public bool isSpeaking {get {return speakingFxn != null;}}
    public Image portrait;
    private Coroutine speakingFxn;
    private int index = 0;
    [HideInInspector] public bool isWaitingForUserInput = false;
    [HideInInspector] public bool doSkipDialogue = false;
    public Button continueButton;
    public bool continueDisabled;
    public Sprite[] portraits;


    // Start is called before the first frame update
    void Start()
    {
        speaker.text = "";
        speechText.text = "";
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
    public IEnumerator Say(string textString, bool additive = false, float textSpeed = .015f, bool disableContinue = false)
    {
        // If we are adding and there is text currently being spoken prematurely end the coroutine.
        if (additive && isSpeaking)
        {
            StopSpeaking();
            speechText.text = speechString;
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
            if (speaker.text.Length <= 0)
            {
                speakerString = "System";
            }
            else
            {
                speakerString = speaker.text;
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

        speakingFxn = StartCoroutine(StartSpeaking(speakerString, speechString, additive, textSpeed, disableContinue));
        yield return speakingFxn;
    }

    /// <summary>
    /// Activates the speechPanel and prints out the text to it. Waits for user input after it finishes
    /// </summary>
    public IEnumerator StartSpeaking(string speakerString, string textString, bool additive, float textSpeed = .015f, bool disableContinue = false)
    {
        speechPanel.SetActive(true);
        speaker.text = speakerString;
        
        if (!additive)
        {
            speechText.text = "";
        }

        isWaitingForUserInput = true;

        int i = 0;
        while (i < textString.Length)
        {
            speechText.text += textString[i];
            i++;
            yield return new WaitForSeconds(textSpeed);
            if (!isWaitingForUserInput || doSkipDialogue)
            {
                speechText.text = speechString; 
                break;
            }
        }

        if (disableContinue)
        {
            continueButton.gameObject.SetActive(false);
            continueDisabled = true;
        }
        
        isWaitingForUserInput = true;
        while (isWaitingForUserInput && !doSkipDialogue)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log(speechText.text);
        Debug.Log("FINISHED speaking1");

        doSkipDialogue = false;
        continueDisabled = false;
        continueButton.gameObject.SetActive(true);
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
        speechPanel.SetActive(false);
    }

    public void ContinueButton()
    {
        isWaitingForUserInput = false;
    }
}
