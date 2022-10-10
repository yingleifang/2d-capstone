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
    public float textSpeed;
    public Image portrait;
    private Coroutine speakingFxn;
    private int index = 0;
    [HideInInspector] public bool isWaitingForUserInput = false;


    // Start is called before the first frame update
    void Start()
    {
        speaker.text = "";
        speechText.text = "";
    }

    /// <summary>
    /// Takes a textString and outputs it into the dialogue box. If additive is true, original
    /// text is box remains there. Grabs speaker information from textString. textString should be
    /// in form: speakerString: speechString
    /// If no speakerString is given, then function infers the speaker from past speaker.
    /// </summary>
    public IEnumerator Say(string textString, bool additive = false)
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

        speakingFxn = StartCoroutine(StartSpeaking(speakerString, speechString, additive));
        yield return speakingFxn;
    }

    /// <summary>
    /// Prematurely ends the current dialogue.
    /// </summary>
    public void SkipDialogue()
    {
        StopSpeaking();
        speechText.text = speechString;  
    }

    /// <summary>
    /// Activates the speechPanel and prints out the text to it. Waits for user input after it finishes
    /// </summary>
    public IEnumerator StartSpeaking(string speakerString, string textString, bool additive)
    {
        speechPanel.SetActive(true);
        speaker.text = speakerString;
        
        if (!additive)
        {
            speechText.text = "";
        }

        isWaitingForUserInput = false;

        foreach (char c in textString.ToCharArray())
        {
            speechText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isWaitingForUserInput = true;
        while (isWaitingForUserInput)
        {
            yield return new WaitForEndOfFrame();
        }

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