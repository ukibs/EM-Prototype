using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyConsistency : Targeteable {

    #region Public Attributes

    // public float maxChasisHealth = 100.0f; // 
    public int maxHealth = 100; //
    [Tooltip("Defense against non bullet impacts.")]
    public float defense = 10;   // The minimal physic strength to start receiving an effect
    //
    public GameObject deathBloodPrefab;
    public GameObject deathParticlesPrefab;

    //[Tooltip("Adjustment for models which central point is deviated")]
    //public Vector3 centralPointOffset = new Vector3(0,1,0);

    // De momento aqui
    public AudioClip deathClip;
    public GameObject deadBodyPrefab;

    #endregion

    #region Private Attributes

    protected ImpactInfoManager impactInfoManager;
    // private float currentChasisHealth;
    protected int currentHealth;
    protected ProvLevelManager levelManager;
    protected EnemyManager enemyManager;
    protected Rigidbody rb;
    protected Vector3 previousVelocity;
    protected AudioSource audioSource;
    //
    protected int managerIndex;
    // Vamos a usar esta variable para controlar pérdida de equilibrio en comportamientos entre otras cosas
    protected bool receivedStrongImpact = false;

    // TODO: Gestionarlo también en weakpoint. Cuando esté listo para manejar temas de penetración de blindaje
    protected EnemyCollider[] bodyColliders;

    #endregion

    #region Properties

    //public float CurrentChasisHealth { get { return currentChasisHealth;  } }
    public float CurrentHealth { get { return currentHealth; } }

    public float Defense { get { return defense; } }
    public bool IsAlive
    {
        get { return /*currentChasisHealth > 0 &&*/ CurrentHealth > 0; }
    }

    //
    public int ManagerIndex {
        get { return managerIndex; }
        set { managerIndex = value; }
    }

    public bool ReceivedStrongImpact { get { return receivedStrongImpact; } }

    public EnemyCollider[] BodyColliders { get { return bodyColliders; } }

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
        //
        bodyColliders = GetComponentsInChildren<EnemyCollider>();
	}

    //
    protected virtual void FixedUpdate()
    {
        // Guardamos la previa para chequear si ha habido un ostión
        previousVelocity = rb.velocity;
        //
        receivedStrongImpact = false;
    }

    // Update is called once per frame
    protected virtual void Update () {
		// Cheqeo extra de salida de escenario
        if(transform.position.y < -10)
        {
            //ManageDamage(currentChasisHealth, transform.position);
            //Destroy(gameObject);
            //EnemyManager
            //
            if (enemyManager != null)
            {
                //enemyManager.SubtractOne(managerIndex);
                // En este caso no sacamos cuerpo muerto
                EnemyAnalyzer.Release();
                enemyManager.SendToReserve(managerIndex, gameObject);
            }
        }
        // Decidimos el daño físico por cambio en la velocidad
        if (CheckDrasticChangeInAcceleration(2.5f))
        {
            float impactForce = GeneralFunctions.GetCollisionForce(rb, null);
            ReceiveImpact(impactForce, transform.position);
            //
            receivedStrongImpact = true;
        }
    }



    // Lo ponemos para ver que falla con las colisiones entre cuerpos
    // Ojo que alguna la pillará por duplicado
    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Estas no las queremos chequear aqui
        Bullet bullet = collision.collider.GetComponent<Bullet>();
        // Chequeamos diferencia de velocidades para ver si solo es fricción u hostiazo
        Vector3 velocityOffset = previousVelocity - rb.velocity;
        // De momento diferencia de 1
        if(bullet == null && collision.collider.tag != "Sand")
        {
            //
            Rigidbody otherRb = collision.collider.GetComponent<Rigidbody>();
            float impactForce = GeneralFunctions.GetCollisionForce(rb, otherRb);
            if (otherRb != null || CheckDrasticChangeInAcceleration(1))
            {
                //Debug.Log("Hitting " + collision.transform.name + " with " + impactForce + " force");
                ReceiveImpact(impactForce, collision.contacts[0].point);
                //
                receivedStrongImpact = true;
            }
        }
        // TODO: Está duplicado aqui y en EnemyCollider
        // Ver como va
        else if(bullet != null)
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

    // Aquí daremos el cambiazo con el cuerpo muerto y le pondermos los decorados
    void PutDeadBody()
    {
        // Instanciamos cuerpo muerto
        GameObject deadBody = Instantiate(deadBodyPrefab, transform.position, transform.rotation);
        // Le asignamos nuestra current rb.velocity
        // Varios, ya que siendo ragdolls tendrán varios
        Rigidbody[] rbs = deadBody.GetComponentsInChildren<Rigidbody>();
        for(int i = 0; i < rbs.Length; i++)
        {
            rbs[i].velocity = rb.velocity;
        }
        // Y le pegamos nuestras pegatinas de sangre
        BulletHole[] bulletHoles = GetComponentsInChildren<BulletHole>();
        for(int i = 0; i < bulletHoles.Length; i++)
        {
            bulletHoles[i].transform.parent = deadBody.transform.GetChild(0);
        }
    }

    //
    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    //
    public void SetCollidersPenetrationColors()
    {
        //
        for(int i = 0; i < bodyColliders.Length; i++)
        {
            bodyColliders[i].SetPenetrationColors();
        }
    }

    //
    public void SetOriginalPenetrationColors()
    {
        //
        for (int i = 0; i < bodyColliders.Length; i++)
        {
            bodyColliders[i].SetOriginalColors();
        }
    }

    //
    bool CheckDrasticChangeInAcceleration(float errorMargin = 2)
    {
        //
        float acceleration = (rb.velocity - previousVelocity).magnitude;
        //
        if (acceleration > errorMargin)
            return true;
        //
        return false;
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
        // De momento chequeamos aqui el fuera de cámara
        if (!IsOnCamera())
            return;
        //
        float damageReceived = impactForce - defense;
        damageReceived = Mathf.Max(damageReceived, 0);
        //

        //Debug.Log(damageReceived + " damage received");
        //if(damageReceived > 0)
        //Debug.Log(gameObject.name + " received body impact with " + impactForce + " force. " + damageReceived + " damage received");
        //
        currentHealth -= (int)damageReceived;
        //currentChasisHealth -= damageReceived;
        ManageDamage(damageReceived, point);
        
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
            currentHealth -= (int)damageReceived;
            ManageDamage(damageReceived, point);

            //impactInfoManager.SendImpactInfo(point, damageReceived);
        }
        else
        {
            //penetrationResult = 0;
            impactInfoManager.SendImpactInfo(point, (int)damageReceived, "No damage");
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
            currentHealth -= (int)damageReceived;
            ManageDamage(penetrationResult, point);

            impactInfoManager.SendImpactInfo(point, (int)damageReceived);
        }
        else
        {
            penetrationResult = 0;
            impactInfoManager.SendImpactInfo(point, (int)penetrationResult, "No damage");
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
                impactInfoManager.SendImpactInfo(point, (int)damageReceived, "Enemy destroyed");
            else
                Debug.Log("Impact info manager is null. Check it");

            //Debug.Log("Enemy " + transform.name + " destroyed. Impact force " + impactForce);
            //gameObject.SetActive(false);

            //
            GeneralFunctions.PlaySoundEffect(audioSource, deathClip);

            //
            //DeactivateStuff();
            
            
            if(levelManager != null)
                levelManager.AnnotateKill();
            if (enemyManager != null)
            {
                //enemyManager.SubtractOne(managerIndex);
                
                enemyManager.SendToReserve(managerIndex, gameObject);
            }

            if (deadBodyPrefab != null)
                PutDeadBody();
            EnemyAnalyzer.Release();

            //
            //Debug.Log("Death log: " + gameObject.name + ", " + transform.position + ", " + damageReceived);
            // Esto para los voladores mas que nada
            //rb.constraints = RigidbodyConstraints.None;

            // 
            if (deathBloodPrefab != null)
                PlaceDeathBlood();
            //
            if(deathParticlesPrefab != null)
                Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);

            // TODO: Mirar como hacer para quitar el rigidody a los x segundos
            //Destroy(rb, 10);
            // Destruimos el script pero dejamos el cuerpo
            //Destroy(this);
        }
        else
        {
            // Chequo provisional para que no de mal en el menu
            if(impactInfoManager != null)
                impactInfoManager.SendImpactInfo(point, (int)damageReceived);
        }
    }

    //
    protected void PlaceDeathBlood()
    {
        //
        RaycastHit hitInfo;
        Vector3 rayOrigin = transform.position + (Vector3.up * 10);

        int layerMask = 1 << 9;
        layerMask = ~layerMask;
        //int layerMask = 9;

        //
        if (Physics.Raycast(rayOrigin, -Vector3.up, out hitInfo, 100, layerMask))
        {
            Vector3 spawnPoint = hitInfo.point + (Vector3.up * 0.05f);
            Instantiate(deathBloodPrefab, spawnPoint, Quaternion.identity);
        }
    }

    //
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

    //
    bool IsOnCamera()
    {
        // Variable hardcodeada para enemigos que quedan tapados por la niebla
        float maxDistance = 500;
        //
        Vector3 positionInScreen = Camera.main.WorldToViewportPoint(transform.position);
        //
        return positionInScreen.x >= 0 && positionInScreen.x <= 1 &&
                positionInScreen.y >= 0 && positionInScreen.y <= 1 &&
                positionInScreen.z > 0 && positionInScreen.z < maxDistance;
    }
    
}
