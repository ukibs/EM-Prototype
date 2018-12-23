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
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 relativeVelocity = collision.relativeVelocity;
        Collider collider = collision.collider;
        GameObject gameObject = collider.gameObject;
        Rigidbody collidingRB = collision.rigidbody;
        ReceiveImpact(relativeVelocity, gameObject, collidingRB);
    }

    #region Methods

    public void ReceiveImpact(Vector3 relativeVelocity, GameObject otherGameObject, Rigidbody collidingRB)
    {
        //
        int extraDefense = 0;
        //
        if (robotControl.CurrentActionCharging == ActionCharguing.Defense)
        {
            switch (robotControl.DefenseMode)
            {
                case DefenseMode.Spheric:
                    extraDefense = 1000;
                    break;

                case DefenseMode.Front:

                    break;
            }
        }
        //
        Vector3 impactSpeed = relativeVelocity;
        Rigidbody rb = collidingRB;
        Bullet bulletComponent = otherGameObject.GetComponent<Bullet>();
        // Si el impacto no tiene rigidbody (escenario) usamos el nuestro para el chequeo
        if (rb == null) rb = GetComponent<Rigidbody>();
        //
        float impactForce = impactSpeed.magnitude * rb.mass;
        // Conversión a julios
        // TODO: Diferenciar cuando sean balas
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
        SufferDamage(impactDamage);
    }

    void SufferDamage(float impactDamage)
    {
        //
        float damageToShields = Mathf.Max(impactDamage - shieldAbsortion, 0);
        currentShield -= impactDamage;
        if (currentShield < 0)
        {
            // Recuerda que el escudo perdido sobrante llega como negativo
            float damageToHealth = Mathf.Min(currentShield + armor, 0);
            currentHealth += currentShield;
            currentShield = 0;
        }            

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
