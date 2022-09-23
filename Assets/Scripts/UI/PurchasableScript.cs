using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchasableScript : MonoBehaviour
{
    public PlayerUnit unitPrefab;
    public Image image;
    public GameObject descriptiveText;

    private void Awake()
    {
        if (!image)
        {
            image = GetComponent<Image>();
        }
    }

    private void OnMouseEnter()
    {
        descriptiveText.SetActive(true);
    }

    private void OnMouseExit()
    {
        descriptiveText.SetActive(false);
    }
}
