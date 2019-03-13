﻿using System.Collections;
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

    private RobotControl robotControl;
    private float currentHealth;
    private float currentShield;
    private ImpactInfoManager impactInfoManager;
    private GameManager gameManager;
    private Rigidbody bodyRB;
    private ProvisionalHUD hud;

    //
    Vector3 previousStepRbVelocity;

    #region Properties

    public float CurrentHealth { get { return currentHealth; } }
    public float CurrentShield { get { return currentShield; } }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        robotControl = GetComponent<RobotControl>();
        currentHealth = maxHealth;
        currentShield = maxShield;
        impactInfoManager = FindObjectOfType<ImpactInfoManager>();
        gameManager = FindObjectOfType<GameManager>();
        bodyRB = GetComponent<Rigidbody>();
        hud = FindObjectOfType<ProvisionalHUD>();
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        // De momento hacemos que se recargen con el tiempo
        currentShield += dt * shieldRegenerationRate;
        currentShield = Mathf.Clamp(currentShield, 0, maxShield);
        currentHealth += dt * healthRegenerationRate;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        //
        previousStepRbVelocity = bodyRB.velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // TODO: Manejar cada masa por separado a la hora de echar las cuentas
        // No es lo mismo golpear una masa 1000 estatica que ser atropellado por ella
        // Así que empieza a mirar formulas de como se aplica

        // Nota extra importante
        // La fuerza del impacto depende de la aceleración, no de la velocidad
        // Animalico

        Vector3 playerDecceleration = bodyRB.velocity - previousStepRbVelocity;
        // De momento calculamos la fuerza a lo bruto
        // Sin tener en cuenta angulo de colisión
        float playerImpactForce = playerDecceleration.sqrMagnitude * bodyRB.mass;

        Vector3 relativeVelocity = collision.relativeVelocity;
        ContactPoint collisionPoint = collision.GetContact(0);
        Collider collider = collision.collider;
        GameObject gameObject = collider.gameObject;
        Rigidbody collidingRB = collision.rigidbody;
        ReceiveImpact(collisionPoint.point, gameObject, collidingRB);
    }

    #region Methods

    public void ReceiveImpact(Vector3 contactPoint, GameObject otherGameObject, Rigidbody collidingRB)
    {
        //
        float extraDefense = 0;
        //
        if (robotControl.CurrentActionCharging == ActionCharguing.Defense)
        {
            switch (robotControl.ActiveDefenseMode)
            {
                case DefenseMode.Spheric:
                    extraDefense = gameManager.sphericShieldStrength;
                    break;

                case DefenseMode.Front:
                    extraDefense = 9999;
                    break;
            }
        }
        
        //
        Rigidbody rb = collidingRB;
        
        //
        Bullet bulletComponent = otherGameObject.GetComponent<Bullet>();
        // Si el impacto no tiene rigidbody (escenario) usamos el nuestro para el chequeo
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        Vector3 impactSpeed = rb.velocity;

        //
        Vector3 impactDirection = contactPoint - transform.position;
        impactDirection = rb.velocity.normalized;
        // impactDirection.y = 0;
        float impactAngle = Vector3.SignedAngle(Camera.main.transform.forward, impactDirection, transform.up);

        //
        float impactForce = impactSpeed.magnitude * rb.mass;
        
        // Si no es bala adaptamos el peso a las toneladas
        if(bulletComponent != null)
            impactForce *= 1000;

        //
        float impactDamage = Mathf.Max(impactForce - extraDefense, 0);
        //
        //Debug.Log("Player impacted by " + gameObject.name + " with a speed of " + relativeVelocity.magnitude + 
        //    "and a mass of " + rb.mass + ". " + impactForce + "J impactForce.");
        //
        impactInfoManager.SendImpactInfo(transform.position, impactForce);
        //
        SufferDamage(impactDamage, impactAngle);
    }

    void SufferDamage(float impactDamage, float impactAngle)
    {
        //
        float damageToShields = Mathf.Max(impactDamage - shieldAbsortion, 0);
        currentShield -= impactDamage;
        DamageType damageType;
        if (currentShield < 0)
        {
            // Recuerda que el escudo perdido sobrante llega como negativo
            float damageToHealth = Mathf.Min(currentShield + armor, 0);
            currentHealth += currentShield;
            currentShield = 0;
            damageType = DamageType.Hull;
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
