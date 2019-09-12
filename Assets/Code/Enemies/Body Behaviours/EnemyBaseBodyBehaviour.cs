using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Actions
{
    Invalid = -1,

    GoingToPlayer,
    EncirclingPlayerForward,
    EncirclingPlayerSideward,
    FacingPlayer,
    Fleeing,
    ZigZagingTowardsPlayer,
    Lunging,
    RetreatingFromPlayer,
    ApproachingPlayer3d,
    // Moverse alrededor de un punto

    Count
}

public class EnemyBaseBodyBehaviour : MonoBehaviour
{
    //[ConditionalField("NextState", AIState.Idle)]
    #region Public Attributes

    public float maxSpeed = 10;
    public float minimalLungeDistance = 15;
    public float minimalShootDistance = 100;
    public float ofFootMaxTime = 5;

    public float timeBetweenActionChecking = 1.0f;
    
    [Tooltip("Rotatin in degrees per second.")]
    public float rotationSpeed = 90;
    
    public Actions[] behaviour;     // Luego trabajaremos bien esto

    #endregion

    #region Protected Attributes

    // Esto para los que hagan zig zag
    protected float currentZigZagDirection = 0;
    protected float currentZigZagVariation = 0.1f;

    //Varaible para determinar si ha paerido el equilibrio
    protected bool ofFoot = false;
    protected float ofFootCurrentTime = 0;

    public EnemyWeapon[] weapons;   // TODO: Que la busque él

    protected RobotControl player;
    protected Rigidbody rb;

    protected Actions currentAction = Actions.GoingToPlayer;
    protected float timeFromLastCheck = 0;

    protected EnemyConsistency bodyConsistency;
    protected TerrainManager terrainManager;
    protected AudioSource audioSource;

    // De momento lo hacemos con posiciones
    protected List<Waypoint> pathToUse;
    protected List<Vector3> pathToPlayer;

    //
    protected bool onFloor = true;

    // Multilicador para cuando haya extemidades dañables
    // 1 - Óptimo, 0 - Inmovibilizado
    protected float movementStatus = 1;

    #endregion

    #region Properties

    public float MovementStatus
    {
        get { return movementStatus; }
        set { movementStatus = value; }
    }

    #endregion

    // Start is called before the first frame update
    protected virtual void Start()
    {
        player = FindObjectOfType<RobotControl>();
        rb = GetComponent<Rigidbody>();
        bodyConsistency = GetComponent<EnemyConsistency>();
        terrainManager = FindObjectOfType<TerrainManager>();
        audioSource = GetComponent<AudioSource>();
    }

    protected virtual void FixedUpdate()
    {
        // Lo ponemos aqui a false
        // Si está en contacto con terreno se pondrá a true en el collision enter/stay
        onFloor = false;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //
        float dt = Time.deltaTime;
        // TODO: Que el offoot sea directamente aplicado por acciones del player
        //if (bodyConsistency.ReceivedStrongImpact)
        //{
        //    //
        //    Debug.Log(gameObject.name + " set off foot");
        //    //
        //    ofFoot = true;
        //    ofFootCurrentTime = 0;
        //}
        UpdateOfFootStatus(dt);

        // Que el player siga vivo
        if (player != null)
        {
            //
            timeFromLastCheck += dt;
            // Si huyen qe no chequeen mas acciones
            if (timeFromLastCheck > timeBetweenActionChecking && currentAction != Actions.Fleeing)
            {
                CheckActionToDo();
                DecideActionToDo();
                timeFromLastCheck -= timeBetweenActionChecking;
            }

            //
            ExecuteCurrentAction(dt);
        }        
        
    }

    //
    protected virtual void OnCollisionEnter(Collision collision)
    {
        //
        if (collision.collider.tag == "Sand" || collision.collider.tag == "Hard Terrain")
            onFloor = true;
    }

    //
    protected virtual void OnCollisionStay(Collision collision)
    {
        //
        if (collision.collider.tag == "Sand" || collision.collider.tag == "Hard Terrain")
            onFloor = true;
    }

