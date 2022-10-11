using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButtonScript : MonoBehaviour
{
    public Button button;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(BattleManager.instance.AbilityButton);
    }

    // Update is called once per frame
    void Update()
    {
        if (!BattleManager.instance.selectedUnit || BattleManager.instance.selectedUnit is EnemyUnit)
        {
            return;
        }

        PlayerUnit curUnit = (PlayerUnit) BattleManager.instance.selectedUnit;
        if (curUnit.currentCoolDown > 0 || curUnit.hasAttacked)
        {
            button.enabled = false;
        }
        else
        {
            button.enabled = true;
        }
    }
}
