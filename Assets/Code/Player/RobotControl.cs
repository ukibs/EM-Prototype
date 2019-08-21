using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackMode
{
    Invalid = -1,
    
    RapidFire,
    Pulse,
    Canon,    
    ParticleCascade,
    //Sharpnel,

    Count
}

public enum DefenseMode
{
    Invalid = -1,

    Spheric,
    Front,

    Count
}

public enum JumpMode
{
    Invalid = -1,

    RepulsorJump,
    ChargedJump,
    Smash,

    Count
}

public enum SprintMode
{
    Invalid = -1,

    Normal,
    Adherence,
    //Hook,

    Count
}

public enum ActionCharguing
{
    Invalid = -1,

    None,
    Attack,
    Jump,
    Sprint,
    Defense,

    Count
}

public class RobotControl : MonoBehaviour {
    
    public float damping = 0.4f;

    public GameObject chargingPulseEmitter;
    public GameObject releasingPulseEmitter;

    // Luego unificamos estas 3
    public GameObject sphericProyectilePrefab;
    public GameObject elipticProyectilePrefab;
    //public GameObject cannonBallPrefab;
    //public GameObject piercingProyectilePrefab;

    public Transform[] machineGunPoints;
    public Transform chargedProyectilePoint;
    public GameObject chargingProjectile;       // Habrá que pulir como manejamos esto
    public GameObject sphericShield;
    public GameObject frontShield;

    // De momento lo ponemos aqui
    public GameObject shootParticlePrefab;

    //
    public AudioClip loadingClip;
    public AudioClip loadingAtMaxClip;
    public AudioClip releasingPulseClip;
    public AudioClip releasingCanonClip;
    public AudioClip rapidFireClip;
    public AudioClip overHeatClip;

    // TODO: Mover a datos de game manager
    public float rotationTime = 0.1f;


    //
    private Transform mainCamera;
    private Rigidbody rb;

    private AttackMode attackMode = (AttackMode)0;
    private DefenseMode defenseMode = (DefenseMode)0;
    private JumpMode jumpMode = (JumpMode)0;
    private SprintMode sprintMode = (SprintMode)0;
    private ActionCharguing actionCharging = ActionCharguing.None;

    private float chargedAmount = 0.0f;    // Por ahora lo manejaremos entre 0 y 1
    private float currentMuzzleSpeed = 0;
    private float currentOverheat = 0;      // Similar
    private bool totalOverheat = false;
    private SpringCamera cameraControl;
    private InputManager inputManager;
    private Repulsor repulsor;
    private GameManager gameManager;
    private ImpactInfoManager impactInfoManager;
    private bool inPlay = true;

    //
    private GameObject proyectileToUse;
    private Rigidbody proyectileRb;

    // De momento lo controlamos con un bool
    private bool applyingDamping = true;
    private bool adhering = false;
    //
    private float rapidFireCooldown = 0;
    private int nextRapidFireSide = 0;
    //
    private AudioSource audioSource;

    // Testeo
    //private bool paused = false;

    //
    private Quaternion objectiveRotation;
    private Vector3 lastAxisXZ;
    private float currentRotationTime;

    // Damping tridimensinal para que el retroceso de las armas no nos mande al espacio
    private bool threeDimensionalDamping = false;
    private float tddCurrentDuration = 0;
    private float tddMaxDuration = 1;

    #region Properties

    public float ChargedAmount { get { return chargedAmount; } }
    public float CurrentOverHeat { get { return currentOverheat; } }
    public bool TotalOverheat { get { return totalOverheat; } }
    public ActionCharguing CurrentActionCharging { get { return actionCharging; } }

    public AttackMode ActiveAttackMode {
        get { return attackMode; }
        set { attackMode = value; }
    }
    public DefenseMode ActiveDefenseMode {
        get { return defenseMode; }
        set { defenseMode = value; }
    }
    public SprintMode ActiveSprintMode {
        get { return sprintMode; }
        set { sprintMode = value; }
    }
    public JumpMode ActiveJumpMode {
        get { return jumpMode; }
        set { jumpMode = value; }
    }

    public bool InPlay {
        get { return inPlay; }
        set { inPlay = value; }
    }

    public bool Adhering { get { return adhering; } }

    public bool IsResting
    {
        get
        {
            return chargedAmount == 0 && rb.velocity.magnitude <= 1;
        }
    }

    public float CurrentMuzzleSpeed { get { return currentMuzzleSpeed; } }

    public GameObject ProyectileToUse { get { return proyectileToUse; } }

    #endregion

