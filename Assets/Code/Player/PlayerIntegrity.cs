using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIntegrity : MonoBehaviour
{
    //
    public GameObject playerFace;
    public Material playerDeadFace;
    //
    public AudioClip shieldDepletionClip;

    private RobotControl robotControl;
    private float currentHealth;
    private float currentShield;
    //private ImpactInfoManager impactInfoManager;
    private GameManager gameManager;
    private Rigidbody bodyRB;
    private ProvisionalHUD hud;
    private AudioSource audioSource;

    //
    //Vector3 previousStepRbVelocity;

    //
    private bool shieldsDepleted = false;
    //private float extraDefense = 0;

    #region Properties

    public float CurrentHealth { get { return currentHealth; } }
    public float CurrentShield {
        get { return currentShield; }
        set {
            currentShield = value;
            currentShield = Mathf.Min(currentShield, gameManager.playerAttributes.maxShield.CurrentValue);
        }
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        robotControl = GetComponent<RobotControl>();
        gameManager = FindObjectOfType<GameManager>();
        bodyRB = GetComponent<Rigidbody>();
        hud = FindObjectOfType<ProvisionalHUD>();
        audioSource = GetComponent<AudioSource>();
        //
        currentHealth = gameManager.playerAttributes.maxHealth;
        currentShield = gameManager.playerAttributes.maxShield.CurrentValue;
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        // De momento hacemos que se recargen con el tiempo
        if (!shieldsDepleted)
        {
            currentShield += dt * gameManager.playerAttributes.shieldRechargeRate;
            currentShield = Mathf.Clamp(currentShield, 0, gameManager.playerAttributes.maxShield.CurrentValue);
        }
        //
        if (shieldsDepleted && robotControl.IsResting)
            shieldsDepleted = false;
        //
        if (robotControl.IsResting)
        {
            currentHealth += dt * gameManager.playerAttributes.repairRate;
            currentHealth = Mathf.Clamp(currentHealth, 0, gameManager.playerAttributes.maxHealth);
        }

        //
        //previousStepRbVelocity = bodyRB.velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Vector3 playerDecceleration = bodyRB.velocity - previousStepRbVelocity;
        // De momento calculamos la fuerza a lo bruto
        // Sin tener en cuenta angulo de colisión
        //float playerImpactForce = playerDecceleration.sqrMagnitude * bodyRB.mass;
        
        ContactPoint collisionPoint = collision.GetContact(0);
        Collider collider = collision.collider;
        GameObject gameObject = collider.gameObject;
        Rigidbody collidingRB = collision.rigidbody;
        //Bullet bulletComponent = gameObject.GetComponent<Bullet>();
        //if (bulletComponent == null)
            ReceiveImpact(collisionPoint.point, gameObject, collidingRB, collisionPoint.normal);
        //else
        //    ReceiveProyectileImpact(bulletComponent, collidingRB, collisionPoint.point);
    }

    #region Methods
    
    // TODO: Hay que coger la normal también
    public void ReceiveImpact(Vector3 contactPoint, GameObject otherGameObject, Rigidbody collidingRB, Vector3 impactNormal)
    {
        // Defensa extra por acciones defensivas
        //float extraDefense = 0;
        ////
        //if (robotControl.CurrentActionCharging == ActionCharguing.Defense)
        //{
        //    switch (robotControl.ActiveDefenseMode)
        //    {
        //        case DefenseMode.Spheric: extraDefense = gameManager.playerAttributes.sphericShieldStrength; break;

        //        case DefenseMode.Front: extraDefense = 9999; break;
        //    }
        //}
        
        // COgemos los dos rigidbodies
        //Rigidbody otherRb = collidingRB;

        //
        Bullet bulletComponent = otherGameObject.GetComponent<Bullet>();

        // Esto tiene pinta de petar en el start
        // Lo montaremos bien para que no pase
        float totalImpactForce = 0;
        
        //
        if (collidingRB != null)
        {
            // TODO: Ver como lo manejamos
            totalImpactForce = collidingRB.velocity.magnitude;
            if(currentShield > 0)
            {
                totalImpactForce = ApplyKineticShield(collidingRB, impactNormal, bulletComponent);
            }
            else
            {
                // TODO: Habrá que ver como manejar esto
                if (bulletComponent != null)
                {
                    totalImpactForce = GeneralFunctions.GetBodyKineticEnergy(totalImpactForce, collidingRB.mass);
                }
                else
                {
                    totalImpactForce = GeneralFunctions.GetCollisionForce(bodyRB, collidingRB);
                }
            }
        }

        // TODO: Averiguar por qué falla
        //Vector3 impactDirection = contactPoint - transform.position;
        Vector3 impactDirection = transform.position - contactPoint;

        // Cogemos el angulo para indicar en el HUD
        float impactAngle = Vector3.SignedAngle(Camera.main.transform.forward, impactDirection, transform.up);

        //
        float impactDamage = Mathf.Max(totalImpactForce/* - extraDefense*/, 0);
        //Debug.Log("Impact from " + otherGameObject.name + ", damage " + impactDamage);
        
        // De momento no visualizamos info del daño que recibimos
        SufferDamage(impactDamage, impactAngle);
    }

    // TODO: Aplicarlo también a impactos que no sean de bala
    // Manejarlo aquí cuando usemos el escudo
    float ApplyKineticShield(Rigidbody collidingRb, Vector3 impactNormal, Bullet bulletComponent)
    {
        //
        //Debug.Log("Colliding rigidbody: " + collidingRb.transform.name);
        //
        float totalImpactForce;
        if (bulletComponent != null)
            totalImpactForce = GeneralFunctions.GetBodyKineticEnergy(collidingRb);
        else
            totalImpactForce = GeneralFunctions.GetCollisionForce(bodyRB, collidingRb);
        //
        Vector3 repulseDirection = Vector3.Reflect(collidingRb.velocity.normalized, impactNormal);
        // Primero sacamos el angulo entre la dirección del impacto y la normal
        float impactAngle = Vector3.Angle(repulseDirection, impactNormal);
        // TODO: Los casos de más de 90 deben de ser proyectiles que tocan a EM después de "atravesarlo"
        // De momento lo ignoramos
        // Lo revisaremos más adelante
        float receivedForce;
        if(impactAngle <= 90)
        {
            //
            float proportionalForceBecauseAngle = Mathf.Cos(impactAngle);
            receivedForce = proportionalForceBecauseAngle * totalImpactForce;
            //
            collidingRb.AddForce(repulseDirection * collidingRb.mass);
            bodyRB.AddForce(-repulseDirection * collidingRb.mass);
            robotControl.ChangeDampingType(DampingType.ThreeDimiensional);
        }
        else
        {
            receivedForce = totalImpactForce;
            bodyRB.AddForce(collidingRb.velocity * collidingRb.mass, ForceMode.Impulse);
            robotControl.ChangeDampingType(DampingType.ThreeDimiensional);
        }

        //
        return receivedForce;
    }

    //
    public void ReceiveBlastDamage(Vector3 forceAndDirection)
    {
        //
        Vector3 impactDirection = transform.position - forceAndDirection;
        float impactAngle = Vector3.SignedAngle(Camera.main.transform.forward, impactDirection, transform.up);
        // Recordar que para fuerzas de empuje trabajamos en toneladas
        bodyRB.AddForce(forceAndDirection / 1000, ForceMode.Impulse);
        // Revisar como hacemos esto con el escudo
        SufferDamage(forceAndDirection.magnitude, impactAngle);
        
    }

    //
    void SufferDamage(float impactDamage, float impactAngle)
    {
        // Primero chequeamos si hay alguna acción defensiva activa
        if (robotControl.CurrentActionCharging == ActionCharguing.Defense)
        {
            switch (robotControl.ActiveDefenseMode)
            {
                case DefenseMode.Spheric:
                    float extraShieldEnergy = robotControl.ChargedAmount * gameManager.playerAttributes.forcePerSecond.CurrentValue;
                    float stoppedDamage = Mathf.Min(impactDamage, extraShieldEnergy);
                    float impactDamageBeforeReduction = impactDamage;
                    impactDamage -= stoppedDamage;
                    // Aplicamos un mini margen para que no se detenga la acción
                    robotControl.ChargedAmount -= (stoppedDamage / gameManager.playerAttributes.forcePerSecond.CurrentValue) - 0.01f;
                    Debug.Log("SD - Impact damage before reduction: " + impactDamageBeforeReduction + ", stopped damage: " + stoppedDamage +
                        ", entering damage: " + impactDamage);
                    break;

                case DefenseMode.Front:  break;
            }
        }
        //float damageToShields = Mathf.Max(impactDamage - shieldAbsortion, 0);
        currentShield -= impactDamage;
        DamageType damageType;
        if (currentShield < 0)
        {
            // Recuerda que el escudo perdido sobrante llega como negativo
            currentHealth += currentShield;
            currentShield = 0;
            shieldsDepleted = true;
            damageType = DamageType.Hull;
            GeneralFunctions.PlaySoundEffect(audioSource, shieldDepletionClip);
        }
        else
        {
            damageType = DamageType.Shield;
        }
        //
        hud.AddDamageIndicator(impactAngle, damageType);

        //
        if (currentHealth <= 0)
        {
            // Muerte
            Debug.Log("YOU DIED, BITCH");
            ManageDeath();
        }
    }

    // Daño de entorno, que se salta ciertas cosas
    public void ReceiveEnvionmentalDamage(float damageAmount)
    {
        //
        currentShield -= damageAmount;
        if (currentShield < 0)
        {
            // Recuerda que el escudo perdido sobrante llega como negativo
            //float damageToHealth = Mathf.Min(currentShield + armor, 0);
            currentHealth += currentShield;
            currentShield = 0;
            shieldsDepleted = true;
            GeneralFunctions.PlaySoundEffect(audioSource, shieldDepletionClip);
        }

        //
        if (currentHealth <= 0)
        {
            // Muerte
            Debug.Log("YOU DIED, BITCH");
            ManageDeath();
        }
    }

    // Automuerte por ciertos eventos
    public void Die()
    {
        //
        currentHealth = 0;
        // Muerte
        Debug.Log("YOU DIED, BITCH");
        ManageDeath();
    }

    void ManageDeath()
    {
        Repulsor repulsor = GetComponent<Repulsor>();
        Destroy(repulsor);

        //RobotControl robotControl = GetComponent<RobotControl>();
        Destroy(robotControl);

        ShovelManager shovelManager = GetComponent<ShovelManager>();
        Destroy(shovelManager);

        //Rigidbody rb = GetComponent.RigidBody

        MeshRenderer faceRenderer = playerFace.GetComponent<MeshRenderer>();
        faceRenderer.material = playerDeadFace;
        faceRenderer.materials[0] = playerDeadFace;
    }

    #endregion
}
