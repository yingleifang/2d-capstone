using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    enum Type {Fish};

    Type type;

    public override bool UseAbility(Vector3Int target)
    {
        return false;
    }

    public IEnumerator performAction(BattleState state)
    {
        PlayerUnit closest = FindClosestPlayerUnit(state);

        if(!closest)
        {
            // No player units? Something's wrong
            yield return null;
        }

        // Move as far as possible towards the closest unit
        List<Vector3Int> path = state.map.FindShortestPath(location, closest.location);
        Vector3Int goal = path[currentMovementSpeed];
        if(goal == closest.location)
        {
            goal = path[currentMovementSpeed - 1];
        }
        DoMovement(goal);
        yield return new WaitForSeconds(0.2f);

        // Attack the unit if they're in range
        if(IsTileInAttackRange(closest.location, state.map))
        {
            DoAttack(closest);
            yield return new WaitForSeconds(0.4f);
        }
    }

    public PlayerUnit FindClosestPlayerUnit(BattleState state)
    {
        PlayerUnit closestUnit = null;
        int closest = 100000;
        foreach (PlayerUnit unit in state.playerUnits)
        {
            if(state.map.RealDistance(location, unit.location) < closest)
            {
                closestUnit = unit;
            }
        }

        return closestUnit;
    }
}
