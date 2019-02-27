using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyConsistency : MonoBehaviour {

    #region Public Attributes

    public float maxChasisHealth = 100.0f; // 
    public float maxCoreHealth = 100.0f; //
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
    private float currentChasisHealth;
    private float currentCoreHealth;
    private ProvLevelManager levelManager;

    #endregion

    #region Properties

    public float CurrentChasisHealth { get { return currentChasisHealth;  } }
    public float CurrentCoreHealth { get { return currentCoreHealth; } }

    public float Defense { get { return defense; } }
    public bool IsAlive
    {
        get { return currentChasisHealth > 0 && CurrentCoreHealth > 0; }
    }

    #endregion


    // Use this for initialization
    void Start () {
        impactInfoManager = FindObjectOfType<ImpactInfoManager>();
        currentChasisHealth = maxChasisHealth;
        currentCoreHealth = maxCoreHealth;
        //
        levelManager = FindObjectOfType<ProvLevelManager>();
        //if(levelManager)

        
	}
	
	// Update is called once per frame
	void Update () {
		// Cheqeo extra de salida de escenario
        if(transform.position.y < -10)
        {
            ManageDamage(currentChasisHealth, transform.position);
        }
	}

    void OnCollisionEnter(Collision collision)
    {
        //
        if (!IsAlive)
            return;
        // Trataremos de forma diferente los impactos de las balas y el resto
        Bullet bullet = collision.collider.GetComponent<Bullet>();

        //
        string bulletConfimation = (bullet != null) ? "Yes" : "No";
        //Debug.Log(collision.collider.gameObject.name + ", has bullet component: " + bulletConfimation);

        // Si lo que nos ha golpeado no tiene rigidbody 
        // hemos chocado con el escenario
        // así que usamos el nuestro
        Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        //
        float impactForce = collision.relativeVelocity.magnitude * rb.mass;

        if (bullet == null)
            ReceiveImpact(impactForce, collision.contacts[0].point);
        // 
        else if (bullet != null)
        {
            // TODO: Hacer bien el checkeo para las balas que colisionan
            // en vez de activar la función

            //CheckImpactedPart(collision.collider);
            //collision.collider
            Debug.Log("Impacto de bala donde no debería");
            //ReceiveInternalImpact(impactForce, collision.contacts[0].point);
        }

        //else if(collision.contacts[0].point != null)
        //    impactInfoManager.SendImpactInfo(collision.contacts[0].point, impactForce, "No damage");

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
        currentChasisHealth -= damageReceived;
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
            currentCoreHealth -= damageReceived;
            ManageDamage(penetrationResult, point);

            //impactInfoManager.SendImpactInfo(point, impactForce);
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
        if (currentChasisHealth <= 0 || currentCoreHealth <= 0)
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
            GameObject smoke = Instantiate(smokePrefab, transform);
            //
            if(levelManager != null)
                levelManager.AnnotateKill();
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
        EnemyGroundBody enemyGroundVehicle = GetComponent<EnemyGroundBody>();
        if (enemyGroundVehicle != null)
        {
            for(int i = 0; i < enemyGroundVehicle.weapons.Length; i++)
            {
                EnemyWeapon nextWeapon = enemyGroundVehicle.weapons[i].GetComponent<EnemyWeapon>();
                if (nextWeapon)
                {
                    Destroy(nextWeapon);
                }
            }
            //
            Destroy(enemyGroundVehicle);
        }

        // Propulsor
        EnemyPropulsion enemyPropulsion = GetComponent<EnemyPropulsion>();
        if(enemyPropulsion != null)
        {
            Destroy(enemyPropulsion);
        }
    }
}