    protected virtual void OnDrawGizmos()
    {
        //
        if (player != null)
        {
            Debug.DrawRay(transform.position, rb.velocity, Color.blue);
            Vector3 playerDirection = player.transform.position - transform.position;
            Debug.DrawRay(transform.position, playerDirection, Color.red);
            //
            if(currentAction == Actions.GoingToPlayer && pathToUse != null && pathToUse.Count > 0)
            {
                Debug.DrawRay(transform.position, pathToUse[0].transform.position - transform.position, Color.yellow);
                if(pathToUse.Count > 1)
                    Debug.DrawRay(pathToUse[0].transform.position, pathToUse[1].transform.position - pathToUse[0].transform.position, Color.yellow);
            }
        }

    }

    protected void UpdateOfFootStatus(float dt)
    {
        // Avanzamos el chequeo de desequilibrio y si está desequilibrado que no pueda actuar
        if (ofFoot)
        {
            ofFootCurrentTime += dt;
            if (ofFootCurrentTime >= ofFootMaxTime)
            {
                ofFootCurrentTime = 0;
                ofFoot = false;
            }
        }
    }

    // 
    protected virtual void Move()
    {
        //
        //if (HasGroundUnderneath())
        if(true)
        {
            //
            Vector3 movingDirection = transform.forward;
            //
            float speedMultiplier = 1;
            //
            switch (currentAction)
            {
                case Actions.EncirclingPlayerForward:
                case Actions.GoingToPlayer:
                    // Aqui nada de momento porque ya es forward por defecto
                    break;
                case Actions.ZigZagingTowardsPlayer:
                    currentZigZagDirection += currentZigZagVariation * Time.deltaTime;
                    if (Mathf.Abs(currentZigZagDirection) >= 1)
                    {
                        currentZigZagVariation *= -1;
                    }
                    movingDirection += transform.right * currentZigZagDirection;
                    movingDirection = movingDirection.normalized;
                    break;
                case Actions.EncirclingPlayerSideward:
                    movingDirection = transform.right;
                    speedMultiplier = 0.2f;
                    break;
                case Actions.RetreatingFromPlayer:
                    movingDirection = -transform.forward;
                    speedMultiplier = 1f;
                    break;
            }
            //
            //rb.velocity = (movingDirection * maxSpeed * speedMultiplier * movementStatus);
            rb.AddForce(movingDirection * maxSpeed * speedMultiplier);
            //
            if (!onFloor)
                rb.velocity += Physics.gravity;
        }
    }
    

    // Reseteamos los daños sufridos
    public virtual void ResetStatus()
    {
        movementStatus = 1;
    }

