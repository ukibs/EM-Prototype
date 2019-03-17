using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackMode
{
    Invalid = -1,

    Pulse,
    RapidFire,
    Canon,    
    ParticleCascade,
    //Sharpnel,
    Piercing,

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

    Normal,
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

    //public float impulseForce = 10.0f;
    //public float jumpForce = 100.0f;
    public float damping = 0.4f;
    //public float pulseForce = 20.0f;
    //public float machineGunCadency = 5;
    public GameObject chargingPulseEmitter;
    public GameObject releasingPulseEmitter;
    public GameObject bulletPrefab;
    public GameObject cannonBallPrefab;
    public GameObject piercingProyectilePrefab;
    public Transform[] machineGunPoints;
    public Transform chargedProyectilePoint;
    public GameObject chargingProjectile;       // Habrá que pulir como manejamos esto
    public GameObject sphericShield;
    public GameObject frontShield;

    // De momento lo ponemos aqui
    public GameObject shootParticlePrefab;

    private Transform mainCamera;
    private Rigidbody rb;

    private AttackMode attackMode = AttackMode.Pulse;
    private DefenseMode defenseMode = DefenseMode.Spheric;
    private JumpMode jumpMode = JumpMode.Normal;
    private SprintMode sprintMode = SprintMode.Normal;
    private ActionCharguing actionCharging = ActionCharguing.None;

    private float chargedAmount = 0.0f;    // Por ahora lo manejaremos entre 0 y 1
    private SpringCamera cameraControl;
    private InputManager inputManager;
    private Repulsor repulsor;
    private GameManager gameManager;
    private bool inPlay = true;

    // De momento lo controlamos con un bool
    private bool applyingDamping = true;
    private bool adhering = false;

    // Testeo
    private bool paused = false;

    #region Properties

    public float ChargedAmount { get { return chargedAmount; } }
    public ActionCharguing CurrentActionCharging { get { return actionCharging; } }

    public AttackMode ActiveAttackMode { get { return attackMode; } }
    public DefenseMode ActiveDefenseMode { get { return defenseMode; } }
    public SprintMode ActiveSprintMode { get { return sprintMode; } }
    public JumpMode ActiveJumpMode { get { return jumpMode; } }

    public bool InPlay {
        get { return inPlay; }
        set { inPlay = value; }
    }

    public bool Adhering { get { return adhering; } }

    #endregion

    // Use this for initialization
    void Start () {
        mainCamera = Camera.main.transform;
        inputManager = FindObjectOfType<InputManager>();
        cameraControl = mainCamera.GetComponent<SpringCamera>();
        rb = GetComponent<Rigidbody>();
        repulsor = GetComponent<Repulsor>();
        gameManager = FindObjectOfType<GameManager>();

        //
#if !UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif
        //
        PlayerReference.Initiate(gameObject);
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
        }
        //
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Log("Triynig to pause");
            if (!paused)
            {
                Time.timeScale = 0;
                paused = true;
            }
            else
            {
                Time.timeScale = 1;
                paused = false;
            }
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
    void AxisMotion(float dt)
    {
        // First check sprint
        float sprintMultiplier = 1;
        RaycastHit adherencePoint;
        Vector3 currentUp = Vector3.up;
        if (inputManager.SprintButton && actionCharging == ActionCharguing.None)
        {
            switch (sprintMode)
            {
                case SprintMode.Normal:
                    actionCharging = ActionCharguing.Sprint;
                    break;
                case SprintMode.Adherence:
                    // Let's check if there are a surface near
                    if(AdherenceCheck(out adherencePoint) != false)
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
        else if(inputManager.SprintButton && actionCharging == ActionCharguing.Sprint)
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
        else if(chargedAmount > 0 && actionCharging == ActionCharguing.Sprint)
        {
            chargedAmount = 0;
            actionCharging = ActionCharguing.None;
            adhering = false;
        }

        // And then the axis
        Vector2 movementAxis = inputManager.StickAxis;
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
                transform.LookAt(EnemyAnalyzer.estimatedToHitPosition, currentUp);
            }                
            else
                transform.LookAt(transform.position + cameraControl.transform.forward, currentUp);
        }
        else
            transform.LookAt(transform.position + directionX + directionZ, currentUp);

        // TODO: Tener en cuenta velocity actual
        Vector3 forceDirection = (directionX + directionZ).normalized;
        rb.AddForce( forceDirection * gameManager.movementForce * sprintMultiplier, ForceMode.Impulse);

        // And dampen de movement
        // TODO: Controlar que no se aplique en ciertos estados
        if (applyingDamping)
        {
            Vector3 currentVelocity = rb.velocity;
            currentVelocity.x *= 1 - damping;
            currentVelocity.z *= 1 - damping;
            rb.velocity = currentVelocity;
        }
        
    }

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

    void SetNewUp(Vector3 point, Vector3 normal)
    {
        transform.rotation = Quaternion.LookRotation(transform.forward, normal);
    }

    /// <summary>
    /// 
    /// </summary>
    void JumpMotion()
    {
        // De momento solo hacia arriba
        // Luego trabajaremos más direcciones
        if (inputManager.JumpButton && actionCharging == ActionCharguing.None)
            actionCharging = ActionCharguing.Jump;
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
                case JumpMode.Normal:
                    // En ese caso hacer que tenga menos impulso que haciéndolo desde el suelo
                    float floorMultiplier = (repulsor.IsOnFloor) ? 1 : 0.5f;
                    // Le damos un mínimo de base
                    rb.AddForce(Vector3.up * (gameManager.jumpForce * chargedAmount + gameManager.jumpForce * floorMultiplier), ForceMode.Impulse);
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
                    rb.AddForce(compensatedDirection * (gameManager.jumpForce * chargedAmount + gameManager.jumpForce * 10), ForceMode.Impulse);
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
                            AttackMode.Pulse : attackMode;
        }
        // Defensive actions
        if (inputManager.SwitchDefenseButton && gameManager.unlockedDefenseActions > 0)
        {
            //
            defenseMode = (DefenseMode)(int)defenseMode + 1;
            defenseMode = (defenseMode == DefenseMode.Count ||
                            (int)defenseMode >= gameManager.unlockedDefenseActions) ?
                            DefenseMode.Spheric : defenseMode;
        }
        // Jump actions
        if (inputManager.SwitchJumpButton && gameManager.unlockedJumpActions > 0)
        {
            //
            jumpMode = (JumpMode)(int)jumpMode + 1;
            jumpMode = (jumpMode == JumpMode.Count ||
                            (int)defenseMode >= gameManager.unlockedDefenseActions) ?
                            JumpMode.Normal : jumpMode;
        }
        // Sprint actions
        if (inputManager.SwitchSprintButton && gameManager.unlockedSprintActions > 0)
        {
            //
            sprintMode = (SprintMode)(int)sprintMode + 1;
            sprintMode = (sprintMode == SprintMode.Count ||
                            (int)defenseMode >= gameManager.unlockedDefenseActions) ?
                            SprintMode.Normal : sprintMode;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckAndFire(float dt)
    {
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
                    break;
                case AttackMode.Canon:
                    //chargingCanonProyectile.SetActive(false);
                    break;
                case AttackMode.Piercing:
                    //chargingPiercingProyectile.SetActive(false);
                    break;
            }
        }            
        else if (inputManager.FireButton && actionCharging == ActionCharguing.Attack)
        {
            //
            chargedAmount += Time.deltaTime;
            if (attackMode == AttackMode.RapidFire)
            {
                RapidFireAttack(dt);
            }
            else if (attackMode == AttackMode.ParticleCascade)
            {
                ParticleCascadeAttack();
            }
            else
            {
                chargedAmount = Mathf.Min(chargedAmount, 1);
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
                    PulseAttack();
                    break;
                case AttackMode.Canon:
                    //chargingCanonProyectile.SetActive(false);
                    CharguedProyectileAttack(cannonBallPrefab, gameManager.canonBaseMuzzleSpeed, dt);                   
                    break;
                case AttackMode.Piercing:
                    //chargingCanonProyectile.SetActive(true);
                    CharguedProyectileAttack(piercingProyectilePrefab, gameManager.piercingBaseMuzzleSpeed, dt);
                    break;
                // Ya haremos el resto
            }
            chargedAmount = 0;
            actionCharging = ActionCharguing.None;
        }
    }

    /// <summary>
    /// Attack that emmits a pulse force
    /// </summary>
    void PulseAttack()
    {
        //
        float coneRadius = 15.0f;
        float coneReach = 20.0f;
        float pulseForceToApply = (gameManager.pulseForce * chargedAmount + gameManager.pulseForce);
        // First sphere check
        RaycastHit[] objectsInRadius = Physics.SphereCastAll(transform.position, coneReach, transform.forward, coneReach);
        for(int i = 0; i < objectsInRadius.Length; i++)
        {
            //Then angle check
            Vector3 pointFromPlayer = objectsInRadius[i].transform.position - transform.position;
            //Debug.Log(pointFromPlayer.magnitude);
            if(Vector3.Angle(pointFromPlayer, transform.forward) < coneRadius)
            {
                // And rigidbody check
                // TODO: Revisar este también
                // Aunque este va a ser más dificil simplificarlo
                Rigidbody objectRb = objectsInRadius[i].transform.GetComponent<Rigidbody>();
                if (objectRb)
                {
                    // Then send them to fly
                    // Nota, tener en cuenta también la distancia para aplicar la fureza
                    Vector3 forceDirection = objectsInRadius[i].transform.position - transform.position;
                    objectRb.AddForce(forceDirection * pulseForceToApply, ForceMode.Impulse);
                }
            }
        }
        // And apply the reaction to the player
        rb.AddForce(-transform.forward * gameManager.pulseForce);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dt"></param>
    void RapidFireAttack(float dt)
    {
        if (chargedAmount >= gameManager.rapidFireRate / 60)
        {
            for (int i = 0; i < machineGunPoints.Length; i++)
            {
                // 
                Vector3 shootForward = (!cameraControl.TargetingPlayer) ?
                    (EnemyAnalyzer.estimatedToHitPosition - machineGunPoints[i].position).normalized :
                    machineGunPoints[i].forward;
                
                //
                Vector3 targetPoint;
                if (EnemyAnalyzer.isActive)
                    targetPoint = EnemyAnalyzer.estimatedToHitPosition;
                else
                    targetPoint = cameraControl.CurrentTarget.position;
                //
                Quaternion shootRotation = Quaternion.LookRotation(targetPoint - machineGunPoints[i].position);

                float muzzleSpeed = gameManager.rapidFireMuzzleSpeed;
                GeneralFunctions.ShootProjectile(bulletPrefab, machineGunPoints[i].position,
                    shootRotation, shootForward, muzzleSpeed, dt, ShootCalculation.MuzzleSpeed);
            }
            // TODO: Revisar por qué no hace esto
            chargedAmount -= gameManager.rapidFireRate / 60;
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
            Quaternion shootRotation = Quaternion.LookRotation(targetPoint - machineGunPoints[i].position);

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
    void CharguedProyectileAttack(GameObject proyectilePrefab, float proyectileMuzzleSpeed, float dt)
    {
        //
        float shootMuzzleSpeed = proyectileMuzzleSpeed * chargedAmount + proyectileMuzzleSpeed;
        // TODO: Coger la masa del game manager
        //float proyectileMass = gameManager.canonBaseProyectileMass * chargedAmount * 10 + gameManager.canonBaseProyectileMass;
        //
        GeneralFunctions.ShootProjectile(proyectilePrefab, chargedProyectilePoint.position, chargedProyectilePoint.rotation,
            chargedProyectilePoint.forward, shootMuzzleSpeed, dt, ShootCalculation.MuzzleSpeed);
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
    public static bool isAlive;

    public static void Initiate(GameObject playerGO)
    {
        playerTransform = playerGO.transform;
        playerRb = playerGO.GetComponent<Rigidbody>();
        playerIntegrity = playerGO.GetComponent<PlayerIntegrity>();
        isAlive = true;
    }
}