using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraReference : MonoBehaviour {

    public Transform objective;
    public float rotationSpeed = 90.0f;
    public float transitionTimeBetweenEnemies = 0.3f;

    //private Vector2 previousMousePosition;
    private SpringCamera cameraControl;
    private InputManager inputManager;
    private RobotControl playerControl;

    private Transform previousObjective;
    private Quaternion previousObjectiveRotation;
    private float transitionProgression = 0;

	// Use this for initialization
	void Start () {
        //previousMousePosition = Input.mousePosition;
        cameraControl = FindObjectOfType<SpringCamera>();
        inputManager = FindObjectOfType<InputManager>();
        playerControl = FindObjectOfType<RobotControl>();
        //
        transitionProgression = transitionTimeBetweenEnemies;
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        float dt = Time.deltaTime;

        // TODO: Trabajar con el ideal position del Repulsor para evitar camara oscilando de forma incomoda
        transform.position = objective.position;

        UpdateRotation(dt);

        UpdateUp(objective.up);

	}

    // Para testeos
    private void OnGUI()
    {
        //GUI.Label(new Rect(20, 100, 200, 20), "Rotation x: " + transform.rotation.x * Mathf.Rad2Deg);
    }

    void UpdateRotation(float dt)
    {
        if (cameraControl.TargetingPlayer)
        {
            Vector2 mouseMovement = inputManager.MouseMovement;
            Vector2 rightAxisMovement = inputManager.RightStickAxis;
            //float amountToRotate = ((mouseMovement.x * 1/10) + (rightAxisMovement.x * 1)) * rotationSpeed * dt;

            float amountToRotateY = ((mouseMovement.x * 1) + (rightAxisMovement.x * 1)) * rotationSpeed * dt;
            float amountToRotateX = ((-mouseMovement.y * 1) + (rightAxisMovement.y * 1)) * rotationSpeed * dt;

            transform.Rotate(new Vector3(amountToRotateX, amountToRotateY, 0.0f));
            // Ajuste en el eje x
            if (!playerControl.Adhering)
            {
                // Que no rtoe en z
                Vector3 currentEulers = transform.eulerAngles;
                currentEulers.z = 0;
                transform.eulerAngles = currentEulers;
                // Vamos a acotar la rotación en x
                //OJO ñapa
                Quaternion rotationConstraints = Quaternion.Euler(60, 30, 0);
                Quaternion currentRotation = transform.rotation;
                currentRotation.x = Mathf.Clamp(currentRotation.x, -rotationConstraints.x, rotationConstraints.y);
                
                transform.rotation = currentRotation;
            }

        }
        else
        {
            // Transición gradual entre objetivos para no marear al player
            // TODO: Chequear si es necesaria aqui también
            // Probablemente valga con tenerla en la cámara
            //if(transitionProgression < transitionTimeBetweenEnemies)
            //{
            //    Quaternion enemyDirection = Quaternion.LookRotation(cameraControl.CurrentTarget.position - transform.position);
            //    transform.rotation = Quaternion.Slerp(previousObjectiveRotation, enemyDirection, 
            //        transitionProgression / transitionTimeBetweenEnemies);
            //    transitionProgression += dt;
            //}
            //else if(previousObjective == cameraControl.CurrentTarget)
            //{
            //    transform.LookAt(cameraControl.CurrentTarget);
            //}
            //// Aquí hacemos la transición
            //else
            //{
            //    transitionProgression = 0;
            //    previousObjective = cameraControl.CurrentTarget;
            //    previousObjectiveRotation = transform.rotation;
            //}
            transform.LookAt(cameraControl.CurrentTarget);
        }
        
    }

    //
    public void ResetEulerX()
    {
        Vector3 currentEulers = transform.eulerAngles;
        currentEulers.x = 0;
        transform.eulerAngles = currentEulers;
    }

    //
    public void UpdateUp(Vector3 newUp)
    {
        transform.rotation = Quaternion.LookRotation(transform.forward, newUp);
    }
}
