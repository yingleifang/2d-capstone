using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PrefabStruct
{
    public Unit prefab;
    public string name;
}

[CreateAssetMenu]
public class UnitPrefabSO : ScriptableObject
{
    public List<PrefabStruct> prefabList;

    public Unit GetPrefab(string name)
    {
        foreach (var prefab in prefabList)
        {
            if (prefab.name == name)
            {
                return prefab.prefab;
            }
        }

        return null;
    }
}
