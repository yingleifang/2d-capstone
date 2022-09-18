using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitInfoWindow : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI healthText, attackRangeText, movementSpeedText, cooldownText,
                    nameText, attackDamageText;
    [SerializeField]
    private Image portraitImage;

    public void ShowStats(Unit unit)
    {
        gameObject.SetActive(true);
        healthText.text = "Health: " + unit.currentHealth + "/" + unit.health;
        attackRangeText.text = "Attack Range: " + unit.attackRange;
        movementSpeedText.text = "Movement Speed: " + unit.movementSpeed;
        cooldownText.text = "Ability Charge: " + unit.coolDown;
        nameText.text = unit.characterName;
        attackDamageText.text = "Attack Damage: " + unit.attackDamage;
        portraitImage.sprite = unit.portrait;
    }

    public void HideStats()
    {
        gameObject.SetActive(false);
    }
}
