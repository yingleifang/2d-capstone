using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyUnit : Unit
{
    public enum Type {Fish};

    public Type type;

    public override IEnumerator UseAbility(Vector3Int target)
    {
        yield break;
    }

    public abstract IEnumerator performAction(BattleState state);

    public PlayerUnit FindClosestPlayerUnit(BattleState state)
    {
        PlayerUnit closestUnit = null;
        int closest = 100000;
        foreach (PlayerUnit unit in state.playerUnits)
        {
            int distance = state.map.RealDistance(location, unit.location);
            if (distance < closest)
            {
                closestUnit = unit;
                closest = distance;
            }
        }

        return closestUnit;
    }

    public PlayerUnit FindFurthestPlayerUnit(BattleState state)
    {
        PlayerUnit furthestUnit = null;
        int furthest = -1;
        foreach (PlayerUnit unit in state.playerUnits)
        {
            int distance = state.map.RealDistance(location, unit.location);
            
            if (distance > furthest)
            {
                
                furthestUnit = unit;
                furthest = distance;
            }
        }
        Debug.Log(furthestUnit);
        return furthestUnit;        
    }
}
