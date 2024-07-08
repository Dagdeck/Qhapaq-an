using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f;
    public float scrollSpeed = 2f;
    public float rotationSpeed = 100f;
    public float minY = 10f;
    public float maxY = 80f;
    public float panBorderThickness = 10f;

    private Vector3 lastMousePosition;

    void Update()
    {
        Vector3 pos = transform.position;

        // Movimiento hacia los bordes de la pantalla
        if (Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            pos.z += panSpeed * Time.deltaTime;
        }
        if (Input.mousePosition.y <= panBorderThickness)
        {
            pos.z -= panSpeed * Time.deltaTime;
        }
        if (Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (Input.mousePosition.x <= panBorderThickness)
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        // Zoom con la rueda del ratón
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        // Rotación de la cámara con el botón derecho del ratón
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(1))
        {
            Vector3 deltaMousePosition = Input.mousePosition - lastMousePosition;
            transform.Rotate(Vector3.up, deltaMousePosition.x * rotationSpeed * Time.deltaTime, Space.World);
            lastMousePosition = Input.mousePosition;
        }

        transform.position = pos;
    }
}