    //
    protected virtual void ExecuteCurrentAction(float dt)
    {
        // Primero que el player siga vivo, si no mal
        if (player != null)
        {
            //
            //Vector3 playerDirection = player.transform.position - transform.position;
            //playerDirection.y = 0.0f;
            
            //
            switch (currentAction)
            {
                case Actions.FacingPlayer:
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed * movementStatus, dt);
                    break;
                case Actions.GoingToPlayer:
                    // TODO: Cuidado con cuando se cambian los paneles. De momento no es un gran problema, pero habrá que abordarlo
                    //
                    Vector3 currentObjective;
                    // Vamos a añadir que no tenga al player a la vista a ver que tal va
                    if (pathToUse != null && pathToUse.Count > 0 && !PlayerOnSight())
                    {
                        //
                        Vector3 xzDistanceToWaypoint = pathToUse[0].transform.position - transform.position;
                        xzDistanceToWaypoint.y = 0;
                        // Si estamos lo bastante cerca del punto que toca lo descartamos
                        if (xzDistanceToWaypoint.magnitude < 50)
                        {
                            pathToUse.RemoveAt(0);
                            //
                            if (pathToUse.Count == 0)
                            {
                                DecideActionToDo();
                                return;
                            }
                                
                            //
                            Debug.Log("Next waypoint: " + pathToUse[0]);
                        }


                        // Ahora el objetivo lo sacaremos con el path
                        currentObjective = pathToUse[0].transform.position;

                        //
                        transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, currentObjective, rotationSpeed * movementStatus, dt);
                        Move();
                    }
                    else
                    {
                        //
                        transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed * movementStatus, dt);
                        Move();
                    }
                    break;
                case Actions.EncirclingPlayerForward:
                    transform.rotation = GeneralFunctions.UpdateRotationOnCross(transform, player.transform.position, rotationSpeed * movementStatus, dt);
                    //Move();
                    break;
                case Actions.EncirclingPlayerSideward:
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed * movementStatus, dt);
                    Move();
                    break;
                case Actions.Fleeing:
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, -player.transform.position, rotationSpeed * movementStatus, dt);
                    Move();
                    break;
                case Actions.RetreatingFromPlayer:
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed * movementStatus, dt);
                    Move();
                    break;
                case Actions.ApproachingPlayer3d:
                    transform.rotation = GeneralFunctions.UpdateRotation(transform, player.transform.position, rotationSpeed, dt);
                    break;
            }

            // Damp para que no se desmadren
            //float dampForce = 10.0f;
            //rb.velocity = rb.velocity * ( 1 - dampForce * dt);
        }
    }

    /// <summary>
    /// Decide que acción del comportamiento utilizar
    /// </summary>
    protected virtual void DecideActionToDo()
    {
        Vector3 playerDistance = player.transform.position - transform.position;
        for (int i = 0; i < behaviour.Length; i++)
        {
            switch (behaviour[i])
            {
                case Actions.FacingPlayer:
                    if (playerDistance.magnitude < minimalShootDistance)
                    {
                        currentAction = behaviour[i];
                        return;
                    }
                    break;
                case Actions.GoingToPlayer:

                    // Esta no lleva condición
                    currentAction = behaviour[i];
                    //
                    pathToUse = terrainManager.GetPathToPlayer(transform);
                    //TODO: Meter aqui el A*
                    return;
                case Actions.EncirclingPlayerSideward:
                    if (playerDistance.magnitude < minimalShootDistance)
                    {
                        currentAction = behaviour[i];
                        return;
                    }
                    break;
                case Actions.EncirclingPlayerForward:
                    if (playerDistance.magnitude < minimalShootDistance)
                    {
                        currentAction = behaviour[i];
                        return;
                    }
                    break;
                case Actions.RetreatingFromPlayer:
                    if (playerDistance.magnitude < minimalShootDistance)
                    {
                        currentAction = behaviour[i];
                        return;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Ejecutar la acción elegida
    /// </summary>
    protected virtual void CheckActionToDo()
    {
        // De momento aqui snecillo, luego nos curramos más la IA
        float distanceToPlayer = (transform.position - player.transform.position).magnitude; // Ya veremos si hacemos sqrt magnitude para ahorrar
        
    }

    /// <summary>
    /// Cheque para ver si el jugador está a la vista
    /// Para determinar si usar waypoints o no
    /// </summary>
    /// <returns></returns>
    protected bool PlayerOnSight()
    {
        return false;
    }

    // De momento usamos este aqui
    protected bool HasGroundUnderneath()
    {
        // TODO: Declararlo public en generales
        Vector3 heightFromFloor = new Vector3(0, -0.55f, 0);
        Collider[] possibleFloor = Physics.OverlapBox(transform.TransformPoint(heightFromFloor), new Vector3(1, 0.1f, 1));
        // TODO: Pillar el collider/s al princio
        Collider bodyCollider = GetComponent<Collider>();
        //
        for (int i = 0; i < possibleFloor.Length; i++)
        {
            if (possibleFloor[i] != bodyCollider)
                return true;
        }
        //
        return false;
    }

    //
    public void LoseFoot()
    {
        //
        Debug.Log("Losing foot");
        //
        ofFoot = true;
        ofFootCurrentTime = 0;
    }

    //
    protected bool CheckIfObstacleInMovingDirection()
    {
        //
        if(Physics.Raycast(transform.position, rb.velocity, rb.velocity.magnitude * 10))
        {
            return true;
        }
        return false;
    }
}
