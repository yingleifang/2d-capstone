using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class TileManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap map;
    
    [SerializeField]
    private List<TileDataScriptableObject> tileDatas; 

    private Dictionary<TileBase, TileDataScriptableObject> baseTileDatas;  
    public Dictionary<Vector3Int, DynamicTileData> dynamicTileDatas;
    
    private Unit selectedUnit;

    // Start is called before the first frame update
    void Awake()
    {
        baseTileDatas = new Dictionary<TileBase, TileDataScriptableObject>();
        dynamicTileDatas = new Dictionary<Vector3Int, DynamicTileData>();
        foreach (TileDataScriptableObject tileData in tileDatas)
        {
            foreach (TileBase tile in tileData.tiles)
            {
                baseTileDatas.Add(tile, tileData);
            }
        }
        for (int x = (int)map.localBounds.min.x; x < map.localBounds.max.x; x++)
        {
            for (int y = (int)map.localBounds.min.y; y < map.localBounds.max.y; y++)
            {
                for (int z = (int)map.localBounds.min.z; z < map.localBounds.max.z; z++)
                {
                    dynamicTileDatas.Add(new Vector3Int(x, y, z), new DynamicTileData());
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector2 mapPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = map.WorldToCell(mapPosition);

            TileBase clickedTile = map.GetTile(gridPosition);

            if (dynamicTileDatas[gridPosition].unit == null)
            {

            }
            Debug.Log(UnityCellToCube(gridPosition));
        }
    }

    public void AddUnit(Vector3Int location, Unit unit)
    {
        dynamicTileDatas[location].unit = unit;
    }

    //Taken from https://github.com/Unity-Technologies/2d-extras/issues/69
    private Vector3Int UnityCellToCube(Vector3Int cell)
    {
        var yCell = cell.x; 
        var xCell = cell.y;
        var x = yCell - (xCell - (xCell & 1)) / 2;
        var z = xCell;
        var y = -x - z;
        return new Vector3Int(x, y, z);
    }
    private Vector3Int CubeToUnityCell(Vector3Int cube)
    {
        var x = cube.x;
        var z = cube.z;
        var col = x + (z - (z & 1)) / 2;
        var row = z;

        return new Vector3Int(col, row,  0);
    }

}
