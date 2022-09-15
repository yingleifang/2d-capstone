using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionWindow : MonoBehaviour
{

    public List<PlayerUnit> unitPrefabs;
    private List<PlayerUnit> selectedUnitPrefabs = new List<PlayerUnit>();
    public List<PurchasableScript> unitIcons = new List<PurchasableScript>();
    private int numUnitsInSelection = 3;

    public IEnumerator Hide()
    {
        // Play hiding animation
        gameObject.SetActive(false);

        yield break;
    }

    public void LoadUnitSelection()
    {
        int numNeeded = numUnitsInSelection;
        int numLeftInList = unitPrefabs.Count;

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

        for (int i = 0; i < unitIcons.Count; i++)
        {
            unitIcons[i].image.sprite = selectedUnitPrefabs[i].spriteRenderer.sprite;
            unitIcons[i].unitPrefab = selectedUnitPrefabs[i];
        }
    }

    public IEnumerator Show()
    {
        Debug.Log("Preparing to show unit selection window");
        LoadUnitSelection();
        // Play appearing animation
        gameObject.SetActive(true);
        yield break;
    }

    public void SelectUnit(PurchasableScript unit)
    {
        Debug.Log("MOUSEDOWN");
        BattleManager.instance.SetUnitToPlace(unit.unitPrefab);
        StartCoroutine(Hide());
    }
}
