using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileDataScriptableObject : ScriptableObject
{
    public TileBase[] tiles;
    public int movementSpeedModifier;
    public bool impassable;
    
    
}
