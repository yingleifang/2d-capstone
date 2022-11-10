using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cursorChange : MonoBehaviour
{
    // Start is called before the first frame update
    public Texture2D cursorArrow;
    void Start()
    {
        Cursor.SetCursor(cursorArrow, Vector2.zero, CursorMode.ForceSoftware);
    }

}
