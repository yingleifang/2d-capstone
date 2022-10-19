using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileInfoWindow : MonoBehaviour
{
    public Image tileImage, overlayImage;

    public TextMeshProUGUI tileName, tileDescription, overlayDescription;

    public GameObject overlayWindow;

    /// <summary>
    /// Displays the given tile data
    /// </summary>
    /// <param name="tileData">the tile data to display. Does nothing if null</param>
    public void ShowTile(HexTileData tileData)
    {
        if (tileData == null)
        {
            return;
        }

        gameObject.SetActive(true);
        tileName.text = tileData.tileData.tileName;
        tileDescription.text = tileData.tileData.description;
        tileImage.sprite = tileData.sprite;

        if (tileData.dynamicTileData.overlay != null)
        {
            overlayWindow.SetActive(true);
            overlayImage.sprite = tileData.dynamicTileData.overlay.sprite;
            overlayDescription.text = tileData.dynamicTileData.overlay.description;
        } else
        {
            overlayWindow.SetActive(false);
        }
    }

    /// <summary>
    /// Hides the tile info window
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
