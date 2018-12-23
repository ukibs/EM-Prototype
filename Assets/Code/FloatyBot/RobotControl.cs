using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackMode
{
    Invalid = -1,

    Pulse,
    RapidFire,
    //ParticleCascade,
    //Sharpnel,
    Canon,
    //Piercing,

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

    public float impulseForce = 10.0f;
    public float jumpForce = 100.0f;
    public float damping = 0.4f;
    public float pulseForce = 20.0f;
    public float machineGunCadency = 5;
    public GameObject chargingPulseEmitter;
    public GameObject releasingPulseEmitter;
    public GameObject bulletPrefab;
    public GameObject cannonBallPrefab;
    public Transform[] machineGunPoints;
    public Transform chargedProyectilePoint;
    public GameObject chargingProjectile;       // Habrá que pulir como manejamos esto
    public GameObject sphericShield;

    // De momento lo ponemos aqui
    public GameObject shootParticlePrefab;

    private Transform mainCamera;
    private Rigidbody rb;
    private AttackMode attackMode = AttackMode.Pulse;
    private DefenseMode defenseMode = DefenseMode.Spheric;
    private ActionCharguing actionCharging = ActionCharguing.None;
    private float chargedAmount = 0.0f;    // Por ahora lo manejaremos entre 0 y 1
    private SpringCamera cameraControl;
    private InputManager inputManager;
    private Repulsor repulsor;
    private GameManager gameManager;

    #region Properties

    public float ChargedAmount { get { return chargedAmount; } }
    public AttackMode ActiveAttackMode { get { return attackMode; } }
    public ActionCharguing CurrentActionCharging { get { return actionCharging; } }
    public DefenseMode DefenseMode { get { return defenseMode; } }

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
    }
	
	// Update is called once per frame
	void Update () {
        //
        float dt = Time.deltaTime;
        AxisMotion();
        JumpMotion();
        //
        CheckWeaponChange();
        CheckAndFire(dt);
        CheckDefense();
    }

    private void OnGUI()
    {
        //GUI.Label(new Rect(20, Screen.height - 30, 200, 20), previousChargeAmount + ", " + chargeAmount);
        GUI.Label(new Rect(20, Screen.height - 30, 200, 20), actionCharging + ", " + chargedAmount);
    }

    #region Methods

    /// <summary>
    /// 
    /// </summary>
    void AxisMotion()
    {
        // First check sprint
        float sprintMultiplier = 1;
        if (inputManager.SprintButton && actionCharging == ActionCharguing.None)
            actionCharging = ActionCharguing.Sprint;
        else if(inputManager.SprintButton && actionCharging == ActionCharguing.Sprint)
        {
            chargedAmount += Time.deltaTime;
            chargedAmount = Mathf.Min(chargedAmount, 1.0f);
            sprintMultiplier += chargedAmount; 
        }
        else if(chargedAmount > 0 && actionCharging == ActionCharguing.Sprint)
        {
            chargedAmount = 0;
            actionCharging = ActionCharguing.None;
        }

        // And then the axis
        Vector2 movementAxis = inputManager.StickAxis;
        Vector3 directionZ = mainCamera.forward * movementAxis.y;
        directionZ.y = 0;
        Vector3 directionX = mainCamera.right * movementAxis.x;
        directionX.y = 0;
        //
        if(actionCharging == ActionCharguing.Attack)
        {
            if (!cameraControl.TargetingPlayer)
                transform.LookAt(cameraControl.CurrentTarget);
            else
                transform.LookAt(transform.position + cameraControl.transform.forward);
        }
        else
            transform.LookAt(transform.position + directionX + directionZ);

        //
        Vector3 forceDirection = (directionX + directionZ).normalized;
        rb.AddForce( forceDirection * impulseForce * sprintMultiplier, ForceMode.Impulse);

        // And dampen de movement
        Vector3 currentVelocity = rb.velocity;
        currentVelocity.x *= 1 - damping;
        currentVelocity.z *= 1 - damping;
        rb.velocity = currentVelocity;
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
            // En ese caso hacer que tenga menos impulso que haciéndolo desde el suelo
            float floorMultiplier = (repulsor.IsOnFloor) ? 1 : 0.5f;
            // Le damos un mínimo de base
            rb.AddForce(Vector3.up * (jumpForce * chargedAmount + jumpForce * floorMultiplier), ForceMode.Impulse);
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
            // TODO: Acuerdate de chequear el tipo de defensa cuando tengamos la otra
            // DefenseMode
            actionCharging = ActionCharguing.Defense;
            sphericShield.SetActive(true);
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
            sphericShield.SetActive(false);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    void CheckWeaponChange()
    {
        if (inputManager.SwitchWeaponButton && gameManager.unlockedAttackActions > 0)
        {
            //
            //unLockedWeapons
            attackMode = (AttackMode)(int)attackMode + 1;
            attackMode = (attackMode == AttackMode.Count || 
                            (int)attackMode >= gameManager.unlockedAttackActions) ? 
                            AttackMode.Pulse : attackMode;
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
                    //
                    ParticleSystem particleSystem = releasingPulseEmitter.GetComponent<ParticleSystem>();
                    particleSystem.Play();
                    //
                    PulseAttack();
                    break;
                case AttackMode.Canon:
                    CharguedProyectileAttack();
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
        float pulseForceToApply = (pulseForce * chargedAmount + pulseForce);
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
        rb.AddForce(-transform.forward * pulseForce);
    }

    void RapidFireAttack(float dt)
    {
        if (chargedAmount >= machineGunCadency / 60)
        {
            for (int i = 0; i < machineGunPoints.Length; i++)
            {
                GameObject newBullet = Instantiate(bulletPrefab, machineGunPoints[i].position, machineGunPoints[i].rotation);
                Rigidbody newBulletRB = newBullet.GetComponent<Rigidbody>();
                newBulletRB.AddForce(transform.forward * 300, ForceMode.Impulse);

                Vector3 shootForward = (!cameraControl.TargetingPlayer) ?
                    (cameraControl.CurrentTarget.position - machineGunPoints[i].position).normalized :
                    machineGunPoints[i].forward;
                //
                //shootForward = machineGunPoints[i].forward;

                // Para chequyear
                Quaternion shootRotation = Quaternion.LookRotation(cameraControl.CurrentTarget.position - machineGunPoints[i].position);

                float muzzleSpeed = 3000;
                GeneralFunctions.ShootProjectile(bulletPrefab, machineGunPoints[i].position,
                    shootRotation, shootForward, muzzleSpeed, dt, ShootCalculation.MuzzleSpeed);
            }
            chargedAmount -= machineGunCadency / 60;
        }
    }

    /// <summary>
    /// Lanza una corriente de partículas de corto alcance
    /// En cristiano: Ametralladora de granos de arena que se funden por la velocidad a la que salen disparados
    /// </summary>
    void ParticleCascadeAttack()
    {
        /*if(chargedAmount >= machineGunCadency / 60)
        {
            for(int i = 0; i < machineGunPoints.Length; i++)
            {
                //GameObject newBullet = Instantiate(bulletPrefab, machineGunPoints[i].position, machineGunPoints[i].rotation);
                //Rigidbody newBulletRB = newBullet.GetComponent<Rigidbody>();
                //newBulletRB.AddForce(transform.forward * 300, ForceMode.Impulse);

                Vector3 shootForward = (!cameraControl.TargetingPlayer) ?
                    (cameraControl.CurrentTarget.position - machineGunPoints[i].position).normalized :
                    machineGunPoints[i].forward;

                // Para chequyear
                Quaternion shootRotation = Quaternion.LookRotation(cameraControl.CurrentTarget.position - machineGunPoints[i].position);

                GeneralFunctions.ShootProjectile(bulletPrefab, machineGunPoints[i].position,
                    shootRotation, shootForward, 0.2f);
            }
            chargedAmount -= machineGunCadency / 60;
        }*/

        // De momento tiro por frame. Es super rápida
        // Nota: En realidad 60 disparos por segundo tampoco es tanto
        for (int i = 0; i < machineGunPoints.Length; i++)
        {

            Vector3 shootForward = (!cameraControl.TargetingPlayer) ?
                (cameraControl.CurrentTarget.position - machineGunPoints[i].position).normalized :
                machineGunPoints[i].forward;

            // Para chequyear
            Quaternion shootRotation = Quaternion.LookRotation(cameraControl.CurrentTarget.position - machineGunPoints[i].position);

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
                EnemyCollider enemyCollider = hitInfo.collider.GetComponent<EnemyCollider>();
                if (enemyCollider != null)
                {
                    //Debug.Log("On enemy");
                    EnemyConsistency mainBody = enemyCollider.GetComponentInParent<EnemyConsistency>();
                    if(mainBody != null)
                    {
                        //
                        float impactDistance = (hitInfo.point - machineGunPoints[i].position).magnitude;
                        // El 50 lo meteremos como parametro
                        float particleDensity = 1600;  // kg/m3
                        float particleInitialVolume = 1 * Mathf.Pow(10, -9); // un milímetro cúbico
                        float particleRelativeVolume = 1 - (impactDistance / 50); // Tenemos en cuenta que se va desintegrando por el camnio
                        float particleMass = particleDensity * particleInitialVolume * particleRelativeVolume;
                        float particleAcceleration = Mathf.Pow(3000, 2); // Tenemos en cuenta que el proyectil frena en seco al impactar
                        float heatBonus = 3f;    // Este me lo invento un poco. Lo investigaremos
                        float impactForce = particleMass * particleAcceleration / 2 * heatBonus;
                        //Debug.Log("Sand impact force: " + impactForce + ". Mass: " + particleMass + ". Acceleration: " + particleAcceleration);
                        // TODO: Trabajar también velocidad relativa
                        mainBody.ReceiveInternalImpact(impactForce, hitInfo.point, enemyCollider.armor);
                    }
                    
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
    void CharguedProyectileAttack()
    {
        GameObject newBullet = Instantiate(cannonBallPrefab, chargedProyectilePoint.position, chargedProyectilePoint.rotation);
        Rigidbody newBulletRB = newBullet.GetComponent<Rigidbody>();
        newBulletRB.AddForce(transform.forward * (10000 * chargedAmount + 1000), ForceMode.Impulse);
    }

    #endregion

}
