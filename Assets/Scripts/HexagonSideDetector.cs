using Unity.VisualScripting;
using UnityEngine;

public class HexagonSideDetector : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // Check if collision is with another collider
        Collider collider = collision.collider;
        if (collider != null)
        {
            // Calculate contact point
            ContactPoint contact = collision.contacts[0];
            Vector3 contactPoint = contact.point;

            // Determine which side the collision occurred on
            DetermineSide(contactPoint);
        }
    }
    void DetermineSide(Vector3 contactPoint)
    {
        // Get the position of the hexagon
        Vector3 hexPosition = transform.position;

        // Calculate the distance between the contact point and the hexagon
        float distanceX = contactPoint.x - hexPosition.x;
        float distanceZ = contactPoint.z - hexPosition.z;

        // Calculate the angle between the contact point and the hexagon
        float angle = Mathf.Atan2(distanceZ, distanceX) * Mathf.Rad2Deg;

        // Determine the side based on the angle
        if (angle >= 30 && angle < 90)
        {
            Debug.Log("Collision on the right side");
        }
        else if (angle >= 90 && angle < 150)
        {
            Debug.Log("Collision on the top right side");
        }
        else if (angle >= 150 || angle < -150)
        {
            Debug.Log("Collision on the top side");
        }
        else if (angle >= -150 && angle < -90)
        {
            Debug.Log("Collision on the top left side");
        }
        else if (angle >= -90 && angle < -30)
        {
            Debug.Log("Collision on the left side");
        }
        else
        {
            Debug.Log("Collision on the bottom left side");
        }
    }
}