    // Use this for initialization
    void Start () {
        mainCamera = Camera.main.transform;
        inputManager = FindObjectOfType<InputManager>();
        cameraControl = mainCamera.GetComponent<SpringCamera>();
        rb = GetComponent<Rigidbody>();
        repulsor = GetComponent<Repulsor>();
        gameManager = FindObjectOfType<GameManager>();
        audioSource = GetComponent<AudioSource>();
        impactInfoManager = FindObjectOfType<ImpactInfoManager>();
        
        //
        PlayerReference.Initiate(gameObject);

        //
        lastAxisXZ = Vector3.forward;

        //
        proyectileToUse = elipticProyectilePrefab;
        proyectileRb = proyectileToUse.GetComponent<Rigidbody>();
        PlayerReference.currentProyectileRB = proyectileRb;

        // Recordar que la masa va en gramos (de momento)
        currentMuzzleSpeed = gameManager.forcePerSecond / (gameManager.massPerSecond / 1000);
        // Debug.Log("Muzzle speed :" + currentMuzzleSpeed);
    }

    // Vamos a probar asi el tdd
    void FixedUpdate()
    {
        //threeDimensionalDamping = false;
    }

    // Update is called once per frame
    void Update () {
        //
        if (inPlay)
        {
            //
            float dt = Time.deltaTime;
            AxisMotion(dt);
            JumpMotion();
            //
            CheckActionChange();
            CheckAndFire(dt);
            CheckDefense();
            //
            UpdateOverheat(dt);  
            // Hay que cambiarlo de sitio
            //if(actionCharging == ActionCharguing.Attack && chargedAmount > 0)
            //{
            //    currentMuzzleSpeed = gameManager.forcePerSecond / gameManager.massPerSecond * chargedAmount;
            //}
            //else
            //{
            //    currentMuzzleSpeed = 0;
            //}
        }
        
    }

    private void OnGUI()
    {
        //GUI.Label(new Rect(20, Screen.height - 30, 200, 20), previousChargeAmount + ", " + chargeAmount);
        //GUI.Label(new Rect(20, Screen.height - 30, 200, 20), actionCharging + ", " + chargedAmount);
    }

    private void OnDrawGizmos()
    {
       
        //Debug.DrawRay(transform.position, rb.velocity, Color.blue);
        //Vector3 playerDirection = player.transform.position - transform.position;
        Debug.DrawRay(transform.position, transform.forward * 300, Color.red);

    }

    private void OnCollisionEnter(Collision collision)
    {
        // Nota: Igual hay que aplicar este cambio también por tiempo
        applyingDamping = true;
    }

    #region Methods

