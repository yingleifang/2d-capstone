using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClickHandler : MonoBehaviour
{
    bool interactable = true;
    public UnityEvent OnClick;

    private void OnMouseDown()
    {
        if(enabled)
        {
            OnClick.Invoke();
        }
    }
}
