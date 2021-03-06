﻿using System.Collections;
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
    GoInFormation,

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

    // TODO: Gestionarlo bien, privado y en propiedades
    public EnemyFormation enemyFormation;

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
    protected EnemyManager enemyManager;

    // De momento lo hacemos con posiciones
    protected List<Waypoint> pathToUse;
    protected List<Vector3> pathToPlayer;

    //
    protected bool onFloor = true;

    // Multilicador para cuando haya extemidades dañables
    // 1 - Óptimo, 0 - Inmovibilizado
    protected float movementStatus = 1;

    // Arma de la formación, sólo la usará si es el líder
    protected WeaponData formationWeaponData;
    protected float formationWeaponCooldown = 0;
    protected GameObject charguingFormationProyectile = null;

    #endregion

    #region Properties

    public float MovementStatus
    {
        get { return movementStatus; }
        set { movementStatus = value; }
    }

    public bool OfFoot { get { return ofFoot; } }

    #endregion

    // Start is called before the first frame update
    protected virtual void Start()
    {
        player = FindObjectOfType<RobotControl>();
        rb = GetComponent<Rigidbody>();
        bodyConsistency = GetComponent<EnemyConsistency>();
        terrainManager = FindObjectOfType<TerrainManager>();
        audioSource = GetComponent<AudioSource>();
        enemyManager = FindObjectOfType<EnemyManager>();
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
        // (GABI): Checkeamos si el jugador está vivo, si está muerto terminamos de procesar la función.
        if (!player) return;
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

        // Uso del arma de la formación si la tiene el bicho
        // De momento lo hacmos aqui
        if (formationWeaponData)
        {
            UpdateFormationWeapon(dt);
        }
    }

    //
    protected virtual void OnCollisionEnter(Collision collision)
    {
        // (GABI): Changed to CompareTag instead
        if (collision.collider.CompareTag("Sand") || collision.collider.CompareTag("Hard Terrain"))
            onFloor = true;
    }

    //
    protected virtual void OnCollisionStay(Collision collision)
    {
        // (GABI): Changed to CompareTag instead
        if (collision.collider.CompareTag("Sand") || collision.collider.CompareTag("Hard Terrain"))
            onFloor = true;
    }

    protected virtual void OnDrawGizmos()
    {
        //

        //
        if (currentAction == Actions.GoingToPlayer && pathToUse != null && pathToUse.Count > 0)
        {
            Debug.DrawRay(transform.position, pathToUse[0].transform.position - transform.position, Color.yellow);
            if (pathToUse.Count > 1)
                Debug.DrawRay(pathToUse[0].transform.position, pathToUse[1].transform.position - pathToUse[0].transform.position, Color.yellow);
            if (pathToUse.Count > 2)
                Debug.DrawRay(pathToUse[1].transform.position, pathToUse[2].transform.position - pathToUse[1].transform.position, Color.yellow);
            if (pathToUse.Count > 3)
                Debug.DrawRay(pathToUse[2].transform.position, pathToUse[3].transform.position - pathToUse[2].transform.position, Color.yellow);
        }
        //
        if (currentAction == Actions.GoInFormation)
        {
            //Vector3 objectivePosition = enemyFormation.GetFormationPlaceInWorld(this);
            //Vector3 objectiveDirection = objectivePosition - transform.position;
            //Debug.DrawRay(transform.position, objectiveDirection, Color.magenta);
            //Gizmos.color = new Color(0, 0, 0, 0.1f);
            //Gizmos.DrawSphere(objectivePosition, 1);
        }
        else
        {
            if (!player) return;
            Vector3 playerDirection = player.transform.position - transform.position;
            Debug.DrawRay(transform.position, playerDirection, Color.red);
            Debug.Log("Current action: " + currentAction);
            //
            if(currentAction == Actions.EncirclingPlayerForward)
            {
                Vector3 playerDirectionCross = Vector3.Cross(transform.up, player.transform.position - transform.position);
                Debug.DrawRay(transform.position, playerDirectionCross, Color.green);
            }
        }
        Debug.DrawRay(transform.position, rb.velocity, Color.blue);
    }

    #region Methods

    protected void UpdateFormationWeapon(float dt)
    {
        // TODO: Gestionarlo menos guarro
        float distanceToPlayer = (transform.position - player.transform.position).magnitude;
        // Ñapa: Le memtemos un pequeño extra para que no se lie tanto con el
        // Encircle player
        if (distanceToPlayer > minimalShootDistance + 50) return;
        //
        formationWeaponCooldown += dt;
        //
        if (formationWeaponData.weapon.preparingProyectilePrefab && !charguingFormationProyectile)
        {
            charguingFormationProyectile = Instantiate(formationWeaponData.weapon.preparingProyectilePrefab,
                                                        transform.TransformPoint(enemyFormation.formationInfo.attackPosition),
                                                        Quaternion.identity);
        }
        else if (charguingFormationProyectile)
        {
            charguingFormationProyectile.transform.localScale = Vector3.one * (formationWeaponCooldown);
            charguingFormationProyectile.transform.position = transform.TransformPoint(enemyFormation.formationInfo.attackPosition);
        }
        //
        if(formationWeaponCooldown >= formationWeaponData.weapon.rateOfFire)
        {
            Debug.Log("Firing formation weapon");
            FireFormationWeapon(dt);
            formationWeaponCooldown = 0;
            // TODO: No debería hacer falta hacer esto
            if (charguingFormationProyectile) Destroy(charguingFormationProyectile);
        }
    }

    protected void FireFormationWeapon(float dt)
    {
        GameObject nextBullet = 
            BulletPool.instance.GetBullet(formationWeaponData.weapon.proyectilePrefab, enemyFormation.FormationStrength);
        //Debug.Log(nextBullet);
        //
        Vector3 attackPosition = transform.TransformPoint(enemyFormation.formationInfo.attackPosition);
        //
        Vector3 anticipatedPlayerPosition = GeneralFunctions.AnticipateObjectivePositionForAiming(
            attackPosition, player.transform.position, PlayerReference.playerRb.velocity, 
            formationWeaponData.weapon.muzzleSpeed, dt);
        // Gravity
        anticipatedPlayerPosition.y -= GeneralFunctions.GetProyectileFallToObjective(attackPosition, anticipatedPlayerPosition,
            formationWeaponData.weapon.muzzleSpeed);

        
        Vector3 attackDirection = anticipatedPlayerPosition - attackPosition;
        //
        GeneralFunctions.ShootProjectileFromPool(nextBullet, attackPosition,
            Quaternion.LookRotation(attackDirection), attackDirection.normalized, formationWeaponData.weapon.muzzleSpeed, dt, ShootCalculation.MuzzleSpeed);
        //
        Missile missile = nextBullet.GetComponent<Missile>();
        if (missile && missile.seeksObjective)
            missile.AssignObjective(player.transform);
    }

    protected void SetFormationProyectileStrength(GameObject nextProyectile)
    {
        Bullet bullet = nextProyectile.GetComponent<Bullet>();
        ExplosiveBullet explosiveBullet = nextProyectile.GetComponent<ExplosiveBullet>();

        bullet.Rb.mass = formationWeaponData.proyectile.maxMass * enemyFormation.FormationStrength;
        if (explosiveBullet)
            explosiveBullet.explosiveLoad = formationWeaponData.proyectile.maxExplosiveLoad * enemyFormation.FormationStrength;
    }

    protected void UpdateOfFootStatus(float dt)
    {
        // Avanzamos el chequeo de desequilibrio y si está desequilibrado que no pueda actuar
        if (!ofFoot) return;
        ofFootCurrentTime += dt;
        if (ofFootCurrentTime < ofFootMaxTime) return;
        ofFootCurrentTime = 0;
        ofFoot = false;
    }

    // 
    protected virtual void Move(float dt)
    {
        // IMPORTANT TODO change: if (!HasGroundUnderneath()) return;
        //                        if (false) return;
        Vector3 movingDirection = transform.forward;
        //
        float speedMultiplier = 1;
        //
        switch (currentAction)
        {
            case Actions.EncirclingPlayerForward:
            case Actions.GoingToPlayer:
            case Actions.ApproachingPlayer3d:
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
            case Actions.GoInFormation:
                // Ajustamos la velocidad dependiendo de la distancia al punto deseado
                Vector3 objectivePosition = enemyFormation.GetFormationPlaceInWorld(this);
                Vector3 objectivePositonOffset = objectivePosition - transform.position;
                float objectivePositionDistance = objectivePositonOffset.magnitude;
                //
                if (objectivePositionDistance < 1)
                    speedMultiplier = 0.9f;
                else if (objectivePositionDistance < 5)
                    speedMultiplier = 1;
                else
                    speedMultiplier = 1.2f;

                //
                //transform.position = enemyFormation.GetFormationPlaceInWorld(this);
                break;
        }
        //
        rb.velocity = (movingDirection * maxSpeed * speedMultiplier * movementStatus);
        //rb.AddForce(movingDirection * maxSpeed * speedMultiplier * dt, ForceMode.Impulse);
        //
        //if (!onFloor)
        //    rb.velocity += Physics.gravity;
    
    }
    

    // Reseteamos los daños sufridos
    public virtual void ResetStatus()
    {
        movementStatus = 1;
    }

    /// <summary>
    /// Executes current action. 
    /// </summary>
    ///
    /// <suggestion>
    /// (GABI):
    /// Maybe use a delegate ?
    /// Maybe refactor this to a variable that changes depending on the state machine ?
    /// Maybe divide each case in a method ?
    /// </suggestion>
    /// 
    /// <param name="dt">Delta time</param>
    protected virtual void ExecuteCurrentAction(float dt)
    {
        // Primero que el player siga vivo, si no mal
        // (GABI): Revisad https://github.com/ukibs/EM-Prototype/issues/1 del por qué hacemos esto de esta manera.
        if (!player) return;
        //
        //Vector3 playerDirection = player.transform.position - transform.position;
        //playerDirection.y = 0.0f;
        
        // TODO: Refactor this.
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
                        //Debug.Log("Next waypoint: " + pathToUse[0]);
                    }


                    // Ahora el objetivo lo sacaremos con el path
                    currentObjective = pathToUse[0].transform.position;

                    //
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, currentObjective, rotationSpeed * movementStatus, dt);
                    Move(dt);
                }
                else
                {
                    //
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed * movementStatus, dt);
                    Move(dt);
                }
                break;
            case Actions.EncirclingPlayerForward:
                transform.rotation = GeneralFunctions.UpdateRotationOnCross(transform, player.transform.position, rotationSpeed * movementStatus, dt);
                Move(dt);
                break;
            case Actions.EncirclingPlayerSideward:
                transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed * movementStatus, dt);
                Move(dt);
                break;
            case Actions.Fleeing:
                transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, -player.transform.position, rotationSpeed * movementStatus, dt);
                Move(dt);
                break;
            case Actions.RetreatingFromPlayer:
                transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed * movementStatus, dt);
                Move(dt);
                break;
            case Actions.ApproachingPlayer3d:
                transform.rotation = GeneralFunctions.UpdateRotation(transform, player.transform.position, rotationSpeed, dt);
                Move(dt);
                break;
            case Actions.GoInFormation:
                Vector3 objectivePosition = enemyFormation.GetFormationPlaceInWorld(this);
                // Aplicamos una rotación de propio para esta acción
                transform.rotation = GeneralFunctions.UpdateRotation(transform, objectivePosition, 360 * movementStatus, dt);
                //transform.rotation = enemyFormation.FormationLeader.transform.rotation;
                Move(dt);
                break;
        }
                
    }

    /// <summary>
    /// Decide que acción del comportamiento utilizar
    /// </summary>
    protected virtual void DecideActionToDo()
    {
        // TODO: Si está en formación y no es el líder que se centre en mantener la posición que le toca
        //if(enemyFormation != null && enemyFormation.FormationLeader != this)
        //{

        //}
        //
        Vector3 playerDistance = player.transform.position - transform.position;
        for (int i = 0; i < behaviour.Length; i++)
        {
            switch (behaviour[i])
            {
                case Actions.EncirclingPlayerSideward:
                case Actions.FacingPlayer:
                case Actions.ApproachingPlayer3d:
                case Actions.EncirclingPlayerForward:
                case Actions.RetreatingFromPlayer:
                    if (playerDistance.magnitude < minimalShootDistance)
                    {
                        currentAction = behaviour[i];
                        Debug.Log("Possible encircling player");
                        return;
                    }
                    break;
                case Actions.GoingToPlayer:

                    // Esta no lleva condición
                    currentAction = behaviour[i];
                    //
                    pathToUse = terrainManager.GetPathToPlayer(transform);

                    return;
                case Actions.GoInFormation:
                    if(enemyFormation != null)
                    {
                        if (enemyFormation.FormationLeader != this)
                        {
                            currentAction = behaviour[i];
                            formationWeaponData = null;
                            return;
                        }
                        // Le asignamos el arma pero no el comportamiento
                        // Ya que será él el que marque el ritmo
                        else
                        {
                            formationWeaponData = enemyFormation.formationInfo.weaponData;
                            // TODO: Chequeo de fuerza de formaciones
                            enemyManager.CheckFusionableFormations(enemyFormation);
                        }
                        
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
        // TODO: Que ignore a otros enemigos
        RaycastHit hitInfo;
        int layerMask = 1 << 9;
        layerMask = ~layerMask;
        Vector3 playerDirection = player.transform.position - transform.position;
        if(Physics.Raycast(transform.position, playerDirection, out hitInfo, playerDirection.magnitude - 1, layerMask))
        {
            //Debug.Log(hitInfo.transform.name + " in path of sight");
            return false;
        }
        return true;
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
        return Physics.Raycast(transform.position, rb.velocity, rb.velocity.magnitude * 10);
    }

    //
    public void LeaveFormation()
    {
        if (enemyFormation != null)
            enemyFormation.LeaveFormation(this);
    }

    #endregion
}
