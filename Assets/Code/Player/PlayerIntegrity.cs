using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIntegrity : MonoBehaviour
{
    //
    public float maxShield;
    public float maxHealth;
    //
    [Tooltip("Minimal force of the impact to cause damage in shields")]
    public float shieldAbsortion = 100;
    [Tooltip("Minimal force of the impact to cause damage in health")]
    public float armor = 1000;
    //
    public float shieldRegenerationRate = 100;
    public float healthRegenerationRate = 10;
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
    private float extraDefense = 0;

    #region Properties

    public float CurrentHealth { get { return currentHealth; } }
    public float CurrentShield {
        get { return currentShield; }
        set {
            currentShield = value;
            currentShield = Mathf.Min(currentShield, maxShield);
        }
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        robotControl = GetComponent<RobotControl>();
        currentHealth = maxHealth;
        currentShield = maxShield;
        //impactInfoManager = FindObjectOfType<ImpactInfoManager>();
        gameManager = FindObjectOfType<GameManager>();
        bodyRB = GetComponent<Rigidbody>();
        hud = FindObjectOfType<ProvisionalHUD>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        // De momento hacemos que se recargen con el tiempo
        if (!shieldsDepleted)
        {
            currentShield += dt * shieldRegenerationRate;
            currentShield = Mathf.Clamp(currentShield, 0, maxShield);
        }
        //
        if (shieldsDepleted && robotControl.IsResting)
            shieldsDepleted = false;
        //
        if (robotControl.IsResting)
        {
            currentHealth += dt * healthRegenerationRate;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
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

    //public void ReceiveProyectileImpact(Bullet proyectileData, Rigidbody proyectileRb, Vector3 impactPoint)
    //{
    //    //
    //    float diameter = proyectileData.diameter;

    //    float penetrationValue = GeneralFunctions.Navy1940PenetrationCalc(proyectileRb.mass, diameter, proyectileRb.velocity.magnitude);
    //    //Debug.Log("Penetration value: " + penetrationValue + ", mass: " + bulletRb.mass + 
    //    //    ", diameter: " + diameter + ", velocity: " + bulletRb.velocity.magnitude);
    //    float penetrationResult = Mathf.Max(penetrationValue - armor, 0);
    //    //
    //    Vector3 impactDirection = impactPoint - transform.position;
    //    // Cogemos el angulo para indicar en el HUD
    //    float impactAngle = Vector3.SignedAngle(Camera.main.transform.forward, impactDirection, transform.up);
    //    //
    //    SufferDamage(impactDamage, impactAngle);

    //}

    // TODO: Hay que coger la normal también
    public void ReceiveImpact(Vector3 contactPoint, GameObject otherGameObject, Rigidbody collidingRB, Vector3 impactNormal)
    {
        // Defensa extra por acciones defensivas
        float extraDefense = 0;
        //
        if (robotControl.CurrentActionCharging == ActionCharguing.Defense)
        {
            switch (robotControl.ActiveDefenseMode)
            {
                case DefenseMode.Spheric: extraDefense = gameManager.sphericShieldStrength; break;

                case DefenseMode.Front: extraDefense = 9999; break;
            }
        }
        
        // COgemos los dos rigidbodies
        Rigidbody otherRb = collidingRB;

        //
        Bullet bulletComponent = otherGameObject.GetComponent<Bullet>();

        // Esto tiene pinta de petar en el start
        // Lo montaremos bien para que no pase
        float totalImpactForce = 0;
        // TODO: Habrá que ver como manejar esto
        if (bulletComponent != null)
        {
            // Vamos a probar con la energía cinética
            totalImpactForce = GeneralFunctions.GetBodyKineticEnergy(otherRb);
            //
            bodyRB.AddForce(collidingRB.velocity * collidingRB.mass, ForceMode.Impulse);
        }
        //
        else if (bodyRB != null)
        {
            totalImpactForce = GeneralFunctions.GetCollisionForce(bodyRB, otherRb);
        }

        //
        Vector3 impactDirection = contactPoint - transform.position;

        // Cogemos el angulo para indicar en el HUD
        float impactAngle = Vector3.SignedAngle(Camera.main.transform.forward, impactDirection, transform.up);

        //
        float impactDamage = Mathf.Max(totalImpactForce - extraDefense, 0);
        
        // De momento no visualizamos info del daño que recibimos
        SufferDamage(impactDamage, impactAngle);
    }

    // TODO: Hcaer que el escudo cinético sea verdadermente cinético
    // Manejarlo aquí cuando usemos el escudo
    void ApplyKineticShield()
    {

    }

    //
    public void ReceiveBlastDamage(Vector3 forceAndDirection)
    {
        //
        Vector3 impactDirection = transform.position - forceAndDirection;
        float impactAngle = Vector3.SignedAngle(Camera.main.transform.forward, impactDirection, transform.up);
        //
        bodyRB.AddForce(forceAndDirection, ForceMode.Impulse);
        //
        SufferDamage(forceAndDirection.magnitude, impactAngle);
        
    }

    //
    void SufferDamage(float impactDamage, float impactAngle)
    {
        //
        //float damageToShields = Mathf.Max(impactDamage - shieldAbsortion, 0);
        currentShield -= impactDamage;
        DamageType damageType;
        if (currentShield < 0)
        {
            // Recuerda que el escudo perdido sobrante llega como negativo
            //float damageToHealth = Mathf.Min(currentShield + armor, 0);
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
