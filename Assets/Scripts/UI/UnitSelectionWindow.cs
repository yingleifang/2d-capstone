using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
            foreach (PlayerUnit prefab in unitPrefabs)
            {
                if (Random.Range(1, numLeftInList) <= numNeeded)
                {
                    selectedUnitPrefabs.Add(prefab);
                    numNeeded--;
                }
                numLeftInList--;
                if (numNeeded <= 0)
                {
                    break;
                }
            }
        }

        for (int i = 0; i < unitIcons.Count; i++)
        {
            unitIcons[i].Initialize(selectedUnitPrefabs[i]);
        }
    }

    public IEnumerator Show(bool random = true)
    {
        Debug.Log("Preparing to show unit selection window");

        LoadUnitSelection(random);
        
        // Play appearing animation
        LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.3f);
        gameObject.SetActive(true);
        yield break;
    }

    public void SelectUnit(PurchasableScript unit)
    {
        BattleManager.instance.SetUnitToPlace(unit.unitPrefab);

        Debug.Log(LevelManager.currentLevel);
        Debug.Log(BattleManager.instance.dialogueManager);
        if (LevelManager.currentLevel == 1 && BattleManager.instance.dialogueManager)
        {
            Debug.Log("FALSED");
            dialogueManager.doSkipDialogue = true;
        }

        unitInfoWindow.HideStats();
        StartCoroutine(Hide());
    }

    public void SelectUnitTutorial(PurchasableScript unit)
    {
        BattleManager.instance.SetUnitToPlace(unit.unitPrefab);
        dialogueManager.isWaitingForUserInput = false;
        unitInfoWindow.HideStats();
        StartCoroutine(Hide());
    }
}
