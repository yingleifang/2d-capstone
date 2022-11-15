using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PurchasableScript : MonoBehaviour
{
    public PlayerUnit unitPrefab;
    public TextMeshProUGUI nameText;
    public Image unitPortrait;
    public Image unitSprite;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI movementText;
    public TextMeshProUGUI rangeText;
    public TextMeshProUGUI startOfBattleText;
    public TextMeshProUGUI abilityName;
    public TextMeshProUGUI inBattleText;
    public TextMeshProUGUI cooldownText;
    public VideoPlayerFix videoPlayer;
    public GameObject statsTab, passiveTab, abilityTab;
    public Button statsButton;

    /// <summary>
    /// Initializes the unit selection window with values from the given unitPrefab
    /// </summary>
    /// <param name="unitPrefab">the unit to display</param>
    public void Initialize(PlayerUnit unitPrefab)
    {
        this.unitPrefab = unitPrefab;
        nameText.text = unitPrefab.characterName;
        unitPortrait.sprite = unitPrefab.portrait;
        unitSprite.sprite = unitPrefab.talkSprite;
        healthText.text = unitPrefab.health.ToString();
        attackText.text = unitPrefab.attackDamage.ToString();
        movementText.text = unitPrefab.movementSpeed.ToString();
        rangeText.text = unitPrefab.attackRange.ToString();
        startOfBattleText.text = unitPrefab.startOfBattleAbilityDescription;
        abilityName.text = unitPrefab.abilityName;
        inBattleText.text = unitPrefab.inBattleAbilityDescription;
        cooldownText.text = unitPrefab.coolDown.ToString() + " Turns";
        videoPlayer.PlayClip(unitPrefab.previewClip);
        statsButton.onClick.Invoke();
    }

    public void ShowStatsTab()
    {
        statsTab.SetActive(true);
        passiveTab.SetActive(false);
        abilityTab.SetActive(false);
    }

    public void ShowPassiveTab()
    {
        statsTab.SetActive(false);
        passiveTab.SetActive(true);
        abilityTab.SetActive(false);
    }

    public void ShowAbilityTab()
    {
        statsTab.SetActive(false);
        passiveTab.SetActive(false);
        abilityTab.SetActive(true);
    }
}
