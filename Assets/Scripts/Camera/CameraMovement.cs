using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float zoomStep, minCamSize, maxCamSize;

    private Vector3 dragOrigin;

    private void Start()
    {
        minCamSize = 0f;
        maxCamSize = 30f;
        zoomStep = 0.5f;
    }

    private void Update()
    {
        PanCamera();
        if (Input.mouseScrollDelta.y > 0)
        {
            ZoomIn();
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            Zoomout();
        }
    }

    private void PanCamera()
    {
        if (Input.GetMouseButtonDown(0))
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);

            cam.transform.position += difference;
        }
    }

    public void ZoomIn()
    {
        Debug.Log("!!!!!!!!");
        float newSize = cam.orthographicSize - zoomStep;
        cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
    }

    public void Zoomout()
    {
        Debug.Log("??????????????");
        float newSize = cam.orthographicSize + zoomStep;
        cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
    }
}
