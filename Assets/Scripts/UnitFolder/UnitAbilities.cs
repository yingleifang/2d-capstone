using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitAbilities", menuName = "Unit Abilities")]
public class UnitAbilities : ScriptableObject
{
    public SoundEffect AttackAdjacentSound;
    public SoundEffect BoostSpeedAdjacentSound;
    public IEnumerator AttackAdjacent(BattleState state, Unit user)
    {
        Debug.Log("Attacking adjacent units");
        List<Vector3Int> tiles = state.map.GetTilesInRange(user.location, 1, false);
        user.audio.PlaySound(AttackAdjacentSound);
        foreach (Vector3Int tile in tiles)
        {
            Unit unit = state.map.GetUnit(tile);
            if(unit && unit != user)
            {
                unit.ChangeHealth(-1);
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield break;
    }

    public IEnumerator BoostSpeedAdjacent(BattleState state, Unit user)
    {
        Debug.Log("Speeding up adjacent units");
        List<Vector3Int> tiles = state.map.GetTilesInRange(user.location, 1, false);
        user.audio.PlaySound(BoostSpeedAdjacentSound);
        foreach (Vector3Int tile in tiles)
        {
            Unit unit = state.map.GetUnit(tile);
            if (unit && unit != user && unit is PlayerUnit player)
            {
                player.currentMovementSpeed += 1;
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield break;
    }

    public IEnumerator Nothing()
    {
        yield break;
    }
}
