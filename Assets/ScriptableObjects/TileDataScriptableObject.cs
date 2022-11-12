using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileDataScriptableObject : ScriptableObject
{
    public string tileName = "Tile";
    [TextArea]
    public string description = "A basic tile with no notable features.";
    public TileBase[] tiles;
    public AnimatedTile animatedTile;
    public int bonusStatModifier;
    public bool impassable;
    public bool hazardous;
    public int weight = 1;
}
