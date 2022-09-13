using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchasableScript : MonoBehaviour
{
    public GameObject unitPrefab;
    public TileManager tileManager;
    public BattleManager battleManager;
    public GameObject popText;
    // Start is called before the first frame update
    void Start()
    {
        tileManager = FindObjectOfType<TileManager>();
        battleManager = FindObjectOfType<BattleManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        
    }

    private void OnTriggerExit2D(Collider2D other) {
        
    }

    public void SelectUnit() 
    {
        Debug.Log("MOUSEDOWN");
        battleManager.isPlacingUnit = true;
        battleManager.ui.selectedPrefab = (GameObject)Instantiate(unitPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        Debug.Log("MOUSEDOWN");
        battleManager.ui.UnloadUnitSelection();
    }
}
