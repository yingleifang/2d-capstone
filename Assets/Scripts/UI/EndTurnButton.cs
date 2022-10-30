using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour
{
    public bool highlighted = false;
    public Animator anim;

    public Button button;

    private void Awake()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
        if (button == null)
        {
            Debug.Log("getting button");
            button = GetComponent<Button>();
        }
    }

    /// <summary>
    /// Sets whether or not the end turn button can be interacted with
    /// </summary>
    /// <param name="interactable">the interactable state to set the button to</param>
    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }

    /// <summary>
    /// Plays the highlight animation for the button
    /// </summary>
    /// <returns>a coroutine representing the animation startup</returns>
    public IEnumerator PlayHighlightAnim()
    {
        Debug.Log("HIGHLIGHTING");
        anim.SetBool("highlighted", true);
        yield break;
    }

    /// <summary>
    /// Stops playing the highlight animation for the button
    /// </summary>
    /// <returns>a coroutine representing the animation wind-down</returns>
    public IEnumerator PlayUnhighlightAnim()
    {
        anim.SetBool("highlighted", false);
        yield break;
    }

    /// <summary>
    /// Sets whether the button is highlighted or not and plays the associated animation.
    /// </summary>
    /// <param name="highlighted">the highlight status to set</param>
    /// <returns>a coroutine representing any animation startups/wind-downs, if any</returns>
    public IEnumerator SetHighlighted(bool highlighted)
    {
        if (this.highlighted != highlighted)
        {
            this.highlighted = highlighted;

            if (highlighted)
            {
                return PlayHighlightAnim();
            } else
            {
                return PlayUnhighlightAnim();
            }
        }

        return BlankEnumerator();
    }

    /// <summary>
    /// A blank coroutine.
    /// Can be returned in functions that return an IEnumerator
    /// </summary>
    /// <returns>a blank coroutine</returns>
    public IEnumerator BlankEnumerator()
    {
        yield break;
    }
}
