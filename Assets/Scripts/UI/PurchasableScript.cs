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
    public Image unitImage;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI movementText;
    public TextMeshProUGUI rangeText;
    public TextMeshProUGUI startOfBattleText;
    public TextMeshProUGUI inBattleText;
    public TextMeshProUGUI cooldownText;
    public VideoPlayerFix videoPlayer;

    /// <summary>
    /// Initializes the unit selection window with values from the given unitPrefab
    /// </summary>
    /// <param name="unitPrefab">the unit to display</param>
    public void Initialize(PlayerUnit unitPrefab)
    {
        this.unitPrefab = unitPrefab;
        nameText.text = unitPrefab.characterName;
        unitImage.sprite = unitPrefab.portrait;
        healthText.text = unitPrefab.health.ToString();
        attackText.text = unitPrefab.attackDamage.ToString();
        movementText.text = unitPrefab.movementSpeed.ToString();
        rangeText.text = unitPrefab.attackRange.ToString();
        startOfBattleText.text = unitPrefab.startOfBattleAbilityDescription;
        inBattleText.text = unitPrefab.inBattleAbilityDescription;
        cooldownText.text = unitPrefab.coolDown.ToString();
        videoPlayer.PlayClip(unitPrefab.previewClip);
    }
}
