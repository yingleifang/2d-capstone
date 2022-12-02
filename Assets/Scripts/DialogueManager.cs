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
    public Coroutine speakingFxn;
    private int index = 0;
    [HideInInspector] public bool isWaitingForUserInput = false;
    [HideInInspector] public bool doSkipDialogue = false;
    public Button[] continueButtons;
    public bool continueDisabled;
    public Sprite[] portraits;
    private Queue<string> textQueue;
    private Queue<int> bubbleQueue;
    private Queue<float> textSpeedQueue;
    private Queue<bool> disableContinueQueue;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < speechPanels.Length; i++)
        {
            speakers[i].text = "";
            speechTexts[i].text = "";
        }
        textQueue = new Queue<string>();
        bubbleQueue = new Queue<int>();
        textSpeedQueue = new Queue<float>();
        disableContinueQueue = new Queue<bool>();
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Space) && !continueDisabled)
        {
            isWaitingForUserInput = false;
        }

        if (textQueue.Count != 0 && !isSpeaking)
        {
            string textString = textQueue.Dequeue();
            int bubble = bubbleQueue.Dequeue();
            float textSpeed = textSpeedQueue.Dequeue();
            bool disableContinue = disableContinueQueue.Dequeue();

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
            speakingFxn = StartCoroutine(StartSpeaking(speakerString, speechString, textSpeed, disableContinue, bubble));
        }
    }

    /// <summary>
    /// Queues a textbox with the given characteristics to be said at the earliest available time.
    /// </summary>
    public IEnumerator Say(string textString, float textSpeed = .015f, bool disableContinue = false, int bubble = 0)
    {
        textQueue.Enqueue(textString);
        bubbleQueue.Enqueue(bubble);
        textSpeedQueue.Enqueue(textSpeed);
        disableContinueQueue.Enqueue(disableContinue);

        while (speakingFxn == null)
        {
            yield return new WaitForSeconds(.015f);
        }
    }

    /// <summary>
    /// Activates the speechPanel and prints out the text to it. Waits for user input after it finishes
    /// </summary>
    public IEnumerator StartSpeaking(string speakerString, string textString, float textSpeed = .015f, bool disableContinue = false, int bubble = 0)
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
        
        speechTexts[bubble].text = "";

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
        // Debug.Log(isWaitingForUserInput);
        // Debug.Log(doSkipDialogue);
        // Debug.Log(speechTexts[bubble].text);
        // Debug.Log("FINISHED speaking1");

        isWaitingForUserInput = false;
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

    public IEnumerator WaitToFinishSpeaking()
    {
        while (isSpeaking)
        {
            yield return new WaitForEndOfFrame();
        }
    }
}
