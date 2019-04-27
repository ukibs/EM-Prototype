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

    //public float maxUpperOffsetY = 5.0f;
    //public float maxLowerOffsetY = 3.0f;

    public float transitionTimeBetweenEnemies = 0.3f;
    #endregion

    #region Private Attributes
    private Vector3 vel = new Vector3 (0.0f, 0.0f, 0.0f);
    private Transform currentTarget;
    private Camera cameraComponent;
    private InputManager inputManager;
    private GameManager gameManager;
    private CameraReference cameraReference;

    //private float originalY;
    //private float currentOffsetY = 0;

    private Vector3 targetPos;
    private Vector3 positionWithoutCorrection;

    private bool changeAllowed = true;
    //
    //private EnemyConsistency currentEnemy;
    //
    private Transform previousObjective;
    private Quaternion previousObjectiveRotation;
    private float transitionProgression = 0;

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
        //originalY = targetOffset.y;

        //
        gameManager = FindObjectOfType<GameManager>();
        //
        positionWithoutCorrection = transform.position;
        //
        cameraReference = FindObjectOfType<CameraReference>();
    }

    //
    private void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        CheckSwitchAndEnemies();
        //
        
        
    }

    // Called before physics
    void FixedUpdate () {
        // TODO: Cuidado con este dt
        float dt = Time.deltaTime;
        // Correction from the mouse movement
        //if (currentTarget == targetPlayer)
        //UpdateRotation(dt);
        // TODO: Hacerlo de forma menos guarra
        //else 
        if (currentTarget != targetPlayer && EnemyAnalyzer.enemyConsistency == null)
            //currentTarget.GetComponent<EnemyConsistency>() == null)
        {
            //if(!SwitchBetweenEnemies())
            //    SwitchTarget();
            //Debug.Log("Enemy down, switching to next");
            SwitchBetweenEnemies(Vector2.zero);
        }
        //
        UpdateMovement(dt);
        AdjustToEnemyMovement(dt);
        UpdateRotation(dt);
        UpdateUp(targetPlayer.up);
        //CheckSwitchAndEnemies();
        CheckRightAxis();

        CheckDontEnterInsideScenario();
	}

    private void OnGUI()
    {
        //GUI.Label(new Rect(10, Screen.height - 30, 150, 20), "Change allowed: " + changeAllowed);
        //
        
    }

    private void OnDrawGizmos()
    {
        //
        //Transform[] nearestEnemies = GetNearestEnemiesInScreenAndWorld();
        //if(nearestEnemies != null)
        //{
        //    Vector3 enemyDirection;
        //    // Del centro de la pantalla al enemigo mas cercano a esta
        //    if (nearestEnemies[0] != null)
        //    {
        //        enemyDirection = nearestEnemies[0].position - transform.forward;
        //        Debug.DrawRay(transform.forward + transform.position, enemyDirection, Color.blue);
        //    }

        //    // Del player al enemigo mas cercano
        //    enemyDirection = nearestEnemies[1].position - transform.position;
        //    Debug.DrawRay(transform.position, enemyDirection, Color.green);

        //    //
        //    Debug.DrawRay(transform.position, transform.forward * 50, Color.red);
        //}
        
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
		
		Vector3 positionOffset = idealPos - positionWithoutCorrection;

		//Apply the damping
		float dampFactor = Mathf.Max(1 - dampingK * dt, 0.0f);
		vel *= dampFactor;

		//Apply the spring
		Vector3 accelSpring = springK * positionOffset;
        vel += accelSpring * dt;
        positionWithoutCorrection += vel * dt;

        // Limit max distance
        Vector3 distanceToPoint = idealPos - positionWithoutCorrection;
        distanceToPoint = Vector3.ClampMagnitude(distanceToPoint, maxDistancePlayer);
        positionWithoutCorrection = idealPos - distanceToPoint;

        
    }

    void UpdateRotation(float dt)
    {
        // To look at the indicated point
        // Si el target es el player el objetivo viene determinado por los controloes
        if (currentTarget == targetPlayer)
            targetPos = currentTarget.TransformPoint(targetOffset);
        // Si no este es ajusatado en AdjustToEnemyMovement
        //else
        //    targetPos = currentTarget.position;

        // TODO: Revisar esto
        //if (currentEnemy != null)
        //    targetPos += currentEnemy.centralPointOffset;
        // Transición gradual entre objetivos para no marear al player
        if (transitionProgression < transitionTimeBetweenEnemies)
        {
            Quaternion enemyDirection = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Slerp(previousObjectiveRotation, enemyDirection,
                transitionProgression / transitionTimeBetweenEnemies);
            transitionProgression += dt;
        }
        else if (previousObjective == currentTarget)
        {

            transform.LookAt(targetPos);
        }
        // Aquí hacemos la transición
        else
        {
            //
            //Debug.Log("Switching objective");
            //
            transitionProgression = 0;
            previousObjective = currentTarget;
            previousObjectiveRotation = transform.rotation;
        }
    }

    // TODO: Hacer que la cámara se enfoque en el punto que va a ocupar el enemigo
    // En proporcion a la velocidad del proyectil actual
    // Será aqui donde trabajemos el fijado
    void AdjustToEnemyMovement(float dt)
    {
        //
        if (!EnemyAnalyzer.isActive)
            return;
        //
        if(EnemyAnalyzer.enemyConsistency == null)
        {
            EnemyAnalyzer.Release();
            return;
        }
        // Empezamos cogiendo la posición del enemigo
        Vector3 targetPoint = EnemyAnalyzer.enemyTransform.TransformPoint(EnemyAnalyzer.enemyConsistency.centralPointOffset);
        // TODO: Sacar un índice del arma actualmente equipada para usar su muzzle speed en la función de atnicpar
        Rigidbody enemyRigidbody = EnemyAnalyzer.enemyRb;
        // Determinamos donde va a estar cuando el proyectil llegue a él
        //
        float proyectileMuzzleSpeed = 1000;
        switch (PlayerReference.playerControl.ActiveAttackMode)
        {
            case AttackMode.RapidFire:
                proyectileMuzzleSpeed = gameManager.rapidFireMuzzleSpeed;
                break;
            case AttackMode.Canon:
                // Recordar que hasta 0.5 no dispara
                // Y que a 0 se  vuelve majara
                if(PlayerReference.playerControl.ChargedAmount > 0.5f)
                    proyectileMuzzleSpeed = gameManager.canonBaseMuzzleSpeed * PlayerReference.playerControl.ChargedAmount;
                break;
        }
        //
        float dragToCheck = (PlayerReference.currentProyectileRB != null) ? PlayerReference.currentProyectileRB.drag : 0.1f;
        //
        EnemyAnalyzer.estimatedToHitPosition = GeneralFunctions.AnticipateObjectivePositionForAiming(
            transform.position, targetPoint, enemyRigidbody.velocity, proyectileMuzzleSpeed, dt,
            dragToCheck);
        // Determinamos el 
        // TODO: Coger el punto de disparo del plauer
        EnemyAnalyzer.estimatedToHitPosition.y += GeneralFunctions.GetProyectileFallToObjective(transform.position,
            EnemyAnalyzer.estimatedToHitPosition, gameManager.rapidFireMuzzleSpeed);
            //
        targetPos = EnemyAnalyzer.estimatedToHitPosition;
    }

    /// <summary>
    /// fdsfs
    /// </summary>
    /// <returns></returns>
    Transform[] GetNearestEnemiesInScreenAndWorld()
    {
        //
        Transform[] enemiesToReturn = new Transform[2];
        //
        EnemyConsistency[] enemies = FindObjectsOfType<EnemyConsistency>();
        if (enemies.Length == 0)
            return null;
        //
        int nearestScreenEnemy = -1;
        int nearestWorldEnemy = -1;
        float minScreenDistance = Mathf.Infinity;
        float minWorldDistance = Mathf.Infinity;
        //
        for (int i = 0; i < enemies.Length; i++)
        {
            // Distancia al centro de pantalla
            Vector3 posInScreen = cameraComponent.WorldToViewportPoint(enemies[i].transform.position);
            // float distanceToCenter = Mathf.Sqrt(Mathf.Pow(posInScreen.x - 0.05f, 2) + Mathf.Pow(posInScreen.y - 0.05f, 2));
            float distanceToCenter = Mathf.Pow(posInScreen.x - 0.5f, 2) + Mathf.Pow(posInScreen.y - 0.5f, 2);
            //bool inScreen = posInScreen.x >= 0 && posInScreen.x <= 1 && posInScreen.y >= 0 && posInScreen.y <= 1;
            //bool inScreen = posInScreen.z > 0;
            bool inScreen = posInScreen.x >= 0 && posInScreen.x <= 1 && 
                posInScreen.y >= 0 && posInScreen.y <= 1 && 
                posInScreen.z > 0;
            //
            EnemyConsistency enemyConsistency = enemies[i].GetComponent<EnemyConsistency>();
            //
            //Debug.Log(enemies[i].transform.name + ", in " + posInScreen);
            if (inScreen && distanceToCenter < minScreenDistance
                 && EnemyOnSight(enemies[i].transform.TransformPoint(enemyConsistency.centralPointOffset)))
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
        //
        if(nearestScreenEnemy != -1)
            enemiesToReturn[0] = enemies[nearestScreenEnemy].transform;
        //
        enemiesToReturn[1] = enemies[nearestWorldEnemy].transform;
        //
        return enemiesToReturn;
    }
    
    /// <summary>
    /// Cambiamos entre player y enemigos
    /// En caso de enemigo, coge al más cercano al cnetro de la vista
    /// Y si no al más cercano en el mundo
    /// </summary>
    void CheckSwitchAndEnemies()
    {
        //
        //Transform previousTarget = currentTarget;
        // Pilla vas pasadas en una
        // TODO: Revisar bien chequeo
        // O añadir boolean extra para que no se raye
        if (inputManager.MarkObjectiveButton)
        {
            // 
            if(currentTarget == targetPlayer)
            {
                Transform[] nearestEnemies = GetNearestEnemiesInScreenAndWorld();

                // Ñapa
                if (nearestEnemies == null)
                    return;

                // The nearest enemy to the screen center if there is
                if(nearestEnemies[0] != null)
                {
                    SwitchTarget(nearestEnemies[0]);
                    //Debug.Log("Switching from " + previousObjective + " to " + currentTarget + " in screen");
                }
                    
                // And the nearest in world if not
                else if(nearestEnemies[1] != null)
                {
                    SwitchTarget(nearestEnemies[1]);
                    //Debug.Log("Switching from " + previousObjective + " to " + currentTarget + " in world");
                }
                    
            }
            else
            {
                //currentEnemy = null;
                SwitchTarget(null);
                cameraReference.ResetEulerX();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool CheckRightAxis()
    {
        Vector2 rightAxis = inputManager.RightStickAxis;
        if (currentTarget != targetPlayer && rightAxis.magnitude > 0.5f && changeAllowed == true)
        {
            SwitchBetweenEnemies(rightAxis);
            changeAllowed = false;
            return true;
        }
        else if (rightAxis.magnitude > 0.5f)
        {
            // TODO: Hacer esto menos guarro
            // Aqui nada
            // Lo ponemos para que no entre en el else
            return false;
        }
        else
        {
            changeAllowed = true;
            return false;
        }
    }

    /// <summary>
    /// Cambia la vista entre enemigos
    /// Cambia al más cercano en cordenadas de pantalla respecto al dirección elegida
    /// </summary>
    void SwitchBetweenEnemies(Vector2 rightAxis)
    {

        // Si la magnitud es 0 trabajmos con distancia en vez de con angulo
        float axisAngle = 0;
        if (rightAxis.magnitude != 0)
            axisAngle = Mathf.Atan2(rightAxis.y, rightAxis.x);
        //
        EnemyConsistency[] enemies = FindObjectsOfType<EnemyConsistency>();
        //
        if (enemies.Length == 0)
        {
            Debug.Log("No active enemies");
            SwitchTarget();
            cameraReference.ResetEulerX();
            return;
        }
            
        //
        float minimalMeasure = 180;
        float minimalDistance = Mathf.Infinity;
        int nearestEnemy = -1;
        for (int i = 0; i < enemies.Length; i++)
        {
            // Vamos a probar cercanía por ángulo
            Vector3 enemyViewPortCoordinates = cameraComponent.WorldToViewportPoint(enemies[i].transform.position);
            Vector2 coordinatesFromScreenCenter = new Vector2(enemyViewPortCoordinates.x - 0.5f, enemyViewPortCoordinates.y - 0.5f);
            EnemyConsistency enemyConsistency = enemies[i].GetComponent<EnemyConsistency>();
            // TODO: Buscar un método mejor
            // Con angulo en este caso
            if (rightAxis.magnitude > 0.05f)
            {
                // Vamos a hacer un sistema de pesos angulo/distancia
                // Ahora sacamos el angulo
                float angle = Mathf.Atan2(coordinatesFromScreenCenter.y, coordinatesFromScreenCenter.x);
                float distance = coordinatesFromScreenCenter.magnitude;
                int inScreen = (enemyViewPortCoordinates.x >= 0 && enemyViewPortCoordinates.x <= 1 &&
                    enemyViewPortCoordinates.y >= 0 && enemyViewPortCoordinates.y <= 1 &&
                    enemyViewPortCoordinates.z > 0) ? 0 : 1;
                //
                float angleOffset = Mathf.Abs(axisAngle - angle);
                //
                float measure = angleOffset + (distance * 1) + (inScreen * 1000);
                
                //
                if (measure < minimalMeasure && 
                    enemies[i].transform != currentTarget &&
                    EnemyOnSight(enemies[i].transform.TransformPoint(enemyConsistency.centralPointOffset)))
                {
                    minimalMeasure = measure;
                    nearestEnemy = i;
                }
            }
            else // Con distancia en elk otro
            {
                //Debug.Log("Checking if enemy on screen before killing one, pos: " + enemyViewPortCoordinates);
                // Nos asegurarnos primero de que esté en pantalla
                bool inScreen = enemyViewPortCoordinates.x >= 0 && enemyViewPortCoordinates.x <= 1 && 
                    enemyViewPortCoordinates.y >= 0 && enemyViewPortCoordinates.y <= 1 && 
                    enemyViewPortCoordinates.z > 0;
                if (inScreen && 
                    coordinatesFromScreenCenter.magnitude < minimalDistance &&
                    EnemyOnSight(enemies[i].transform.TransformPoint(enemyConsistency.centralPointOffset)))
                {
                    minimalDistance = coordinatesFromScreenCenter.magnitude;
                    nearestEnemy = i;
                }
            }  
        }
        //
        if(nearestEnemy > -1)
        {
            //currentEnemy = enemies[nearestEnemy];
            //currentTarget = enemies[nearestEnemy].transform;
            SwitchTarget(enemies[nearestEnemy].transform);
        }
        else
        {
            //Debug.Log("No more enemies in screen");
            SwitchTarget();
            cameraReference.ResetEulerX();
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
            EnemyAnalyzer.Release();
            //targetOffset.x = 3;
        }
        else
        {
            // TODO: Sustituirlo del todo
            currentTarget = newTarget;
            EnemyAnalyzer.Assign(newTarget);
            //targetOffset.x = 0;
        }
        // In any case
        //targetOffset.y = 0;
    }

    /// <summary>
    /// Coming soon
    /// </summary>
    /// <param name="directionAndForce"></param>
    public void ShakeCamera(Vector3 directionAndForce)
    {

    }

    //
    public void UpdateUp(Vector3 newUp)
    {
        transform.rotation = Quaternion.LookRotation(transform.forward, newUp);
    }

    //
    bool EnemyOnSight(Vector3 enemyPosition)
    {
        //
        RaycastHit hitInfo;
        Vector3 direction = enemyPosition - transform.position;
        //
        if (Physics.Raycast(transform.position, direction, out hitInfo, direction.magnitude))
        {
            //Debug.Log("Object 'on sight': " + hitInfo.transform);
            EnemyCollider enemyCollider = hitInfo.transform.GetComponent<EnemyCollider>();
            if (enemyCollider != null)
                return true;
            EnemyConsistency enemyConsistency = hitInfo.transform.GetComponent<EnemyConsistency>();
            if (enemyConsistency != null)
                return true;
        }
        //
        return false;
    }

    //
    void CheckDontEnterInsideScenario()
    {
        //
        //targetPos = targetPlayer.TransformPoint(targetOffset);
        // TODO: Trabajar que funcione mejor
        Vector3 directionToCheck = positionWithoutCorrection - targetPos;
        RaycastHit hitInfo;
        // Ponemos de momento el punto de inicio del raycast a 10 metros por delante de la cámara
        float magnitudeToUse = Mathf.Min(20, directionToCheck.magnitude);
        Vector3 rayOrigin = positionWithoutCorrection - Vector3.ClampMagnitude(directionToCheck, magnitudeToUse);
        //
        Debug.DrawRay(rayOrigin, directionToCheck, Color.cyan);
        // TODO: Guardar lo posición en la que estaría de no ser corregida
        // CHECK: Si filtrar o no la layer de enemy (9) (se salta también la arena por alguna razón)
        if (Physics.Raycast(rayOrigin, directionToCheck, out hitInfo, magnitudeToUse))
        {
            transform.position = Vector3.Lerp(rayOrigin, hitInfo.point, 1f);
        }
        else
        {
            transform.position = positionWithoutCorrection;
        }
    }
    #endregion
}

// De momento lo hacemos aqui
// Ya que solo es uno a la vez vamos a hacerlo estático
public static class EnemyAnalyzer
{
    public static Transform enemyTransform;
    public static Rigidbody enemyRb;
    public static EnemyConsistency enemyConsistency;
    public static Vector3 estimatedToHitPosition;
    public static bool isActive = false;

    public static void Assign(Transform enemyReference)
    {
        enemyTransform = enemyReference;
        enemyRb = enemyReference.GetComponent<Rigidbody>();
        enemyConsistency = enemyReference.GetComponent<EnemyConsistency>();
        isActive = true;
    }

    public static void Release()
    {
        enemyTransform = null;
        enemyRb = null;
        enemyConsistency = null;
        isActive = false;
    }
}