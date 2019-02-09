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
        CheckDontEnterInsideScenario();
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
    void CheckSwitchAndEnemies()
    {
        if (inputManager.MarkObjectiveButton)
        {
            // TODO: Hacer que vaya cambiando de enemigo
            if(currentTarget == targetPlayer)
            {
                EnemyConsistency[] enemies = FindObjectsOfType<EnemyConsistency>();
                float minDistance = Mathf.Infinity;
                foreach (EnemyConsistency enemy in enemies)
                {
                    float enemyDistance = (transform.position - enemy.transform.position).sqrMagnitude;
                    if (enemyDistance < minDistance)
                    {
                        SwitchTarget(enemy.transform);
                        minDistance = enemyDistance;
                    }
                }
            }
            else
            {
                SwitchTarget(null);
            }
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
        if (Physics.Raycast(targetPos, directionToCheck, out hitInfo, directionToCheck.magnitude))
        {
            transform.position = Vector3.Lerp(transform.position, hitInfo.point, 0.8f);
        }
    }
    #endregion
}
