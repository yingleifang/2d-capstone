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

    private Vector3Int[] directions = {new Vector3Int(1, 0, -1), new Vector3Int(1, -1, 0), 
             new Vector3Int(0, -1, 1), new Vector3Int(-1, 0, 1), new Vector3Int(-1, 1, 0), new Vector3Int(0, 1, -1)};

    public enum CubeDirections : int {
        RIGHT = 0,
        TOP_RIGHT = 1,
        TOP_LEFT = 2,
        LEFT = 3,
        BOTTOM_LEFT = 4,
        BOTTOM_RIGHT = 5
    }

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
            Vector3Int cubePosition = UnityCellToCube(gridPosition);
        }
    }

    public void AddUnit(Vector3Int location, Unit unit)
    {
        dynamicTileDatas[location].unit = unit;
    }

    public Vector3Int CubeNeighbor(Vector3Int cubeCoords, CubeDirections direction)
    {
        return cubeCoords + GetCubeDirection(direction);
    }

    private Vector3Int GetCubeDirection(CubeDirections direction)
    {
        return directions[(int) direction];
    }

    private Vector3Int UnityCellToCube(Vector3Int cell)
    {
        var col = cell.x; 
        var row = cell.y * -1;
        var q = col - (row - (row & 1)) / 2;
        var r = row;
        var s = -q - r;
        return new Vector3Int(q, r, s);
    }

    private Vector3Int CubeToUnityCell(Vector3Int cube)
    {
        var q = cube.x;
        var r = cube.y;
        var col = q + (r - (r & 1)) / 2;
        var row = r * -1;

        return new Vector3Int(col, row,  0);
    }

}
