using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    private Vector2 previousMousePosition;
    private Vector2 mouseMovement;

    // Start is called before the first frame update
    void Start()
    {
        previousMousePosition = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Get the mouse movement
        Vector2 newmousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        mouseMovement = newmousePosition - previousMousePosition;
        previousMousePosition = newmousePosition;
        // And apply to the rotation
        transform.Rotate(mouseMovement.y, mouseMovement.x, 0);
        // Apply correction
        Vector3 revisedEulers = transform.eulerAngles;
        revisedEulers.z = 0;
        transform.eulerAngles = revisedEulers;
    }
}
