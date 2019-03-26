using System.Collections;
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

    public GameObject face;
    public Material deadFaceMaterial;
    public GameObject explosionPrefab;
    public GameObject smokePrefab;

    #endregion

    #region Private Attributes

    private ImpactInfoManager impactInfoManager;
    // private float currentChasisHealth;
    private float currentHealth;
    private ProvLevelManager levelManager;
    private Rigidbody rb;
    private Vector3 previousVelocity;

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
    void Start () {
        impactInfoManager = FindObjectOfType<ImpactInfoManager>();
        //currentChasisHealth = maxChasisHealth;
        currentHealth = maxHealth;
        //
        levelManager = FindObjectOfType<ProvLevelManager>();
        //if(levelManager)

        // De momento para klos voladores mas que nada
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		// Cheqeo extra de salida de escenario
        if(transform.position.y < -10)
        {
            //ManageDamage(currentChasisHealth, transform.position);
            Destroy(gameObject);
        }
	}

    //
    void FixedUpdate()
    {
        // Guardamos la previa para chequear si ha habido un ostión
        previousVelocity = rb.velocity;
    }

    // Lo ponemos para ver que falla con las colisiones entre cuerpos
    // Ojo que alguna la pillará por duplicado
    private void OnCollisionEnter(Collision collision)
    {
        // Estas no las queremos chequear aqui
        Bullet bullet = collision.collider.GetComponent<Bullet>();
        // Chequeamos diferencia de velocidades para ver si solo es fricción u hostiazo
        Vector3 velocityOffset = previousVelocity - rb.velocity;
        // De momento diferencia de 1
        if(bullet == null)
        {
            //
            Rigidbody otherRb = collision.collider.GetComponent<Rigidbody>();
            float impactForce = GeneralFunctions.GetCollisionForce(rb, otherRb);
            if(otherRb != null || velocityOffset.magnitude > 2)
            {
                //Debug.Log("Hitting " + collision.transform.name + " with " + impactForce + " force");
                ReceiveImpact(impactForce, collision.contacts[0].point);
            }            
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
            //
            ReceiveInternalImpact(penetrationResult, collision.GetContact(0).point);
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
    public void ReceiveInternalImpact(float penetrationResult, Vector3 point)
    {
        //
        float damageReceived = penetrationResult;
        //float damageReceived = GeneralFunctions.Navy1940PenetrationCalc();
        //damageReceived = Mathf.Max(damageReceived, 0);
        //
        if(damageReceived > 0)
        {
            //Debug.Log("Received bullet impact with " + impactForce + " force against " + sideArmor + " armor. "
            //+ damageReceived + " damage received");
            //
            currentHealth -= damageReceived;
            ManageDamage(penetrationResult, point);

            impactInfoManager.SendImpactInfo(point, damageReceived);
        }
        else
        {
            //impactInfoManager.SendImpactInfo(point, impactForce, "No damage");
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    void ManageDamage(float damageReceived, Vector3 point)
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

            // Cambio de cara
            face.GetComponent<MeshRenderer>().material = deadFaceMaterial;

            //
            DeactivateStuff();
            
            //
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 10);
            //
            //GameObject smoke = 
                Instantiate(smokePrefab, transform);
            //
            if(levelManager != null)
                levelManager.AnnotateKill();
            // Esto para los voladores mas que nada
            rb.constraints = RigidbodyConstraints.None;
            //
            //Destroy(rb, 10);
            //
            Destroy(this);
        }
        else
        {
            //impactInfoManager.SendImpactInfo(point, damageReceived);
        }
    }

    void DeactivateStuff()
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
            
        // Cuerpo
        EnemyBodyBehaviour enemyBodyBehaviour = GetComponent<EnemyBodyBehaviour>();
        if (enemyBodyBehaviour != null)
        {
            for(int i = 0; i < enemyBodyBehaviour.weapons.Length; i++)
            {
                EnemyWeapon nextWeapon = enemyBodyBehaviour.weapons[i].GetComponent<EnemyWeapon>();
                if (nextWeapon)
                {
                    Destroy(nextWeapon);
                }
            }
            //
            Destroy(enemyBodyBehaviour);
        }

        // Propulsor
        EnemyPropulsion enemyPropulsion = GetComponent<EnemyPropulsion>();
        if(enemyPropulsion != null)
        {
            Destroy(enemyPropulsion);
        }
    }
    
}