    /// <summary>
    /// 
    /// </summary>
    void UpdateOverheat(float dt)
    {
        // TODO: Lo tenemos repe por arriba para el impact info manager. Unificarlo
        bool isRapidFiring = (actionCharging == ActionCharguing.Attack && inputManager.FireButton);
        //
        if (chargedAmount == 0 || !isRapidFiring || totalOverheat)
        {
            // TODO: Así es un poco ñapa, pulirlo
            float totalOverheatMultiplier = (totalOverheat) ? 1 : 2;
            //
            currentOverheat -= totalOverheatMultiplier * dt;
            currentOverheat = Mathf.Max(currentOverheat, 0);
            //
            if (currentOverheat == 0 && totalOverheat)
                totalOverheat = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void AxisMotion(float dt)
    {
        // First check sprint
        float sprintMultiplier = 1;
        Vector3 currentUp = Vector3.up;
        //
        if (gameManager.unlockedSprintActions > 0)
        {
            RaycastHit adherencePoint;
            
            if (inputManager.SprintButton && actionCharging == ActionCharguing.None)
            {
                switch (sprintMode)
                {
                    case SprintMode.Normal:
                        actionCharging = ActionCharguing.Sprint;
                        break;
                    case SprintMode.Adherence:
                        // Let's check if there are a surface near
                        if (AdherenceCheck(out adherencePoint) != false)
                        {
                            // Trabajamos con el punto y la normal
                            //adherencePoint.normal
                            actionCharging = ActionCharguing.Sprint;
                            SetNewUp(adherencePoint.point, adherencePoint.normal);
                            currentUp = adherencePoint.normal;
                            chargedAmount = 1;
                            adhering = true;
                            //cameraControl.
                            Debug.Log("Adherence point found");
                        }
                        break;
                }
            }
            else if (inputManager.SprintButton && actionCharging == ActionCharguing.Sprint)
            {
                switch (sprintMode)
                {
                    case SprintMode.Normal:
                        // De momento el multiplicador irá de 1 a 2
                        chargedAmount += Time.deltaTime;
                        chargedAmount = Mathf.Min(chargedAmount, gameManager.maxCharge);
                        sprintMultiplier += chargedAmount;
                        break;
                    case SprintMode.Adherence:
                        if (AdherenceCheck(out adherencePoint) != false)
                        {
                            // Trabajamos con el punto y la normal
                            //adherencePoint.normal
                            SetNewUp(adherencePoint.point, adherencePoint.normal);
                            currentUp = adherencePoint.normal;
                            Vector3 gravityForce = Vector3.up * -rb.velocity.y;
                            Vector3 adherenceForce = -transform.up * 1;
                            rb.AddForce(gravityForce + adherenceForce, ForceMode.Impulse);
                            //chargedAmount = 1;
                            //cameraControl.
                            Debug.Log("Adherenring to something");
                        }
                        else
                        {
                            Debug.Log("Adherenrce lost");
                            chargedAmount = 0;
                            actionCharging = ActionCharguing.None;
                            adhering = false;
                        }

                        break;
                }

            }
            else if (chargedAmount > 0 && actionCharging == ActionCharguing.Sprint)
            {
                chargedAmount = 0;
                actionCharging = ActionCharguing.None;
                adhering = false;
            }
        }

        // And then the axis
        Vector2 movementAxis = inputManager.StickAxis;
        //
        if (applyingDamping == false && movementAxis.magnitude > Mathf.Epsilon)
            applyingDamping = true;
        //
        Vector3 directionZ = mainCamera.forward * movementAxis.y;
        Vector3 directionX = mainCamera.right * movementAxis.x;

        // The movement direction will depend on the current up
        directionZ = Vector3.ProjectOnPlane(directionZ, currentUp).normalized;
        directionX = Vector3.ProjectOnPlane(directionX, currentUp).normalized;

        // En estos casos el player estará encarado con el objetivo
        if(actionCharging == ActionCharguing.Attack || 
            (actionCharging == ActionCharguing.Defense && defenseMode == DefenseMode.Front) ||
            (actionCharging == ActionCharguing.Jump && jumpMode == JumpMode.Smash))
        {
            // Si no targetea al player
            // Osea un enemigo
            // Puesto aqui por si sacamos más casos (cinematicas por ejemplo)
            if (!cameraControl.TargetingPlayer)
            {
                //
                // TODO: Ajustar el stimated para que pille bien 
                Vector3 pointToLook = EnemyAnalyzer.estimatedToHitPosition;
                transform.LookAt(pointToLook, currentUp);
            }                
            else
                transform.LookAt(transform.position + cameraControl.transform.forward, currentUp);
        }
        else
        {
            // Para que ajuste la rotación poco a poco
            if((directionX + directionZ).magnitude > Mathf.Epsilon){
                currentRotationTime = 0;
                lastAxisXZ = directionX + directionZ;
            }
            //
            objectiveRotation = Quaternion.LookRotation(lastAxisXZ, currentUp);
            currentRotationTime += dt;
            transform.rotation = Quaternion.Slerp(transform.rotation, objectiveRotation, currentRotationTime/rotationTime);
            
            //transform.LookAt(transform.position + directionX + directionZ, currentUp);
        }
            

        // TODO: Tener en cuenta velocity actual
        Vector3 forceDirection = (directionX + directionZ).normalized;
        rb.AddForce( forceDirection * gameManager.movementForcePerSecond * sprintMultiplier);

        // And dampen de movement
        if (applyingDamping)
        {
            if (!threeDimensionalDamping)
            {
                Vector3 currentVelocity = rb.velocity;
                currentVelocity.x *= 1 - (damping * dt);
                currentVelocity.z *= 1 - (damping * dt);
                rb.velocity = currentVelocity;
            }
            else
            {
                //
                rb.velocity *= 1 - (damping * dt);
                //
                tddCurrentDuration += dt;
                if(tddCurrentDuration > tddMaxDuration)
                {
                    threeDimensionalDamping = false;
                    tddCurrentDuration = 0;
                }
            }
        }

    }

    //
    void Activate3DimensionalDamping()
    {
        threeDimensionalDamping = true;
        // Problemente pongamos también efecto de sonido de frenazo
    }

    //
    bool AdherenceCheck(out RaycastHit hitInfo)
    {
        RaycastHit hit;
        float checkRange = 10;

        // TODO: Habrá que decidir cuantos raycast tiramos

        // Simple ones
        if (Physics.Raycast(transform.position, Vector3.up, out hit, checkRange)) { hitInfo = hit; return true; }
        if (Physics.Raycast(transform.position, -Vector3.right, out hit, checkRange)) { hitInfo = hit; return true; }
        if (Physics.Raycast(transform.position, Vector3.right, out hit, checkRange)) { hitInfo = hit; return true; }
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, checkRange)) { hitInfo = hit; return true; }

        //
        //if (Physics.Raycast(transform.position, new Vector3(1, 0, 1), out hit, checkRange)) { hitInfo = hit; return true; }


        // Null response
        hitInfo = hit;
        return false;
    }

    // TODO: Creo que el nombre de esta función no encaja del todo con lo que hace
    // TODO: Revisar que no lo hayamos desarmado
    void SetNewUp(Vector3 point, Vector3 normal)
    {
        //
        objectiveRotation = Quaternion.LookRotation(transform.forward, normal);
        //
        transform.rotation = Quaternion.Slerp(transform.rotation, objectiveRotation, 0.1f);
    }

