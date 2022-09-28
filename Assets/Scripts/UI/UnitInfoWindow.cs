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

    public void ShowStats(Unit unit, bool spawned = true)
    {
        gameObject.SetActive(true);
        if (spawned)
        {
            healthText.text = "Health: " + unit.currentHealth + "/" + unit.health;
        }
        else
        {
            healthText.text = "Health: " + unit.health;
        }  
        attackRangeText.text = "Attack Range: " + unit.attackRange;
        movementSpeedText.text = "Movement Speed: " + unit.movementSpeed;
        cooldownText.text = "Ability Cooldown: " + unit.coolDown;
        nameText.text = unit.characterName;
        attackDamageText.text = "Attack Damage: " + unit.attackDamage;
        portraitImage.sprite = unit.portrait;
    }

    /// <summary>
    /// Displays the given tile data
    /// </summary>
    /// <param name="tileData">the tile data to display. Does nothing if null</param>
    public void ShowTile(HexTileData tileData)
    {
        if(tileData == null)
        {
            return;
        }

        gameObject.SetActive(true);
        ClearText();
        nameText.text = tileData.tileData.tileName;
        cooldownText.text = tileData.tileData.description; // TODO: might want a dedicated description text field
        portraitImage.sprite = tileData.sprite;
        
    }

    /// <summary>
    /// Clears all text on the info window
    /// </summary>
    public void ClearText()
    {
        healthText.text = "";
        attackRangeText.text = "";
        movementSpeedText.text = "";
        cooldownText.text = "";
        nameText.text = "";
        attackDamageText.text = "";
    }

    public void HideStats()
    {
        gameObject.SetActive(false);
    }
}
