using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitInfoWindow : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI healthText, attackRangeText, movementSpeedText, cooldownText,
                    nameText, attackDamageText, passiveText, abilityText;
    public GameObject abilityTabButton, statPanel, passivePanel, abilityPanel;
    public Button statsTabButton;
    public Button abilityButton;
    [SerializeField]
    private Image portraitImage;
    private Unit displayedUnit;
    public VideoPlayerFix videoPlayer;

    public void ShowStats(Unit unit, bool spawned = true)
    {
        gameObject.SetActive(true);
        if (displayedUnit != unit)
        {
            ShowStatsTab();
            statsTabButton.Select();
        }
        displayedUnit = unit;

        if (unit is NPCUnit)
        {
            healthText.text = unit.currentHealth + "/" + unit.health;
            if (unit.currentCoolDown == 0)
            {
                abilityButton.interactable = true;
                cooldownText.text = "Cooldown: READY";
            }
            else
            {
                abilityButton.interactable = false;
                cooldownText.text = "Cooldown: " + unit.currentCoolDown;
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
                cooldownText.text = "Cooldown: READY";
            }
            else
            {
                abilityButton.interactable = false;
                cooldownText.text = "Cooldown: " + unit.currentCoolDown;
            }    
        }
        else
        {
            healthText.text = "Health: " + unit.health;
            cooldownText.text = "Cooldown: " + unit.coolDown;
        }  
        attackRangeText.text = unit.attackRange.ToString();
        movementSpeedText.text = unit.movementSpeed.ToString();
        nameText.text = unit.characterName;
        attackDamageText.text = unit.attackDamage.ToString();
        portraitImage.sprite = unit.portrait;

        if (unit is PlayerUnit player)
        {
            passiveText.text = player.startOfBattleAbilityDescription;
            abilityText.text = player.inBattleAbilityDescription;
            videoPlayer.PlayClip(player.previewClip);
            abilityTabButton.SetActive(true);
        } else if (unit is EnemyUnit enemy)
        {
            passiveText.text = enemy.characterDescription;
            abilityTabButton.SetActive(false);
        }
    }

    public void ShowStatsTab()
    {
        statPanel.SetActive(true);
        passivePanel.SetActive(false);
        abilityPanel.SetActive(false);
    }

    public void ShowPassiveTab()
    {
        statPanel.SetActive(false);
        passivePanel.SetActive(true);
        abilityPanel.SetActive(false);
    }

    public void ShowAbilityTab()
    {
        statPanel.SetActive(false);
        passivePanel.SetActive(false);
        abilityPanel.SetActive(true);
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
