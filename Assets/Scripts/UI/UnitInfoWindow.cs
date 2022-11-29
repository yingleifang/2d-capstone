using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitInfoWindow : MonoBehaviour
{
    
    public Animator anim;
    [SerializeField]
    private TextMeshProUGUI healthText, attackRangeText, movementSpeedText, cooldownText,
                    nameText, attackDamageText, passiveText, abilityText, abilityName;
    public GameObject abilityTabButton, statPanel, passivePanel, abilityPanel, mainPanel;
    public Button statsTabButton;
    public Button abilityButton;
    public TextMeshProUGUI abilityButtonText;
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
                abilityButtonText.text = "Use Ability";
                abilityButton.animator.SetBool("Cooldown", false);
            }
            else
            {
                abilityButton.interactable = false;
                abilityButtonText.text = "Cooldown: " + unit.currentCoolDown;
                abilityButton.animator.SetBool("Cooldown", true);
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
            abilityName.text = player.abilityName;
            cooldownText.text = "Cooldown: " + unit.coolDown + " Turns";
            videoPlayer.PlayClip(player.previewClip);
            abilityTabButton.SetActive(true);
            if (player.hasAttacked)
            {
                abilityButton.interactable = false;
                if (player.currentCoolDown == 0)
                {
                    abilityButtonText.text = "Already Attacked";
                }
            }
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
        abilityPanel.SetActive(true);
        mainPanel.SetActive(false);
        anim.SetBool("Show Ability", true);
    }

    public void ShowMainPanel()
    {
        abilityPanel.SetActive(false);
        mainPanel.SetActive(true);
        anim.SetBool("Show Ability", false);
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
