using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float zoomStep, minCamSize, maxCamSize;

    [SerializeField]
    private float dragSpeed;

    private Vector3 dragOrigin;

    [SerializeField]
    private float mapMinX, mapMinY, mapMaxX, mapMaxY;

    private float camSize;

    private void Start()
    {
        camSize = cam.orthographicSize;
    }

    private void Update()
    {
        if (BattleManager.instance.gameIsPaused || BattleManager.instance.ui.unitSelectionWindow.gameObject.activeSelf)
        {
            StartCoroutine(WaitForSeconds(.2f));
            return;
        }
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

    private IEnumerator WaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    private void PanCamera()
    {
        if (Input.GetMouseButtonDown(0))
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            difference *= dragSpeed;
            cam.transform.position = ClampCamera(cam.transform.position + difference);
        }
        cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, -10);
    }

    public void ZoomIn()
    {
        float newSize = cam.orthographicSize - zoomStep;
        
        newSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
        cam.orthographicSize = newSize;

        cam.transform.position = ClampCamera(cam.transform.position);
    }

    public void Zoomout()
    {
        float newSize = cam.orthographicSize + zoomStep;
        cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
        cam.transform.position = ClampCamera(cam.transform.position);
    }

    private Vector3 ClampCamera(Vector3 targetPosition)
    {
        //float camHeight = cam.orthographicSize;
        //float camWidth = cam.orthographicSize * cam.aspect;
        float minX = mapMinX * (camSize / cam.orthographicSize);
        float maxX = mapMaxX * (camSize / cam.orthographicSize);
        float minY = mapMinY * (camSize / cam.orthographicSize);
        float maxY = mapMaxY * (camSize / cam.orthographicSize);
        float newX = Mathf.Clamp(targetPosition.x, minX, maxX);
        float newY = Mathf.Clamp(targetPosition.y, minY, maxY);

        return new Vector3(newX, newY, targetPosition.z);
    }
}
