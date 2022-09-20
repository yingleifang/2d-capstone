using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchasableScript : MonoBehaviour
{
    public PlayerUnit unitPrefab;
    public Image image;

    private void Awake()
    {
        if (!image)
        {
            image = GetComponent<Image>();
        }
    }
}
