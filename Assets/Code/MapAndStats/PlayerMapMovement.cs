using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMapMovement : MonoBehaviour
{
    public Transform planetT;

    private InputManager inputManager;

    // Start is called before the first frame update
    void Start()
    {
        inputManager = FindObjectOfType<InputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 controlAxis = inputManager.StickAxis;
        Vector3 controlAxis3D = new Vector3(controlAxis.x, controlAxis.y, 0);
        Vector3 planetDirection = transform.position - planetT.position;
        Vector3 movementDirection = Vector3.Cross(controlAxis3D, -planetDirection);

        transform.RotateAround(planetT.position, -controlAxis.x * Vector3.up, 90*Time.deltaTime);
        transform.RotateAround(planetT.position, controlAxis.y * Vector3.right, 90 * Time.deltaTime);
    }
}
