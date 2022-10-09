using SpriteGlow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessingSettings : MonoBehaviour
{
    [SerializeField]
    private Color SelectColor;

    [SerializeField]
    private Color DeSelectColor;

    [SerializeField]
    private float CanMoveAndAttackGlowBrightness;

    [SerializeField]
    private float CanAttackGlowBrightness;

    public void ShowTheGlow(List<PlayerUnit> playerUnits)
    {
        foreach (var playerUnit in playerUnits)
        {
            var spriteGlow = playerUnit.gameObject.transform.GetComponentInChildren<SpriteGlowEffect>();
            spriteGlow.GlowBrightness = CanMoveAndAttackGlowBrightness;
            spriteGlow.OutlineWidth = 2;
        };
    }

    public void DisableGlow(PlayerUnit playerUnit)
    {
            var spriteGlow = playerUnit.gameObject.transform.GetComponentInChildren<SpriteGlowEffect>();
            spriteGlow.OutlineWidth = 0;
    }

    public void CanAttackGlow(PlayerUnit playerUnit)
    {
        var spriteGlow = playerUnit.gameObject.transform.GetComponentInChildren<SpriteGlowEffect>();
        spriteGlow.GlowBrightness = CanAttackGlowBrightness;
    }

    public void CanMoveAndAttackGlow(PlayerUnit playerUnit)
    {
        var spriteGlow = playerUnit.gameObject.transform.GetComponentInChildren<SpriteGlowEffect>();
        spriteGlow.GlowBrightness = CanMoveAndAttackGlowBrightness;
    }

    public void ChangeColorToSelected(PlayerUnit playerUnit)
    {
        var spriteGlow = playerUnit.gameObject.transform.GetComponentInChildren<SpriteGlowEffect>();
        spriteGlow.GlowColor = SelectColor;
    }

    public void ChangeColorToDeSelected(PlayerUnit playerUnit)
    {
        var spriteGlow = playerUnit.gameObject.transform.GetComponentInChildren<SpriteGlowEffect>();
        spriteGlow.GlowColor = DeSelectColor;
    }

}
