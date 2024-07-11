using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject pivot; // Referencia al GameObject que actúa como pivote de la cámara
    public float rotationSpeed = 100f;
    public float minY = 10f;
    public float maxY = 80f;
    public float panBorderThickness = 10f;

    private Vector3 lastMousePosition;

    void Update()
    {
        Vector3 pos = transform.position;

        // Zoom con la rueda del ratón
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * 10f * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        // Movimiento hacia los bordes laterales de la pantalla para rotar
        if (Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            RotateCameraAroundPivot(Vector3.up);
        }
        else if (Input.mousePosition.x <= panBorderThickness)
        {
            RotateCameraAroundPivot(Vector3.down);
        }

        transform.position = pos;
    }

    void RotateCameraAroundPivot(Vector3 axis)
    {
        if (pivot != null)
        {
            Vector3 pivotPosition = pivot.transform.position;
            Vector3 cameraPosition = transform.position;

            Vector3 offset = cameraPosition - pivotPosition;

            float angle = rotationSpeed * Time.deltaTime;

            transform.RotateAround(pivotPosition, axis, angle);

            transform.position = pivotPosition + offset;
        }
    }
}
