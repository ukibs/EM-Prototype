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

    // TODO: Meter parámetros para flexibilidad de cámara

    //public float maxUpperOffsetY = 5.0f;
    //public float maxLowerOffsetY = 3.0f;

    public bool useOnlyUI = false;

    public float transitionTimeBetweenEnemies = 0.3f;
    #endregion

    #region Private Attributes
    private Vector3 vel = new Vector3 (0.0f, 0.0f, 0.0f);
    private Transform currentTarget;
    private Camera cameraComponent;
    private InputManager inputManager;
    //private GameManager gameManager;
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
        // TODO: Preferiría no hacerlo así
        // Puede dar problemas si metemos más de una cámara
        //cameraComponent = GetComponent<Camera>();
        cameraComponent = FindObjectOfType<Camera>();
        //
        inputManager = FindObjectOfType<InputManager>();
        //
        //originalY = targetOffset.y;

        //
        //gameManager = FindObjectOfType<GameManager>();
        //
        positionWithoutCorrection = transform.position;
        //
        cameraReference = FindObjectOfType<CameraReference>();
        // Ojo con ele enemy analyzer
        // Al ser pasivo se puede quedar activado entre escenas
        EnemyAnalyzer.Release();
    }

    //
    private void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        CheckSwitchAndEnemies();
    }

    // Called before physics
    void FixedUpdate () {
        // TODO: Cuidado con este dt
        float dt = Time.deltaTime;
        if (currentTarget != targetPlayer && !EnemyAnalyzer.isActive)
        {
            // TODO: Llamar a función en mundo
            //SwitchBetweenEnemies(Vector2.zero);
            SwitchToNearestInWorldEnemy();
        }
        UpdateMovement(dt);
        AdjustToEnemyMovement(dt);
        UpdateRotation(dt);
        UpdateUp(targetPlayer.up);
        CheckRightAxis();
        CheckDontEnterInsideScenario();
	}

    // Para testeos
    private void OnGUI()
    {
        //GUI.Label(new Rect(10, Screen.height - 30, 150, 20), "Change allowed: " + changeAllowed);
        //
        //GUI.Label(new Rect(10, Screen.height - 30, 300, 20), "Current target: " + currentTarget.name);
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

    //
    void UpdateRotation(float dt)
    {
        // To look at the indicated point
        // Si el target es el player el objetivo viene determinado por los controloes
        if (currentTarget == targetPlayer)
            targetPos = currentTarget.TransformPoint(targetOffset);
        
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
            // TODO: Revisar que falla con el estiamte position
            //transform.LookAt(currentTarget.position);
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
        // TODO: Crear metodo más felxible
        // Trabajar no solo con enemy consistency
        /*if(EnemyAnalyzer.enemyConsistency == null)
        {
            EnemyAnalyzer.Release();
            return;
        }*/

        // TODO: Hemos movido el centralPointOffset a Targeteable
        // Empezamos cogiendo la posición del enemigo
        //Vector3 targetPoint = EnemyAnalyzer.enemyTransform.TransformPoint(EnemyAnalyzer.enemyConsistency.centralPointOffset);
        Vector3 targetPoint = EnemyAnalyzer.enemyTransform.position;

        // TODO: Sacar un índice del arma actualmente equipada para usar su muzzle speed en la función de atnicpar
        Rigidbody enemyRigidbody = EnemyAnalyzer.enemyRb;

        // Determinamos donde va a estar cuando el proyectil llegue a él
        //
        float proyectileMuzzleSpeed = 1000;
        proyectileMuzzleSpeed = PlayerReference.playerControl.CurrentMuzzleSpeed;
        
        //
        float dragToCheck = (PlayerReference.currentProyectileRB != null) ? PlayerReference.currentProyectileRB.drag : 0.1f;
        //
        EnemyAnalyzer.estimatedToHitPosition = GeneralFunctions.AnticipateObjectivePositionForAiming(
            transform.position, targetPoint, enemyRigidbody.velocity, proyectileMuzzleSpeed, dt,
            dragToCheck);
        // Determinamos el 
        // TODO: Coger el punto de disparo del plauer
        EnemyAnalyzer.estimatedToHitPosition.y += GeneralFunctions.GetProyectileFallToObjective(transform.position,
            EnemyAnalyzer.estimatedToHitPosition, proyectileMuzzleSpeed);
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
        // Recuerda, tiene que estar activo
        Targeteable[] enemies = FindObjectsOfType<Targeteable>();
        List<Targeteable> targeteableEnemies = FilterActiveTargeteables(enemies);
        //
        if (targeteableEnemies.Count == 0)
            return null;
        //
        int nearestScreenEnemy = -1;
        int nearestWorldEnemy = -1;
        float minScreenDistance = Mathf.Infinity;
        float minWorldDistance = Mathf.Infinity;
        //
        for (int i = 0; i < targeteableEnemies.Count; i++)
        {
            // Distancia al centro de pantalla
            Vector3 posInScreen = cameraComponent.WorldToViewportPoint(targeteableEnemies[i].transform.position);
            // float distanceToCenter = Mathf.Sqrt(Mathf.Pow(posInScreen.x - 0.05f, 2) + Mathf.Pow(posInScreen.y - 0.05f, 2));
            float distanceToCenter = Mathf.Pow(posInScreen.x - 0.5f, 2) + Mathf.Pow(posInScreen.y - 0.5f, 2);
            //bool inScreen = posInScreen.x >= 0 && posInScreen.x <= 1 && posInScreen.y >= 0 && posInScreen.y <= 1;
            //bool inScreen = posInScreen.z > 0;
            bool inScreen = posInScreen.x >= 0 && posInScreen.x <= 1 && 
                posInScreen.y >= 0 && posInScreen.y <= 1 && 
                posInScreen.z > 0;
            //
            // TODO: Revisar para trabajar con Targeteable
            //EnemyConsistency enemyConsistency = targeteableEnemies[i].GetComponent<EnemyConsistency>();
            //
            //Debug.Log(enemies[i].transform.name + ", in " + posInScreen);
            if (inScreen && distanceToCenter < minScreenDistance
                 && EnemyOnSight(targeteableEnemies[i].transform.TransformPoint(targeteableEnemies[i].centralPointOffset)))
            {
                minScreenDistance = distanceToCenter;
                nearestScreenEnemy = i;
            }
            // Distancia al player
            float enemyDistance = (transform.position - targeteableEnemies[i].transform.position).sqrMagnitude;
            if (enemyDistance < minWorldDistance)
            {

                minWorldDistance = enemyDistance;
                nearestWorldEnemy = i;
            }
        }
        //
        if(nearestScreenEnemy != -1)
            enemiesToReturn[0] = targeteableEnemies[nearestScreenEnemy].transform;
        //
        enemiesToReturn[1] = targeteableEnemies[nearestWorldEnemy].transform;
        //
        return enemiesToReturn;
    }

    //
    public List<Targeteable> FilterActiveTargeteables(Targeteable[] candidates)
    {
        //
        List<Targeteable> filteredTargeteables = new List<Targeteable>(candidates.Length);
        //
        for (int i = 0; i < candidates.Length; i++)
        {
            if (candidates[i].active)
                filteredTargeteables.Add(candidates[i]);
        }
        //
        return filteredTargeteables;
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
            if (EnemyAnalyzer.enemyConsistency && EnemyAnalyzer.enemyConsistency.IsMultipart)
                SwitchBetweenEnemyParts(rightAxis);
            else
                SwitchBetweenEnemies(rightAxis);            
            // Vamos a meter aqui el movimiento entre partes del mismo enemigo
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
        Targeteable[] enemies = FindObjectsOfType<Targeteable>();
        List<Targeteable> targeteableEnemies = FilterActiveTargeteables(enemies);
        //
        if (targeteableEnemies.Count == 0)
        {
            //Debug.Log("No active enemies");
            SwitchTarget();
            cameraReference.ResetEulerX();
            return;
        }
            
        //
        float minimalMeasure = 180;
        float minimalDistance = Mathf.Infinity;
        int nearestEnemy = -1;
        for (int i = 0; i < targeteableEnemies.Count; i++)
        {
            // Vamos a probar cercanía por ángulo
            Vector3 enemyViewPortCoordinates = cameraComponent.WorldToViewportPoint(targeteableEnemies[i].transform.position);
            Vector2 coordinatesFromScreenCenter = new Vector2(enemyViewPortCoordinates.x - 0.5f, enemyViewPortCoordinates.y - 0.5f);
            //EnemyConsistency enemyConsistency = targeteableEnemies[i].GetComponent<EnemyConsistency>();
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
                    targeteableEnemies[i].transform != currentTarget &&
                    EnemyOnSight(targeteableEnemies[i].transform.TransformPoint(targeteableEnemies[i].centralPointOffset)))
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
                    EnemyOnSight(targeteableEnemies[i].transform.TransformPoint(targeteableEnemies[i].centralPointOffset)))
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
            SwitchTarget(targeteableEnemies[nearestEnemy].transform);
        }
        else
        {
            //Debug.Log("No more enemies in screen");
            SwitchTarget();
            cameraReference.ResetEulerX();
        }
    }

    //
    void SwitchBetweenEnemyParts(Vector2 rightAxis)
    {
        // Si la magnitud es 0 trabajmos con distancia en vez de con angulo
        float axisAngle = 0;
        if (rightAxis.magnitude != 0)
            axisAngle = Mathf.Atan2(rightAxis.y, rightAxis.x);
        //
        //EnemyCollider[] bodyColliders =  EnemyAnalyzer.enemyConsistency.BodyColliders;
        List<EnemyCollider> targeteableColliders = EnemyAnalyzer.enemyConsistency.TargeteableColliders;
        //
        float minimalMeasure = 180;
        //float minimalDistance = Mathf.Infinity;
        int nearestEnemy = -1;
        //
        for (int i = 0; i < targeteableColliders.Count; i++)
        {
            // Vamos a probar cercanía por ángulo
            Vector3 enemyViewPortCoordinates = cameraComponent.WorldToViewportPoint(targeteableColliders[i].transform.position);
            Vector2 coordinatesFromScreenCenter = new Vector2(enemyViewPortCoordinates.x - 0.5f, enemyViewPortCoordinates.y - 0.5f);
            // Vamos a hacer un sistema de pesos angulo/distancia
            // Ahora sacamos el angulo
            float angle = Mathf.Atan2(coordinatesFromScreenCenter.y, coordinatesFromScreenCenter.x);
            float distance = coordinatesFromScreenCenter.magnitude;
            //
            float angleOffset = Mathf.Abs(axisAngle - angle);
            //
            float measure = angleOffset + (distance * 1);

            //
            if (measure < minimalMeasure &&
                targeteableColliders[i].transform != currentTarget 
                //&& EnemyOnSight(bodyColliders[i].transform.TransformPoint(bodyColliders[i].centralPointOffset))
                )
            {
                minimalMeasure = measure;
                nearestEnemy = i;
            }
        }
        //
        SwitchTarget(targeteableColliders[nearestEnemy].transform);
    }

    //
    void SwitchToNearestInWorldEnemy()
    {
        //Debug.Log("Switching to nearest enemy in world " + EnemyAnalyzer.lastEnemyPosition);
        //
        Transform enemyToSwitch = null;
        float nearestDistance;
        //
        Targeteable[] enemies = FindObjectsOfType<Targeteable>();
        List<Targeteable> targeteableEnemies = FilterActiveTargeteables(enemies);
        //
        if (targeteableEnemies.Count > 0)
        {
            enemyToSwitch = targeteableEnemies[0].transform;
            nearestDistance = (enemyToSwitch.position - EnemyAnalyzer.lastEnemyPosition).magnitude;
        }
        else
        {
            SwitchTarget();
            return;
        }
            
        //
        for (int i = 1; i < targeteableEnemies.Count; i++)
        {
            float nextOnedistance = (targeteableEnemies[i].transform.position - EnemyAnalyzer.lastEnemyPosition).magnitude;
            if (nextOnedistance < nearestDistance)
            {
                enemyToSwitch = targeteableEnemies[i].transform;
                nearestDistance = nextOnedistance;
            }
        }
        //
        EnemyAnalyzer.Release();
        EnemyAnalyzer.Assign(enemyToSwitch);
        SwitchTarget(enemyToSwitch);
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
            EnemyAnalyzer.Release();
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
        // Algún día
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
        // Ponemos de momento el punto de inicio del raycast a X metros por delante de la cámara
        float magnitudeToUse = Mathf.Min(20, directionToCheck.magnitude);
        Vector3 rayOrigin = positionWithoutCorrection - Vector3.ClampMagnitude(directionToCheck, magnitudeToUse);
        //
        Debug.DrawRay(rayOrigin, directionToCheck, Color.cyan);
        // TODO: Guardar lo posición en la que estaría de no ser corregida
        // CHECK: Si filtrar o no la layer de enemy (9) (se salta también la arena por alguna razón)
        if (Physics.Raycast(rayOrigin, directionToCheck, out hitInfo, magnitudeToUse))
        {
            transform.position = Vector3.Lerp(rayOrigin, hitInfo.point, 0.8f);
        }
        else
        {
            transform.position = positionWithoutCorrection;
        }
    }
    #endregion
}

