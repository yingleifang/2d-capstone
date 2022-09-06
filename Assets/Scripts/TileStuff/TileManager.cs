using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class CubeCoord {
    private Vector3Int coords;

    public CubeCoord(int x, int y, int z) 
    {
        coords = new Vector3Int(x, y, z);
    }

    public CubeCoord(Vector3Int vec)
    {
        coords = vec;
    }

    public int x {
        get { return coords.x; }
        set { coords.x = value; }
    }

    public int y {
        get { return coords.y; }
        set { coords.y = value; }
    }

    public int z {
        get { return coords.z; }
        set { coords.z = value; }
    }

    public int q {
        get { return coords.x; }
        set { coords.x = value; }
    }

    public int r {
        get { return coords.y; }
        set { coords.y = value; }
    }

    public int s {
        get { return coords.z; }
        set { coords.z = value; }
    }

    public static CubeCoord operator +(CubeCoord a, CubeCoord b) {
        return new CubeCoord(a.coords + b.coords);
    }

    public static CubeCoord operator -(CubeCoord a, CubeCoord b) {
        return new CubeCoord(a.coords + b.coords);
    }

    public override string ToString() {
        return coords.ToString();
    }
}

public class TileManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap map;
    
    [SerializeField]
    private List<TileDataScriptableObject> tileDatas; 

    private CubeCoord[] directions = {new CubeCoord(1, 0, -1), new CubeCoord(1, -1, 0), 
             new CubeCoord(0, -1, 1), new CubeCoord(-1, 0, 1), new CubeCoord(-1, 1, 0), new CubeCoord(0, 1, -1)};

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
            CubeCoord cubePosition = UnityCellToCube(gridPosition);
        }
    }

    public void AddUnit(Vector3Int location, Unit unit)
    {
        dynamicTileDatas[location].unit = unit;
    }

    public int Distance(CubeCoord start, CubeCoord end)
    {
        CubeCoord temp = start - end;
        return Mathf.Max(Mathf.Abs(temp.x), Mathf.Abs(temp.y), Mathf.Abs(temp.z));
    }

    public int Distance(Vector3Int start, Vector3Int end) 
    {
        return Distance(UnityCellToCube(start), UnityCellToCube(end));
    }

    public bool IsImpassable(Vector3Int cellCoords)
    {
        return baseTileDatas[map.GetTile(cellCoords)].impassable;
    }

    public bool IsImpassable(CubeCoord cubeCoords) 
    {
        return IsImpassable(CubeToUnityCell(cubeCoords));
    }

    public bool InBounds(Vector3Int cellCoords)
    {
        if (dynamicTileDatas.ContainsKey(cellCoords))
        {
            return true;
        }
        return false;
    }

    public CubeCoord CubeNeighbor(CubeCoord cubeCoords, CubeDirections direction)
    {
        return cubeCoords + GetCubeDirection(direction);
    }

    private CubeCoord GetCubeDirection(CubeDirections direction)
    {
        return directions[(int) direction];
    }

    private CubeCoord UnityCellToCube(Vector3Int cell)
    {
        var col = cell.x; 
        var row = cell.y * -1;
        var q = col - (row - (row & 1)) / 2;
        var r = row;
        var s = -q - r;
        return new CubeCoord(q, r, s);
    }

    private Vector3Int CubeToUnityCell(CubeCoord cube)
    {
        var q = cube.x;
        var r = cube.y;
        var col = q + (r - (r & 1)) / 2;
        var row = r * -1;

        return new Vector3Int(col, row,  0);
    }

}
