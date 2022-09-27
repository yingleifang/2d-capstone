using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PurchasableScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PlayerUnit unitPrefab;
    public Image image;
    public UnitInfoWindow unitInfoWindow;

    private void Awake()
    {
        if (!image)
        {
            image = GetComponent<Image>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        unitInfoWindow.ShowStats(unitPrefab, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("EXIT");
        unitInfoWindow.HideStats();
    }
}
