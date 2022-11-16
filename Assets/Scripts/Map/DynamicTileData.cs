using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicTileData
{
    public Unit unit;
    public Overlay overlay; // Mainly used in TileInfoWindow
    public Unit deadUnit; // Last unit that died on the tile
    public GameObject decoration; // decorative sprite on the tile ie. a gravestone
}
