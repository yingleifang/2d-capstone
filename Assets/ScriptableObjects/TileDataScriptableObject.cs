using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


/// <summary>
/// Stores tile data which does not change throughout the course of
/// the game
/// </summary>
[CreateAssetMenu]
public class TileDataScriptableObject : ScriptableObject
{
    public TileBase[] tiles;
    public int movementSpeedModifier;
    public bool impassable;
    public bool hazardous;
    public int weight = 1;
}
