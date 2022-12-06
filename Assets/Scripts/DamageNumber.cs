using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DamageNumber : MonoBehaviour
{
    public TextMeshPro text;

    /// <summary>
    /// Initializes the damage popup with the given parameters
    /// </summary>
    /// <param name="text">the text to display</param>
    /// <param name="color">the color of the text</param>
    public void Initialize(string text, Color color)
    {
        this.text.text = text;
        this.text.color = color;
    }

    /// <summary>
    /// Destroys the gameobject this component is attached to.
    /// Used to destroy the object in animations
    /// </summary>
    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}