// De momento lo hacemos aqui
// TODO: Moverlo a un script propio
// Ya que solo es uno a la vez vamos a hacerlo estático
public static class EnemyAnalyzer
{
    public static Transform enemyTransform;
    public static Rigidbody enemyRb;
    public static EnemyConsistency enemyConsistency;
    public static EnemyCollider enemyCollider;
    public static Vector3 estimatedToHitPosition;
    public static Targeteable targeteable;
    public static bool isActive = false;
    public static Vector3 lastEnemyPosition;

    // TODO: Ajustarlo para que trabaje con casos sin rigidbody y/o enemyconsistency
    public static void Assign(Transform enemyReference)
    {
        enemyTransform = enemyReference;
        enemyRb = enemyReference.GetComponent<Rigidbody>();
        // Chequeo extra para multipartes
        if(enemyRb == null)
            enemyRb = enemyReference.GetComponentInParent<Rigidbody>();
        // Chequeo para gusano grande. Debería ser el de la cabeza el que coja
        if (enemyRb == null)
        {
            enemyRb = enemyReference.GetComponentInChildren<Rigidbody>();
            // Para cuando cambias entre partes del cuerpo
            if(enemyRb == null)
                enemyRb = enemyReference.parent.GetComponentInChildren<Rigidbody>();
            //
            else
                enemyTransform = enemyRb.transform;
        }
        //
        enemyConsistency = enemyReference.GetComponent<EnemyConsistency>();
        // Chequeo extra para  las body parts
        if(enemyConsistency == null)
            enemyConsistency = enemyReference.GetComponentInParent<EnemyConsistency>();
        // Para el gusano grande
        if(enemyConsistency == null)
            enemyConsistency = enemyReference.parent.GetComponentInChildren<EnemyConsistency>();
        // Chequeo para los componentes que no lo tienen, como los WeakPoints
        // TODO: Ponerselos más adelante y quitar esto
        if (enemyConsistency != null)
            enemyConsistency.SetCollidersPenetrationColors();
        //
        enemyCollider = enemyReference.GetComponent<EnemyCollider>();
        //
        targeteable = enemyReference.GetComponent<Targeteable>();
        // Chequeo extra para  las body parts
        if (targeteable == null)
            targeteable = enemyReference.GetComponentInParent<Targeteable>();
        isActive = true;
    }

    public static void RecalculatePenetration()
    {
        if(enemyConsistency != null)
            enemyConsistency.SetCollidersPenetrationColors();
    }

    public static void Release()
    {
        //
        //Debug.Log("Releasing enemy");

        // Trabajamos con el transform del targeteable
        if (targeteable != null)
            lastEnemyPosition = targeteable.transform.position;
        //
        enemyTransform = null;
        enemyRb = null;

        // Chequeo para los componentes que no lo tienen, como los WeakPoints
        if (enemyConsistency != null)
            enemyConsistency.SetOriginalPenetrationColors();

        enemyConsistency = null;
        isActive = false;
    }
}