    /// <summary>
    /// 
    /// </summary>
    void JumpMotion()
    {
        // La primera se desbloquea en el repulsor
        if (gameManager.unlockedJumpActions == 2 || jumpMode == JumpMode.RepulsorJump)
            return;
        // De momento solo hacia arriba
        // Luego trabajaremos más direcciones
        if (inputManager.JumpButton && actionCharging == ActionCharguing.None)
        {
            actionCharging = ActionCharguing.Jump;
            GeneralFunctions.PlaySoundEffect(audioSource, loadingClip);
        }            
        else if(inputManager.JumpButton && actionCharging == ActionCharguing.Jump)
        {
            //
            chargedAmount += Time.deltaTime;
            chargedAmount = Mathf.Clamp01(chargedAmount);
        }
        else if(chargedAmount > 0 && actionCharging == ActionCharguing.Jump)
        {
            // NOTA: Pensar si dejarle saltar en el aire
            // Chequeamos el tipo de salto seleccionado
            switch (jumpMode)
            {
                case JumpMode.ChargedJump:
                    // En ese caso hacer que tenga menos impulso que haciéndolo desde el suelo
                    float floorSupport = (repulsor.IsOnFloor) ? gameManager.jumpForce : 0;
                    // Le damos un mínimo de base
                    rb.AddForce(Vector3.up * (gameManager.jumpForce * chargedAmount + floorSupport), ForceMode.Impulse);
                    //
                    GeneralFunctions.PlaySoundEffect(audioSource, releasingPulseClip);
                    break;
                case JumpMode.Smash:
                    // Le damos un mínimo de base
                    // TODO: Añadir icono
                    // TODO: Clacular bien la dirección
                    // TODO: Aplicar más fuerza y probar
                    Vector3 cameraForward = cameraControl.transform.forward;
                    applyingDamping = false;
                    Vector3 currentVelocity = rb.velocity;
                    // Revisar: Podría ser el player.forward
                    Vector3 desiredDirection = (!cameraControl.TargetingPlayer) ? 
                        (cameraControl.CurrentTarget.position - transform.position) 
                        : cameraForward;
                    // TODO: Revisar
                    Vector3 compensatedDirection = (desiredDirection - currentVelocity).normalized;
                    rb.AddForce(compensatedDirection * (gameManager.jumpForce * chargedAmount + gameManager.jumpForce) * 10, ForceMode.Impulse);
                    //
                    GeneralFunctions.PlaySoundEffect(audioSource, releasingPulseClip);
                    break;
            }
            
            chargedAmount = 0;
            actionCharging = ActionCharguing.None;
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckDefense()
    {
        //
        if (gameManager.unlockedDefenseActions == 0)
            return;
        // De momento solo hacia arriba
        // Luego trabajaremos más direcciones
        if (inputManager.DefenseButton && actionCharging == ActionCharguing.None && gameManager.unlockedDefenseActions > 0)
        {
            // DefenseMode
            actionCharging = ActionCharguing.Defense;
            // Chequeamos el tipo de defensa que utilizamos
            switch (defenseMode)
            {
                case DefenseMode.Spheric:
                    sphericShield.SetActive(true);
                    break;

                case DefenseMode.Front:
                    // TODO: Crear escudo frontal
                    frontShield.SetActive(true);
                    break;
            }
            
            // De momento haremos que la carga empiece a tope en la defensa
            chargedAmount = 1;
        }            
        else if (inputManager.DefenseButton && actionCharging == ActionCharguing.Defense)
        {
            // Bajará con los impactos 
            // Y se recargará con el tiempo
            chargedAmount += Time.deltaTime;
            chargedAmount = Mathf.Clamp01(chargedAmount);
        }
        else if (chargedAmount > 0 && actionCharging == ActionCharguing.Defense)
        {
            chargedAmount = 0;
            actionCharging = ActionCharguing.None;
            // A ver cual hay que desactivar
            switch (defenseMode)
            {
                case DefenseMode.Spheric:
                    sphericShield.SetActive(false);
                    break;

                case DefenseMode.Front:
                    // TODO: Crear escudo frontal
                    frontShield.SetActive(false);
                    break;
            }
        }

    }

    /// <summary>
    /// Check the different action change buttons
    /// </summary>
    void CheckActionChange()
    {
        // Attack actions
        if (inputManager.SwitchWeaponButton && gameManager.unlockedAttackActions > 0)
        {
            //
            attackMode = (AttackMode)(int)attackMode + 1;
            attackMode = (attackMode == AttackMode.Count || 
                            (int)attackMode >= gameManager.unlockedAttackActions) ?
                            (AttackMode)0 : attackMode;
            // Asignamos en el player reference el rigidbody que corresponda
            //switch (attackMode)
            //{
            //    case AttackMode.Pulse:
            //        PlayerReference.currentProyectileRB = null;
            //        break;
            //    case AttackMode.RapidFire:
            //        PlayerReference.currentProyectileRB = proyectilePrefab.GetComponent<Rigidbody>();
            //        break;
            //    case AttackMode.Canon:
            //        PlayerReference.currentProyectileRB = cannonBallPrefab.GetComponent<Rigidbody>();
            //        break;
            //}

            // TODO: Habrá que trabajar esto con la carga variable
            EnemyAnalyzer.RecalculatePenetration();
        }
        // Defensive actions
        if (inputManager.SwitchDefenseButton && gameManager.unlockedDefenseActions > 0)
        {
            //
            defenseMode = (DefenseMode)(int)defenseMode + 1;
            defenseMode = (defenseMode == DefenseMode.Count ||
                            (int)defenseMode >= gameManager.unlockedDefenseActions) ?
                            (DefenseMode)0 : defenseMode;
        }
        // Jump actions
        if (inputManager.SwitchJumpButton && gameManager.unlockedJumpActions > 0)
        {
            //
            jumpMode = (JumpMode)(int)jumpMode + 1;
            jumpMode = (jumpMode == JumpMode.Count ||
                            (int)jumpMode >= gameManager.unlockedJumpActions) ?
                            (JumpMode)0 : jumpMode;
        }
        // Sprint actions
        if (inputManager.SwitchSprintButton && gameManager.unlockedSprintActions > 0)
        {
            //
            sprintMode = (SprintMode)(int)sprintMode + 1;
            sprintMode = (sprintMode == SprintMode.Count ||
                            (int)sprintMode >= gameManager.unlockedSprintActions) ?
                            (SprintMode)0 : sprintMode;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckAndFire(float dt)
    {
        //
        if (gameManager.unlockedAttackActions == 0)
            return;
        //
        if (inputManager.FireButton && actionCharging == ActionCharguing.None && gameManager.unlockedAttackActions > 0)
        {
            actionCharging = ActionCharguing.Attack;
            switch (attackMode)
            {
                // Luego lo trabajamos
                // Poner "proyectil formándose"
                // O partículas del ataque de pulso
                case AttackMode.Pulse:
                    releasingPulseEmitter.SetActive(false);
                    chargingPulseEmitter.SetActive(true);
                    GeneralFunctions.PlaySoundEffect(audioSource, loadingClip);
                    break;
                case AttackMode.Canon:
                    //chargingCanonProyectile.SetActive(false);
                    GeneralFunctions.PlaySoundEffect(audioSource, loadingClip);
                    break;
            }
        }            
        else if (inputManager.FireButton && actionCharging == ActionCharguing.Attack)
        {
            //
            chargedAmount += dt;
            if (attackMode == AttackMode.RapidFire)
            {
                // TODO: Hacerlo un poco menos ñapa
                if (totalOverheat)
                {
                    chargedAmount -= dt;
                    return;
                }
                    
                //
                currentOverheat += dt;
                //
                if(currentOverheat <= gameManager.maxOverheat)
                {
                    //
                    RapidFireAttack(dt);
                }
                else
                {
                    // Meteremos aqui el efecto de sobrecarga
                    totalOverheat = true;
                    currentOverheat = gameManager.maxOverheat;
                    GeneralFunctions.PlaySoundEffect(audioSource, overHeatClip);
                    actionCharging = ActionCharguing.None;
                }
                //
                chargedAmount = Mathf.Min(chargedAmount, 1);
            }
            else if (attackMode == AttackMode.ParticleCascade)
            {
                ParticleCascadeAttack();
            }
            else
            {
                //
                //chargedAmount += Time.deltaTime;
                //
                chargedAmount = Mathf.Min(chargedAmount, 1);
                //
                if (chargedAmount == 1 && audioSource.clip != loadingAtMaxClip)
                    GeneralFunctions.PlaySoundEffect(audioSource, loadingAtMaxClip);
            }
        }
        else if(chargedAmount > 0 && actionCharging == ActionCharguing.Attack) // Release shot
        {
            switch (attackMode)
            {
                case AttackMode.Pulse:
                    chargingPulseEmitter.SetActive(false);
                    releasingPulseEmitter.SetActive(true);
                    // TODO: Revisar este get component
                    ParticleSystem particleSystem = releasingPulseEmitter.GetComponent<ParticleSystem>();
                    particleSystem.Play();
                    //
                    AlternativePulseAttack();
                    //
                    GeneralFunctions.PlaySoundEffect(audioSource, releasingPulseClip);
                    break;
                case AttackMode.Canon:
                    //
                    //if(chargedAmount >= gameManager.canonMinCharge)
                    //{
                    //chargingCanonProyectile.SetActive(false);
                    CharguedProyectileAttack(proyectileToUse, chargedProyectilePoint, dt);
                    //
                    GeneralFunctions.PlaySoundEffect(audioSource, releasingCanonClip);
                    //}                    
                    break;
                case AttackMode.RapidFire:
                    //impactInfoManager.
                    // Igual no hace falta poner nada
                    break;
                // Ya haremos el resto
            }
            chargedAmount = 0;
            actionCharging = ActionCharguing.None;
        }
    }

    //
    void AlternativePulseAttack()
    {
        // TODO: Trabajar estos parámetros
        float coneRadius = 20.0f;
        float coneReach = 50.0f;
        float pulseForceToApply = (gameManager.forcePerSecond * chargedAmount);
        //
        AffectedByPulseAttack elementsOnReachOfPulseAttack = GetElementsOnReachOfPulseAttack(coneReach, coneRadius);
        Vector3 pointFromPlayer;
        // Primero enemy colliders
        for (int i = 0; i < elementsOnReachOfPulseAttack.affectedEnemyColliders.Count; i++)
        {
            pointFromPlayer = elementsOnReachOfPulseAttack.affectedEnemyColliders[i].transform.position - transform.position;
            elementsOnReachOfPulseAttack.affectedEnemyColliders[i].ReceivePulseDamage(pointFromPlayer.normalized * pulseForceToApply);
        }
        // Enemy consistencies
        for (int i = 0; i < elementsOnReachOfPulseAttack.affectedEnemyConsistencies.Count; i++)
        {
            pointFromPlayer = elementsOnReachOfPulseAttack.affectedEnemyConsistencies[i].transform.position - transform.position;
            elementsOnReachOfPulseAttack.affectedEnemyConsistencies[i].ReceivePulseDamage(pointFromPlayer.normalized * pulseForceToApply);
        }
        // Destructible terrains
        for (int i = 0; i < elementsOnReachOfPulseAttack.affectedDestructibleTerrains.Count; i++)
        {
            pointFromPlayer = elementsOnReachOfPulseAttack.affectedDestructibleTerrains[i].transform.position - transform.position;
            elementsOnReachOfPulseAttack.affectedDestructibleTerrains[i].ReceivePulseImpact(pointFromPlayer.normalized * pulseForceToApply);
        }
        // Y rigidbodies
        for (int i = 0; i < elementsOnReachOfPulseAttack.affectedRigidbodies.Count; i++)
        {
            pointFromPlayer = elementsOnReachOfPulseAttack.affectedRigidbodies[i].transform.position - transform.position;
            elementsOnReachOfPulseAttack.affectedRigidbodies[i].AddForce(pointFromPlayer.normalized * pulseForceToApply, ForceMode.Impulse);
        }
        //
        rb.AddForce(-transform.forward * pulseForceToApply, ForceMode.Impulse);
    }

    //
    AffectedByPulseAttack GetElementsOnReachOfPulseAttack(float pulseReach, float pulseRadius)
    {
        //
        AffectedByPulseAttack affectedByPulseAttack = new AffectedByPulseAttack();
        //
        Collider[] collidersOnReach = Physics.OverlapSphere(transform.position, pulseReach);
        //
        for (int i = 0; i < collidersOnReach.Length; i++)
        {
            //Si no entra en el ángulo, siguiente
            Vector3 pointFromPlayer = collidersOnReach[i].transform.position - transform.position;
            if (Vector3.Angle(pointFromPlayer, transform.forward) > pulseRadius)
                continue;

            // Chequemamos primero por enemy colliders
            EnemyCollider enemyCollider = collidersOnReach[i].GetComponent<EnemyCollider>();
            if (enemyCollider != null)
                affectedByPulseAttack.affectedEnemyColliders.Add(enemyCollider);
            // Después enemy consistencies
            EnemyConsistency enemyConsistency = collidersOnReach[i].GetComponent<EnemyConsistency>();
            if(enemyConsistency == null)
                enemyConsistency = collidersOnReach[i].GetComponentInParent<EnemyConsistency>();
            if (enemyConsistency != null)
                affectedByPulseAttack.affectedEnemyConsistencies.Add(enemyConsistency);
            // Terrenos destruibles
            DestructibleTerrain destructibleTerrain = collidersOnReach[i].GetComponent<DestructibleTerrain>();
            if (destructibleTerrain == null)
                destructibleTerrain = collidersOnReach[i].GetComponentInParent<DestructibleTerrain>();
            if (destructibleTerrain != null)
                affectedByPulseAttack.affectedDestructibleTerrains.Add(destructibleTerrain);
            // Y por último rigidbodies
            // Estos solo deberían entrar en la lista si no ha cuajado arriba
            Rigidbody rigidbody = collidersOnReach[i].GetComponent<Rigidbody>();
            if (rigidbody != null && enemyConsistency == null)
                affectedByPulseAttack.affectedRigidbodies.Add(rigidbody);
                
        }
        return affectedByPulseAttack;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dt"></param>
    void RapidFireAttack(float dt)
    {
        //
        rapidFireCooldown += dt;
        //
        if (rapidFireCooldown >= 1 / gameManager.rapidFireRate)
        {
            // Esta vez el muzzlespeed lo calculamos en vez de aplicarlo
            //float muzzleSpeed = gameManager.forcePerSecond / gameManager.massPerSecond * chargedAmount;

            // La calculamos desde los puntos de la ametralladora para más precision
            // TODO: Revisar aqui tambien el cambio de centralPointOffset
            if (EnemyAnalyzer.isActive)
                EnemyAnalyzer.estimatedToHitPosition = GeneralFunctions.AnticipateObjectivePositionForAiming(
                machineGunPoints[nextRapidFireSide].position,
                EnemyAnalyzer.enemyTransform.position,
                //EnemyAnalyzer.enemyTransform.TransformPoint(EnemyAnalyzer.enemyConsistency.centralPointOffset), 
                EnemyAnalyzer.enemyRb.velocity,
                currentMuzzleSpeed, dt);

            //
            float bulletDrag = proyectileRb.drag;
            EnemyAnalyzer.estimatedToHitPosition.y += GeneralFunctions.GetProyectileFallToObjective(transform.position,
                EnemyAnalyzer.estimatedToHitPosition, currentMuzzleSpeed, bulletDrag);
            
            //
            Vector3 shootForward = (!cameraControl.TargetingPlayer) ?
                (EnemyAnalyzer.estimatedToHitPosition - machineGunPoints[nextRapidFireSide].position).normalized :
                machineGunPoints[nextRapidFireSide].forward;
                
            //
            Vector3 targetPoint;
            if (EnemyAnalyzer.isActive)
                targetPoint = EnemyAnalyzer.estimatedToHitPosition;
            else
                targetPoint = cameraControl.CurrentTarget.position;
            //
            Quaternion shootRotation = Quaternion.LookRotation(targetPoint - machineGunPoints[nextRapidFireSide].position);

            //// Recordar que estamos pasando de gramos a toneladas
            //proyectileRb.mass = gameManager.massPerSecond * chargedAmount / 1000000;
            ////float muzzleSpeed = gameManager.rapidFireMuzzleSpeed;
            //float forceToApply = gameManager.forcePerSecond * chargedAmount;

            ////GeneralFunctions.ShootProjectile(bulletPrefab, machineGunPoints[nextRapidFireSide].position,
            ////    shootRotation, shootForward, muzzleSpeed, dt, ShootCalculation.MuzzleSpeed);
            //GeneralFunctions.ShootProjectile(proyectileToUse, machineGunPoints[nextRapidFireSide].position,
            //    shootRotation, shootForward, forceToApply, dt, ShootCalculation.Force);

            //// Aplicamos la fuerza opuesta a EM
            //rb.AddForce(-machineGunPoints[nextRapidFireSide].forward * forceToApply, ForceMode.Impulse);
            //Activate3DimensionalDamping();

            //
            CharguedProyectileAttack(proyectileToUse, machineGunPoints[nextRapidFireSide], dt);

            // TODO: Vamos a aplicar el overheat aqui
            //chargedAmount -= 1 / gameManager.rapidFireRate;
            chargedAmount = 0.01f;
            rapidFireCooldown -= 1 / gameManager.rapidFireRate;
            //
            nextRapidFireSide = (nextRapidFireSide) == 0 ? 1 : 0;
            //
            GeneralFunctions.PlaySoundEffect(audioSource, rapidFireClip);
        }
    }

    /// <summary>
    /// Lanza una corriente de partículas de corto alcance
    /// En cristiano: Ametralladora de granos de arena que se funden por la velocidad a la que salen disparados
    /// </summary>
    void ParticleCascadeAttack()
    {

        // De momento tiro por frame. Es super rápida
        // Nota: En realidad 60 disparos por segundo tampoco es tanto
        for (int i = 0; i < machineGunPoints.Length; i++)
        {

            Vector3 shootForward = (!cameraControl.TargetingPlayer) ?
                (cameraControl.CurrentTarget.position - machineGunPoints[i].position).normalized :
                machineGunPoints[i].forward;

            // TODO: Hcaer más optimo
            EnemyConsistency enemyConsistency = EnemyAnalyzer.enemyConsistency;
            Vector3 targetPoint = cameraControl.CurrentTarget.position;
            if (enemyConsistency != null)
            {
                targetPoint += enemyConsistency.centralPointOffset;
            }
            //
            //Quaternion shootRotation = Quaternion.LookRotation(targetPoint - machineGunPoints[i].position);

            // TODO: Mover esto a general functions
            // Lo haremos con raycast
            // Digamos que la partícula solo aguanta un step antes de desintegrarse
            RaycastHit hitInfo;
            // De momento decidmos 50 de alcance
            // Lo que vendría a ser 3000 m/s
            // Debug.Log("Shooting");
            if (Physics.Raycast(machineGunPoints[i].position, shootForward, out hitInfo, 50))
            {
                //Debug.Log("Impact on" + hitInfo.collider.name);
                // Este va a ser más complicado de aligerar
                EnemyCollider enemyCollider = hitInfo.collider.GetComponent<EnemyCollider>();
                if (enemyCollider != null)
                {
                    //Debug.Log("On enemy");
                    //EnemyConsistency mainBody = enemyCollider.GetComponentInParent<EnemyConsistency>();
                    //if(mainBody != null)
                    //{
                    //    //
                    //    float impactDistance = (hitInfo.point - machineGunPoints[i].position).magnitude;
                    //    // El 50 lo meteremos como parametro
                    //    float particleDensity = 1600;  // kg/m3
                    //    float particleInitialVolume = 1 * Mathf.Pow(10, -9); // un milímetro cúbico
                    //    float particleRelativeVolume = 1 - (impactDistance / 50); // Tenemos en cuenta que se va desintegrando por el camnio
                    //    float particleMass = particleDensity * particleInitialVolume * particleRelativeVolume;
                    //    float particleAcceleration = Mathf.Pow(3000, 2); // Tenemos en cuenta que el proyectil frena en seco al impactar
                    //    float heatBonus = 3f;    // Este me lo invento un poco. Lo investigaremos
                    //    float impactForce = particleMass * particleAcceleration / 2 * heatBonus;
                    //    //Debug.Log("Sand impact force: " + impactForce + ". Mass: " + particleMass + ". Acceleration: " + particleAcceleration);
                    //    // TODO: Trabajar también velocidad relativa
                    //    mainBody.ReceiveInternalImpact(impactForce, hitInfo.point, enemyCollider.armor);
                    //}

                    // Vamos a hacer que pele armadura
                    enemyCollider.Armor--;

                }
                else
                {
                    // Haremos algún efecto en el punto de impacto
                }
                //
                Instantiate(shootParticlePrefab, hitInfo.point, Quaternion.identity);
            }
            //
            chargedAmount = 0.1f;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CharguedProyectileAttack(GameObject proyectilePrefab, Transform muzzlePoint, float dt)
    {
        // Establecemos la masa
        proyectileRb.mass = gameManager.massPerSecond * chargedAmount / 1000000;
        // Y la fuerza a aplicar
        float forceToApply = gameManager.forcePerSecond * chargedAmount;
        //
        GameObject nextProyectile = GeneralFunctions.ShootProjectile(proyectilePrefab, muzzlePoint.position, muzzlePoint.rotation,
            muzzlePoint.forward, forceToApply, dt, ShootCalculation.Force);
        // TODO: Revisar diametro, densidad, etc
        Bullet bulletComponent = nextProyectile.GetComponent<Bullet>();
        //
        float volume = proyectileRb.mass / gameManager.currentDensity;
        // =(volume*3/(4*PI()*ratioAB))^(1/3) * 2
        float ratioAB = 2;
        float elipseDiameter = Mathf.Pow((volume * 3 / (4 * Mathf.PI * ratioAB)), 1 / 3) * 2;
        bulletComponent.diameter = elipseDiameter;
        bulletComponent.length = elipseDiameter * ratioAB;
        //
        rb.AddForce(-chargedProyectilePoint.forward * forceToApply, ForceMode.Impulse);
        Activate3DimensionalDamping();
    }

    #endregion

}

// Referencia estática al player para que no haya qye estar haciento 
// GetComponent y FindObjectOfType todo el rato
// Le tenemos aqui de momento
public static class PlayerReference
{
    public static Transform playerTransform;
    public static Rigidbody playerRb;
    public static PlayerIntegrity playerIntegrity;
    public static RobotControl playerControl;
    public static bool isAlive;

    //
    public static int currentAttackAction;
    public static Rigidbody currentProyectileRB;

    public static void Initiate(GameObject playerGO)
    {
        playerTransform = playerGO.transform;
        playerRb = playerGO.GetComponent<Rigidbody>();
        playerIntegrity = playerGO.GetComponent<PlayerIntegrity>();
        playerControl = playerGO.GetComponent<RobotControl>();
        isAlive = true;
    }

    // TODO: Que pueda acceder directamente al gamemanager
    public static float GetCurrentWeaponPenetrationEstimation(/*GameManager gameManager*/)
    {
        
        // Sacamos la info de penetración del arma equipada
        float penetrationCapacity = -1;

        Rigidbody rFProyectileBody = PlayerReference.currentProyectileRB;
        Bullet rfProyectileData = playerControl.ProyectileToUse.GetComponent<Bullet>();
        penetrationCapacity = GeneralFunctions.Navy1940PenetrationCalc(
            rFProyectileBody.mass, rfProyectileData.diameter, playerControl.CurrentMuzzleSpeed);
        //
        return penetrationCapacity;
    } 
}

//
public static class GameControl
{
    public static bool paused;
    public static bool bulletTime;
    //public static float 
}

//
public class AffectedByPulseAttack
{
    public List<EnemyCollider> affectedEnemyColliders;
    public List<EnemyConsistency> affectedEnemyConsistencies;
    public List<DestructibleTerrain> affectedDestructibleTerrains;
    // Tener en cuenta que solo se verán afectados los rigibodies que no sean enemgios activos
    public List<Rigidbody> affectedRigidbodies;

    public AffectedByPulseAttack()
    {
        affectedEnemyColliders = new List<EnemyCollider>(10);
        affectedEnemyConsistencies = new List<EnemyConsistency>(5);
        affectedDestructibleTerrains = new List<DestructibleTerrain>(1);
        affectedRigidbodies = new List<Rigidbody>(5);
    }

}