﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyConsistency : MonoBehaviour {

    #region Public Attributes

    // public float maxChasisHealth = 100.0f; // 
    public float maxHealth = 100.0f; //
    [Tooltip("Defense against non bullet impacts.")]
    public float defense = 10;   // The minimal physic strength to start receiving an effect
    [Tooltip("Adjustment for models which central point is deviated")]
    public Vector3 centralPointOffset = new Vector3(0,1,0);

    #endregion

    #region Private Attributes

    protected ImpactInfoManager impactInfoManager;
    // private float currentChasisHealth;
    protected float currentHealth;
    protected ProvLevelManager levelManager;
    protected EnemyManager enemyManager;
    protected Rigidbody rb;
    protected Vector3 previousVelocity;
    protected AudioSource audioSource;

    #endregion

    #region Properties

    //public float CurrentChasisHealth { get { return currentChasisHealth;  } }
    public float CurrentHealth { get { return currentHealth; } }

    public float Defense { get { return defense; } }
    public bool IsAlive
    {
        get { return /*currentChasisHealth > 0 &&*/ CurrentHealth > 0; }
    }

    #endregion


    // Use this for initialization
    protected virtual void Start () {
        impactInfoManager = FindObjectOfType<ImpactInfoManager>();
        //currentChasisHealth = maxChasisHealth;
        currentHealth = maxHealth;
        //
        levelManager = FindObjectOfType<ProvLevelManager>();
        enemyManager = FindObjectOfType<EnemyManager>();
        //if(levelManager)

        // De momento para klos voladores mas que nada
        rb = GetComponent<Rigidbody>();
        //
        audioSource = GetComponent<AudioSource>();
	}

    //
    protected virtual void FixedUpdate()
    {
        // Guardamos la previa para chequear si ha habido un ostión
        previousVelocity = rb.velocity;
    }

    // Update is called once per frame
    protected virtual void Update () {
		// Cheqeo extra de salida de escenario
        if(transform.position.y < -10)
        {
            //ManageDamage(currentChasisHealth, transform.position);
            Destroy(gameObject);
            //
            if (enemyManager != null)
                enemyManager.SubtractOne(gameObject);
        }
        // Decidimos el daño físico por cambio en la velocidad
        if ((rb.velocity.magnitude - previousVelocity.magnitude) > 3)
        {
            float impactForce = GeneralFunctions.GetCollisionForce(rb, null);
            ReceiveImpact(impactForce, transform.position);
        }
    }



    // Lo ponemos para ver que falla con las colisiones entre cuerpos
    // Ojo que alguna la pillará por duplicado
    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Estas no las queremos chequear aqui
        Bullet bullet = collision.collider.GetComponent<Bullet>();
        // Chequeamos diferencia de velocidades para ver si solo es fricción u hostiazo
        //Vector3 velocityOffset = previousVelocity - rb.velocity;
        // De momento diferencia de 1
        if(bullet == null)
        {
            //
            //Rigidbody otherRb = collision.collider.GetComponent<Rigidbody>();
            //float impactForce = GeneralFunctions.GetCollisionForce(rb, otherRb);
            //if(otherRb != null || velocityOffset.magnitude > 2)
            //{
            //    //Debug.Log("Hitting " + collision.transform.name + " with " + impactForce + " force");
            //    //ReceiveImpact(impactForce, collision.contacts[0].point);
            //}            
        }
        // TODO: Está duplicado aqui y en EnemyCollider
        // Ver como va
        else
        {
            //Debug.Log("Procesado en EnemyConsistency");
            EnemyCollider bodyPart = collision.GetContact(0).thisCollider.GetComponent<EnemyCollider>();
            //Debug.Log("Collider: " + collision.collider);

            float diameter = bullet.diameter;
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

            float penetrationValue = GeneralFunctions.Navy1940PenetrationCalc(bulletRb.mass, diameter, bulletRb.velocity.magnitude);
            //Debug.Log("Penetration value: " + penetrationValue + ", mass: " + bulletRb.mass + 
            //    ", diameter: " + diameter + ", velocity: " + bulletRb.velocity.magnitude);
            float penetrationResult = Mathf.Max(penetrationValue - bodyPart.armor, 0);
            //Debug.Log(penetrationResult + ", " + penetrationValue + ", " + bodyPart.armor);
            // Pasamos en qué proporción ha penetrado
            if (penetrationResult > 0)
                penetrationResult = 1 - (bodyPart.armor / penetrationValue);
            //Debug.Log("Pen proportion: " + penetrationResult);
            //
            ReceiveProyectileImpact(penetrationResult, collision.GetContact(0).point, bulletRb);
        }
        
    }

    /// <summary>
    /// df
    /// TODO: Revisar manejo de 
    /// </summary>
    /// <param name="impactForce"></param>
    /// <param name="point"></param>
    public void ReceiveImpact(float impactForce, Vector3 point)
    {
        //
        if (!IsAlive)
            return;
        //
        float damageReceived = impactForce - defense;
        damageReceived = Mathf.Max(damageReceived, 0);
        //

        //Debug.Log(damageReceived + " damage received");
        //if(damageReceived > 0)
        //Debug.Log(gameObject.name + " received body impact with " + impactForce + " force. " + damageReceived + " damage received");
        //
        currentHealth -= damageReceived;
        //currentChasisHealth -= damageReceived;
        ManageDamage(impactForce, point);
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="impactForce"></param>
    /// <param name="point"></param>
    public void ReceiveProyectileImpact(float penetrationResult, Vector3 point, Rigidbody proyectileRb)
    {
        //
        float damageReceived = 0;
        //float damageReceived = GeneralFunctions.Navy1940PenetrationCalc();
        //damageReceived = Mathf.Max(damageReceived, 0);
        //
        if(penetrationResult > 0)
        {
            //Debug.Log("Received bullet impact with " + impactForce + " force against " + sideArmor + " armor. "
            //+ damageReceived + " damage received");
            //
            float kineticEnergy = GeneralFunctions.GetBodyKineticEnergy(proyectileRb);
            damageReceived = kineticEnergy * penetrationResult;
            //Debug.Log("Penetration result: " + penetrationResult + ", kinetic energy: " + kineticEnergy + ", damage received: " + damageReceived);
            currentHealth -= damageReceived;
            ManageDamage(damageReceived, point);

            //impactInfoManager.SendImpactInfo(point, damageReceived);
        }
        else
        {
            //penetrationResult = 0;
            impactInfoManager.SendImpactInfo(point, damageReceived, "No damage");
        }
        
    }

    //
    public void ReceiveSharpnelImpact(float penetrationResult, Vector3 point, FakeRB proyectileRb)
    {
        // TODO: Revisar esto
        float damageReceived = 0;
        if (penetrationResult > 0)
        {
            damageReceived = GeneralFunctions.GetFakeBodyKineticEnergy(proyectileRb) * penetrationResult;
            currentHealth -= damageReceived;
            ManageDamage(penetrationResult, point);

            impactInfoManager.SendImpactInfo(point, damageReceived);
        }
        else
        {
            penetrationResult = 0;
            impactInfoManager.SendImpactInfo(point, penetrationResult, "No damage");
        }

    }

    // Funcion provisional para trabajar impactos de metralla
    public void ReceiveSharpnelImpact(float penetrationResult, Vector3 point, float sharpnelMass)
    {
        if(penetrationResult > 0)
        {
            //FUCK
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected virtual void ManageDamage(float damageReceived, Vector3 point)
    {
        // Si la vida cae a cero lo convertimos en un simple objeto con rigidbody
        // Le quitamos sus scripts de comportamiento, vamos
        if (/*currentChasisHealth <= 0 || */currentHealth <= 0)
        {
            //
            if (impactInfoManager != null)
                impactInfoManager.SendImpactInfo(point, damageReceived, "Enemy destroyed");
            else
                Debug.Log("Impact info manager is null. Check it");

            //Debug.Log("Enemy " + transform.name + " destroyed. Impact force " + impactForce);
            //gameObject.SetActive(false);

            

            //
            DeactivateStuff();
            
            
            if(levelManager != null)
                levelManager.AnnotateKill();
            if (enemyManager != null)
                enemyManager.SubtractOne(gameObject);
            // Esto para los voladores mas que nada
            rb.constraints = RigidbodyConstraints.None;
            
            // TODO: Mirar como hacer para quitar el rigidody a los x segundos
            //Destroy(rb, 10);
            // Destruimos el script pero dejamos el cuerpo
            Destroy(this);
        }
        else
        {
            // Chequo provisional para que no de mal en el menu
            if(impactInfoManager != null)
                impactInfoManager.SendImpactInfo(point, damageReceived);
        }
    }

    protected virtual void DeactivateStuff()
    {
        //Chequamos y quitamos
        // Torretas
        EnemyTurret enemyTurret = GetComponent<EnemyTurret>();
        if (enemyTurret != null)
        {
            for (int i = 0; i < enemyTurret.weapons.Length; i++)
            {
                EnemyWeapon nextWeapon = enemyTurret.weapons[i].GetComponent<EnemyWeapon>();
                if (nextWeapon)
                {
                    Destroy(nextWeapon);
                }
            }
            //
            Destroy(enemyTurret);
        }
            
        // Cuerpo (mekanoide)
        MekanoidBodyBehaviour mekanoidBodyBehaviour = GetComponent<MekanoidBodyBehaviour>();
        if (mekanoidBodyBehaviour != null)
        {
            for(int i = 0; i < mekanoidBodyBehaviour.weapons.Length; i++)
            {
                EnemyWeapon nextWeapon = mekanoidBodyBehaviour.weapons[i].GetComponent<EnemyWeapon>();
                if (nextWeapon)
                {
                    Destroy(nextWeapon);
                }
            }
            //
            Destroy(mekanoidBodyBehaviour);
        }

        // Cuerpo (bicho)
        BugBodyBehaviour bugBodyBehaviour = GetComponent<BugBodyBehaviour>();
        if (bugBodyBehaviour != null)
        {
            // TODO: Algo esta fallando aqui
            Debug.Log("Destroying " + transform.name + " body behaviour");
            for (int i = 0; i < bugBodyBehaviour.weapons.Length; i++)
            {
                EnemyWeapon nextWeapon = bugBodyBehaviour.weapons[i].GetComponent<EnemyWeapon>();
                Debug.Log("Destroying " + transform.name + " weapon " + nextWeapon.transform.name);
                if (nextWeapon)
                {
                    Destroy(nextWeapon);
                }
            }
            //
            Destroy(bugBodyBehaviour);
        }

        // Propulsor
        EnemyPropulsion enemyPropulsion = GetComponent<EnemyPropulsion>();
        if(enemyPropulsion != null)
        {
            Destroy(enemyPropulsion);
        }
    }
    
}
