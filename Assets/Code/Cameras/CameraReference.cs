using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraReference : MonoBehaviour {

    public Transform objective;
    public float rotationSpeed = 90.0f;

    private Vector2 previousMousePosition;
    private SpringCamera cameraControl;
    private InputManager inputManager;

	// Use this for initialization
	void Start () {
        previousMousePosition = Input.mousePosition;
        cameraControl = FindObjectOfType<SpringCamera>();
        inputManager = FindObjectOfType<InputManager>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        float dt = Time.deltaTime;

        transform.position = objective.position;

        UpdateRotation(dt);

	}

    void UpdateRotation(float dt)
    {
        if (cameraControl.TargetingPlayer)
        {
            Vector2 mouseMovement = inputManager.MouseMovement;
            Vector2 rightAxisMovement = inputManager.RightStickAxis;
            //float amountToRotate = ((mouseMovement.x * 1/10) + (rightAxisMovement.x * 1)) * rotationSpeed * dt;
#if UNITY_EDITOR
            float amountToRotate = ((mouseMovement.x * 1) + (rightAxisMovement.x * 1)) * rotationSpeed * dt;
#else
            float amountToRotate = ((mouseMovement.x * 10) + (rightAxisMovement.x * 1)) * rotationSpeed * dt;
#endif
            transform.Rotate(new Vector3(0.0f, amountToRotate, 0.0f));
            // Ajuste en el eje x
            Vector3 currentEulers = transform.eulerAngles;
            currentEulers.x = 0;
            transform.eulerAngles = currentEulers;
        }
        else
        {
            transform.LookAt(cameraControl.CurrentTarget);
        }
        
    }
}
