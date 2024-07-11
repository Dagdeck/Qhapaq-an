using UnityEngine;

public class CameraController : MonoBehaviour
{
  public float rotationSpeed = 100;
 // referenciar la camara que es un child de este objeto
    public GameObject camera;



  
   void Update()
   {
   if (Input.GetAxis("Mouse ScrollWheel") > 0)
     {
        GetComponentInChildren<Camera>().fieldOfView--;
     }
     if (Input.GetAxis("Mouse ScrollWheel") < 0)
     {
         GetComponentInChildren<Camera>().fieldOfView++;
     }

     OnMouseDown();
     
      
   }
    //Quiero que el objeto rote cuando se haga click en el mouse y se mueva el mouse
    void OnMouseDown()
    {
        if (Input.GetMouseButton(1))
        {
            float h = rotationSpeed * Input.GetAxis("Mouse X");
            transform.Rotate(0, h, 0);
        }
    }
    // Hacer zoom con la rueda del mouse
    

}
