using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitSelectionWindow : MonoBehaviour
{

    public List<PlayerUnit> unitPrefabs;
    private List<PlayerUnit> selectedUnitPrefabs = new List<PlayerUnit>();
    public List<PurchasableScript> unitIcons = new List<PurchasableScript>();
    public GameObject canvas;
    public GameObject dialogue;
    public DialogueManager dialogueManager;
    private UnitInfoWindow unitInfoWindow;
    private int numUnitsInSelection = 3;
    public PlayerUnit selectedUnit;
    public Button confirmButton;
    public Animator anim;
    public bool minimized = false;
    public TextMeshProUGUI minimizeButtonText;

    private void Awake()
    {
        unitInfoWindow = canvas.transform.Find("UnitInfoWindow").GetComponent<UnitInfoWindow>();
        StartCoroutine(Hide());
    }

    public IEnumerator Hide()
    {
        // Play hiding animation
        LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.1f);
        gameObject.SetActive(false);

        yield break;
    }

    public void LoadUnitSelection(bool random = true)
    {
        int numNeeded = numUnitsInSelection;
        int numLeftInList = unitPrefabs.Count;

        selectedUnitPrefabs.Clear();

        if (!random)
        {
            foreach (PlayerUnit prefab in unitPrefabs)
            {
                selectedUnitPrefabs.Add(prefab);
                if (selectedUnitPrefabs.Count == 3)
                {
                    break;
                }  
            }      
        }

        else
        {
            //Randomly select a set of prefabs from the list to load
            List<PlayerUnit> tempList = new List<PlayerUnit>(unitPrefabs);
            while (selectedUnitPrefabs.Count < numNeeded)
            {
                int index = Random.Range(0, tempList.Count);
                PlayerUnit unit = tempList[index];
                selectedUnitPrefabs.Add(unit);
                tempList.RemoveAt(index);
            }
        }

        for (int i = 0; i < unitIcons.Count; i++)
        {
            unitIcons[i].Initialize(selectedUnitPrefabs[i]);
        }
    }

    public void LoadTutorialUnitSelection()
    {
        int numNeeded = numUnitsInSelection;
        int numLeftInList = unitPrefabs.Count;

        selectedUnitPrefabs.Clear();


        foreach (PlayerUnit prefab in unitPrefabs)
        {
            if (prefab is Locke)
            {
                selectedUnitPrefabs.Add(prefab);
            }
        }      
        unitIcons[1].Initialize(selectedUnitPrefabs[0]);
    }

    public IEnumerator Show(bool random = true, bool tutorial = false)
    {
        Debug.Log("Preparing to show unit selection window");

        if (!tutorial)
        {
            LoadUnitSelection(random);
        }
        else
        {
            LoadTutorialUnitSelection();
        }
        
        // Play appearing animation
        LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.3f);
        gameObject.SetActive(true);
        yield break;
    }

    public void SelectUnit(PurchasableScript unit)
    {
        selectedUnit = unit.unitPrefab;
        confirmButton.interactable = true;

        if (dialogueManager)
        {
            dialogueManager.HideDialogueWindow();
        }
    }

    public void ConfirmUnit()
    {
        BattleManager.instance.SetUnitToPlace(selectedUnit);

        Debug.Log(LevelManager.currentLevel);
        Debug.Log(BattleManager.instance.dialogueManager);
        if (LevelManager.currentLevel == 1 && BattleManager.instance.dialogueManager)
        {
            Debug.Log("FALSED");
            if (dialogueManager.isWaitingForUserInput)
            {
                dialogueManager.doSkipDialogue = true;
            }
            unitInfoWindow.HideStats();
            StartCoroutine(Hide());
        }
        else
        {
            // Minimize selection
            ToggleMinimize();
        }
    }

    public void ToggleMinimize()
    {
        minimized = !minimized;
        anim.SetBool("Minimized", minimized);
        if (minimized)
        {
            minimizeButtonText.text = "v";
            BattleManager.instance.acceptingInput = true;
        } else
        {
            minimizeButtonText.text = "-";
            BattleManager.instance.UndoUnitToPlace();
        }
    }
}
