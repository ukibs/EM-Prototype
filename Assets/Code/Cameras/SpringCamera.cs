using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringCamera : MonoBehaviour {

	#region Public Attributes
	public Transform targetPlayer;
	public Vector3 idealOffset = new Vector3(0.0f, 2.0f, -3.0f);
	public float springK = 5.0f;
	public float dampingK = 20.0f;
	public Vector3 targetOffset = new Vector3 (0.0f, 2.0f, 0.0f);
	public float maxDistancePlayer = 2.0f;

    public float maxUpperOffsetY = 5.0f;
    public float maxLowerOffsetY = 3.0f;
    #endregion

    #region Private Attributes
    private Vector3 vel = new Vector3 (0.0f, 0.0f, 0.0f);
    private Transform currentTarget;
    private Camera cameraComponent;
    private InputManager inputManager;

    private float originalY;
    private float currentOffsetY = 0;

    private Vector3 targetPos;

    private bool changeAllowed = true;
    #endregion

    #region Properties

    public Transform CurrentTarget { get { return currentTarget; } }

    public bool TargetingPlayer { get { return currentTarget == targetPlayer; } }

    #endregion

    #region Monobehaviour Methods
    // Use this for initialization
    void Start () {
		//To move to its postion respect to the player
		Vector3 idealPos = targetPlayer.position + 
			idealOffset.x * targetPlayer.right + 
			idealOffset.y * targetPlayer.up +
			idealOffset.z * targetPlayer.forward;

		transform.position = idealPos;

		//To look at the palyer
		targetPos = targetPlayer.TransformPoint (targetOffset);
		transform.LookAt (targetPos, targetPlayer.up);

        //
        currentTarget = targetPlayer;
        //
        cameraComponent = GetComponent<Camera>();
        //
        inputManager = FindObjectOfType<InputManager>();
        //
        originalY = targetOffset.y;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
		float dt = Time.deltaTime;
        // Correction from the mouse movement
        //if (currentTarget == targetPlayer)
        //UpdateRotation(dt);
        // TODO: Hacerlo de forma menos guarra
        //else 
        if (currentTarget.GetComponent<EnemyConsistency>() == null)
            SwitchTarget();

        UpdateMovement (dt);
        UpdateUp(targetPlayer.up);
        CheckSwitchAndEnemies();
        SwitchBetweenEnemies();

        CheckDontEnterInsideScenario();
	}

    private void OnGUI()
    {
        GUI.Label(new Rect(10, Screen.height - 30, 150, 20), "Change allowed: " + changeAllowed);
    }

    #endregion

    #region Methods
    //
    void UpdateMovement(float dt){
		//To move to its postion respect to the player
		Vector3 idealPos = targetPlayer.position + 
			idealOffset.x * targetPlayer.right + 
			idealOffset.y * targetPlayer.up +
			idealOffset.z * targetPlayer.forward;
		
		Vector3 positionOffset = idealPos - transform.position;

		//Apply the damping
		float dampFactor = Mathf.Max(1 - dampingK * dt, 0.0f);
		vel *= dampFactor;

		//Apply the spring
		Vector3 accelSpring = springK * positionOffset;
        vel += accelSpring * dt;
        transform.position += vel * dt;

        // Limit max distance
        Vector3 distanceToPoint = idealPos - transform.position;
        distanceToPoint = Vector3.ClampMagnitude(distanceToPoint, maxDistancePlayer);
        transform.position = idealPos - distanceToPoint;

        //To look at the indicated point
        targetPos = currentTarget.TransformPoint(targetOffset);
        transform.LookAt(targetPos, Vector3.up);
    }

    void UpdateRotation(float dt)
    {
        Vector2 mouseMovement = inputManager.MouseMovement;
        Vector2 rightAxisMovement = inputManager.RightStickAxis;
        //transform.Rotate(new Vector3(-mouseMovement.y, 0.0f, 0.0f));
        //currentOffsetY += (mouseMovement.y + (rightAxisMovement.y * 10)) * dt;
        currentOffsetY += ((mouseMovement.y * 20) + (rightAxisMovement.y * 10)) * dt;
        currentOffsetY = Mathf.Clamp(currentOffsetY, -maxLowerOffsetY, maxUpperOffsetY);
        targetOffset.y = originalY + currentOffsetY;
    }

    //
    void AdjustToEnemyMovement()
    {
        //currentTarget
    }
    
    /// <summary>
    /// Cambiamos entre player y enemigos
    /// En caso de enemigo, coge al más cercano al cnetro de la vista
    /// Y si no al más cercano en el mundo
    /// </summary>
    void CheckSwitchAndEnemies()
    {
        if (inputManager.MarkObjectiveButton)
        {
            // TODO: Hacer que vaya cambiando de enemigo
            if(currentTarget == targetPlayer)
            {
                // Que coja el más cercano al centro de la vista
                // De no haber, que coja el más cercano tal cual
                EnemyConsistency[] enemies = FindObjectsOfType<EnemyConsistency>();
                int nearestScreenEnemy = -1;
                int nearestWorldEnemy = -1;
                float minScreenDistance = Mathf.Infinity;
                float minWorldDistance = Mathf.Infinity;
                //foreach (EnemyConsistency enemy in enemies)
                for(int i = 0; i < enemies.Length; i++)
                {
                    // Distancia al centro de pantalla
                    Vector3 posInScreen = cameraComponent.WorldToViewportPoint(enemies[i].transform.position);
                    // float distanceToCenter = Mathf.Sqrt(Mathf.Pow(posInScreen.x - 0.05f, 2) + Mathf.Pow(posInScreen.y - 0.05f, 2));
                    float distanceToCenter = Mathf.Pow(posInScreen.x - 0.05f, 2) + Mathf.Pow(posInScreen.y - 0.05f, 2);
                    bool inScreen = posInScreen.x >= 0 && posInScreen.x <= 1 && posInScreen.y >= 0 && posInScreen.y <= 1;
                    if (inScreen && distanceToCenter < minScreenDistance)
                    {
                        minScreenDistance = distanceToCenter;
                        nearestScreenEnemy = i;
                    }
                    // Distancia al player
                    float enemyDistance = (transform.position - enemies[i].transform.position).sqrMagnitude;
                    if (enemyDistance < minWorldDistance)
                    {
                        
                        minWorldDistance = enemyDistance;
                        nearestWorldEnemy = i;
                    }
                }
                // The nearest enemy to the screen center if there is
                if(nearestScreenEnemy != -1)
                    SwitchTarget(enemies[nearestScreenEnemy].transform);
                // And the nearest in world if not
                else if(nearestWorldEnemy != -1)
                    SwitchTarget(enemies[nearestWorldEnemy].transform);
            }
            else
            {
                SwitchTarget(null);
            }
        }
    }

    /// <summary>
    /// Cambia la vista entre enemigos
    /// Cambia al más cercano en cordenadas de pantalla respecto al dirección elegida
    /// </summary>
    void SwitchBetweenEnemies()
    {
        Vector2 rightAxis = inputManager.RightStickAxis;
        // Mientras el jugador no haga un buen movimiento de joystick que no haga nada
        if(currentTarget != targetPlayer && rightAxis.magnitude > 0.5f && changeAllowed == true)
        {
            //
            float axisAngle = Mathf.Atan2(rightAxis.y, rightAxis.x);
            //
            EnemyConsistency[] enemies = FindObjectsOfType<EnemyConsistency>();
            float minimalAngle = 180;
            int nearestEnemy = -1;
            for (int i = 0; i < enemies.Length; i++)
            {
                // Vamos a probar cercanía por ángulo
                Vector3 enemyViewPortCoordinates = cameraComponent.WorldToViewportPoint(enemies[i].transform.position);
                Vector2 coordinatesFromScreenCenter = new Vector2(enemyViewPortCoordinates.x - 0.5f, enemyViewPortCoordinates.y - 0.5f);
                // Ahora sacamos el angulo
                float angle = Mathf.Atan2(coordinatesFromScreenCenter.y, coordinatesFromScreenCenter.x);
                //
                float angleOffset = Mathf.Abs(axisAngle - angle);
                if (angleOffset < minimalAngle)
                {
                    minimalAngle = angleOffset;
                    nearestEnemy = i;
                }
            }
            //
            currentTarget = enemies[nearestEnemy].transform;
            changeAllowed = false;
        }
        else if (rightAxis.magnitude > 0.5f)
        {
            // TODO: Hacer esto menos guarro
            // Aqui nada
            // Lo ponemos para que no entre en el else
        }
        else
        {
            changeAllowed = true;
        }
    }

    /// <summary>
    /// Switch camera's target with a new one
    /// </summary>
    /// <param name="newTarget"></param>
    public void SwitchTarget(Transform newTarget = null)
    {
        // TODO: Hcaer una versión limpia
        if (newTarget == null)
        {
            currentTarget = targetPlayer;
            targetOffset.x = 3;
        }
        else
        {
            currentTarget = newTarget;
            targetOffset.x = 0;
        }
        // In any case
        targetOffset.y = 0;
    }

    /// <summary>
    /// Coming soon
    /// </summary>
    /// <param name="directionAndForce"></param>
    public void ShakeCamera(Vector3 directionAndForce)
    {

    }

    public void UpdateUp(Vector3 newUp)
    {
        transform.rotation = Quaternion.LookRotation(transform.forward, newUp);
    }

    void CheckDontEnterInsideScenario()
    {
        Vector3 directionToCheck = transform.position - targetPos;
        RaycastHit hitInfo;
        // Ignoramos la layer Enemy(9)
        if (Physics.Raycast(targetPos, directionToCheck, out hitInfo, directionToCheck.magnitude, 9))
        {
            transform.position = Vector3.Lerp(transform.position, hitInfo.point, 0.8f);
        }
    }
    #endregion
}
