using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UIController : MonoBehaviour
{
    public GameObject blackSquare;
    public GameObject translucentRect;
    public List<GameObject> unitPrefabs;
    public GameObject selectedPrefab;
    private List<GameObject> selectedUnitPrefabs = new List<GameObject>();
    private List<GameObject> unitIcons = new List<GameObject>();
    private int numUnitsInSelection = 3;

    private void Awake() 
    {        
        unitIcons.Add(this.transform.Find("Units/Unit1").gameObject);
        unitIcons.Add(this.transform.Find("Units/Unit2").gameObject);
        unitIcons.Add(this.transform.Find("Units/Unit3").gameObject);
    }

    public IEnumerator SwitchScene(int fadeSpeed = 2)
    {
        Color goColor = blackSquare.GetComponent<Image>().color;
        float newA = 0;

        while (blackSquare.GetComponent<Image>().color.a < 1)
        {
            newA = goColor.a + (fadeSpeed * Time.deltaTime);
            goColor = new Color (goColor.r, goColor.g, goColor.b, newA);
            blackSquare.GetComponent<Image>().color = goColor;
            yield return null;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public IEnumerator SwitchScene(string sceneName, int fadeSpeed = 2)
    {
        Color goColor = blackSquare.GetComponent<Image>().color;
        float newA = 0;

        while (blackSquare.GetComponent<Image>().color.a < 1)
        {
            newA = goColor.a + (fadeSpeed * Time.deltaTime);
            Debug.Log(newA);
            goColor = new Color (goColor.r, goColor.g, goColor.b, newA);
            blackSquare.GetComponent<Image>().color = goColor;
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void StartButton()
    {
        StartCoroutine(SwitchScene());
    }

    public void PlayAgainButton()
    {
        StartCoroutine(SwitchScene("StartScreen"));
    }

    public void UnloadUnitSelection()
    {
        translucentRect.SetActive(false);

        foreach (GameObject icon in unitIcons)
        {
            icon.SetActive(false);
        }
    }

    public void InstantiateSelectedPrefab()
    {

    }

    public void LoadUnitSelection()
    {
        translucentRect.SetActive(true);
        int numNeeded = numUnitsInSelection;
        int numLeftInList = unitPrefabs.Count;

        //Randomly select a set of prefabs from the list to load
        foreach (GameObject prefab in unitPrefabs)
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
            unitIcons[i].SetActive(true);
            unitIcons[i].GetComponent<Image>().sprite = 
                        selectedUnitPrefabs[i].GetComponent<PlayerUnit>().spriteRenderer.sprite;
            unitIcons[i].GetComponent<PurchasableScript>().unitPrefab = selectedUnitPrefabs[i];
        }
    }
}
