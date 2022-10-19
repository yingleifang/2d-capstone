using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitInfoWindow : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI healthText, attackRangeText, movementSpeedText, cooldownText,
                    nameText, attackDamageText, descriptionText;
    public Button abilityButton;
    [SerializeField]
    private Image portraitImage;

    public void ShowStats(Unit unit, bool spawned = true)
    {
        gameObject.SetActive(true);

        if (unit is NPCUnit)
        {
            healthText.text = unit.currentHealth + "/" + unit.health;
            if (unit.currentCoolDown == 0)
            {
                abilityButton.interactable = true;
                cooldownText.text = "Ability Cooldown: READY";
            }
            else
            {
                abilityButton.interactable = false;
                cooldownText.text = "Ability Cooldown: " + unit.currentCoolDown;
            }
            attackRangeText.text = unit.attackRange.ToString();
            movementSpeedText.text = unit.movementSpeed.ToString();
            nameText.text = unit.characterName;
            attackDamageText.text = unit.attackDamage.ToString();
            portraitImage.sprite = unit.portrait;    
        }



        if (spawned)
        {
            healthText.text = unit.currentHealth + "/" + unit.health;

            if (unit.currentCoolDown == 0)
            {
                abilityButton.interactable = true;
                cooldownText.text = "Ability Cooldown: READY";
            }
            else
            {
                abilityButton.interactable = false;
                cooldownText.text = "Ability Cooldown: " + unit.currentCoolDown;
            }    
        }
        else
        {
            healthText.text = "Health: " + unit.health;
            cooldownText.text = "Ability Cooldown: " + unit.coolDown;
        }  
        attackRangeText.text = unit.attackRange.ToString();
        movementSpeedText.text = unit.movementSpeed.ToString();
        nameText.text = unit.characterName;
        attackDamageText.text = unit.attackDamage.ToString();
        portraitImage.sprite = unit.portrait;

        if (unit is PlayerUnit player)
        {
            descriptionText.text = player.startOfBattleAbilityDescription;
        } else if (unit is EnemyUnit enemy)
        {
            descriptionText.text = enemy.characterDescription;
        }
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
