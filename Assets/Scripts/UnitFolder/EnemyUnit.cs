using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    enum Type {Fish};

    Type type;

    public override bool useAbility(Vector3Int target)
    {
        return false;
    }

}
