using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyConsistency : Targeteable {

    #region Public Attributes

    //
    [Tooltip("The name follows a label format: Size and cateogry")]
    public string inGameName = "Size Category";
    // public float maxChasisHealth = 100.0f; // 
    public int maxHealth = 100; //
    [Tooltip("Defense against non bullet impacts.")]
    public float defense = 10;   // The minimal physic strength to start receiving an effect

    // TODO: Gestionarlo también en weakpoint. Cuando esté listo para manejar temas de penetración de blindaje
    [Tooltip("Normalemente se asignan solos. Si falla, a mano")]
    public EnemyCollider[] bodyColliders;

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
    protected SpringCamera springCamera;
    protected EnemyBaseBodyBehaviour bodyBehaviour;
    //
    protected int managerIndex;
    // Vamos a usar esta variable para controlar pérdida de equilibrio en comportamientos entre otras cosas
    //protected bool receivedStrongImpact = false;

    
    //
    protected bool isMultipart = false;
    protected List<EnemyCollider> targetableColliders;

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

    //public bool ReceivedStrongImpact { get { return receivedStrongImpact; } }

    public EnemyCollider[] BodyColliders { get { return bodyColliders; } }

    public List<EnemyCollider> TargeteableColliders { get { return targetableColliders; } }

    public bool IsMultipart { get { return isMultipart; } }

    #endregion


    // Use this for initialization
    protected virtual void Start () {
        impactInfoManager = FindObjectOfType<ImpactInfoManager>();
        //currentChasisHealth = maxChasisHealth;
        currentHealth = maxHealth;
        //
        levelManager = FindObjectOfType<ProvLevelManager>();
        enemyManager = FindObjectOfType<EnemyManager>();
        springCamera = FindObjectOfType<SpringCamera>();
        //if(levelManager)
        //
        bodyBehaviour = GetComponent<EnemyBaseBodyBehaviour>();

        // De momento para klos voladores mas que nada
        rb = GetComponent<Rigidbody>();
        // Caso del gusano grande, debería coger el de la cabeza
        //if (rb == null)
        //    rb = GetComponentInChildren<Rigidbody>();
        //
        audioSource = GetComponent<AudioSource>();
        //
        if(bodyColliders.Length == 0)
            bodyColliders = GetComponentsInChildren<EnemyCollider>();
        
        // Chequeo extra para el gusano grande
        //if(bodyColliders == null || bodyColliders.Length == 0)
        //    bodyColliders = transform.parent.GetComponentsInChildren<EnemyCollider>();
        // Debug.Log(gameObject.name + ", " + bodyColliders + ", " + bodyColliders.Length);
        //
        DetermineIfMultipart();
	}

    //
    protected virtual void FixedUpdate()
    {
        // Guardamos la previa para chequear si ha habido un ostión
        previousVelocity = rb.velocity;
        //
        //receivedStrongImpact = false;
    }

    // Update is called once per frame
    protected virtual void Update () {
		// Cheqeo extra de salida de escenario
        // TODO: Reviasr algunas cosas que se ven afectadas opri esto
        if(transform.position.y < -100)
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
        //if (CheckDrasticChangeInAcceleration(2.5f))
        //{
        //    float impactForce = GeneralFunctions.GetCollisionForce(rb, null);
        //    ReceiveImpact(impactForce, transform.position);
        //    //
        //    receivedStrongImpact = true;
        //}
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
        if(bullet == null && collision.collider.tag != "Sand")
        {
            //
            Rigidbody otherRb = collision.collider.GetComponent<Rigidbody>();
            float impactForce = GeneralFunctions.GetCollisionForce(rb, otherRb);
            //impactForce = GeneralFunctions.GetBodyKineticEnergy(rb);
            if (otherRb != null || CheckDrasticChangeInAcceleration(1))
            {
                //Debug.Log("Hitting " + collision.transform.name + " with " + impactForce + " force");
                ReceiveImpact(impactForce, collision.contacts[0].point);
                //
                //receivedStrongImpact = true;
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

    #region Methods

    // Aquí daremos el cambiazo con el cuerpo muerto y le pondermos los decorados
    void PutDeadBody(Vector3 deathBlowForce = new Vector3())
    {
        // Instanciamos cuerpo muerto
        GameObject deadBody = Instantiate(deadBodyPrefab, transform.position, transform.rotation);
        // Le asignamos nuestra current rb.velocity
        // Varios, ya que siendo ragdolls tendrán varios
        Rigidbody[] rbs = deadBody.GetComponentsInChildren<Rigidbody>();
        for(int i = 0; i < rbs.Length; i++)
        {
            rbs[i].velocity = rb.velocity + deathBlowForce;
        }
        // Y le pegamos nuestras pegatinas de sangre
        BulletHole[] bulletHoles = GetComponentsInChildren<BulletHole>();
        for(int i = 0; i < bulletHoles.Length; i++)
        {
            bulletHoles[i].transform.parent = deadBody.transform.GetChild(0);
        }
    }

    //
    public void ResetStatus()
    {
        currentHealth = maxHealth;
        //
        if(bodyBehaviour != null)
        bodyBehaviour.ResetStatus();
        //
        DetermineIfMultipart();
        //
        if (targetableColliders == null) return;
        for (int i = 0; i < targetableColliders.Count; i++)
            targetableColliders[i].ResetStatus();
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

    // Para recibir daño del ataque de pulso
    // TODO: Revisar como manejarlo con las explosiones
    public void ReceivePulseDamage(Vector3 directionWithForce)
    {
        //
        //Debug.Log("Receiving pulse damage with " + directionWithForce + " force");
        //
        float impactForce = directionWithForce.magnitude;
        //
        float damageReceived = impactForce - defense;
        damageReceived = Mathf.Max(damageReceived, 0);
        //
        currentHealth -= (int)damageReceived;
        //
        ManageDamage(damageReceived, transform.position, directionWithForce);
        //
        //receivedStrongImpact = true;
        bodyBehaviour.LoseFoot();
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
            //
            if(impactInfoManager != null)
            impactInfoManager.SendImpactInfo(point, (int)damageReceived);
        }
        else
        {
            //penetrationResult = 0;
            // Para evitar errores probando en el menu
            if(impactInfoManager != null)
                impactInfoManager.SendImpactInfo(point, (int)damageReceived, "No damage");
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    protected virtual void ManageDamage(float damageReceived, Vector3 point, Vector3 receivedForce = new Vector3())
    {
        // Si la vida cae a cero lo convertimos en un simple objeto con rigidbody
        // Le quitamos sus scripts de comportamiento, vamos
        if (currentHealth <= 0)
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
            
            // Anotamos muerte (mandando al propio bicho como muestra)
            if(levelManager != null)
            {
                //
                Transform wholeObject = transform.parent;
                //
                if (wholeObject == null)
                    wholeObject = transform;
                //
                levelManager.AnnotateKill(inGameName);
            }
                
            if (enemyManager != null)
            {
                // Esto hay que abordarlo de varias maneras
                // Recordar que es el padre el que hay que enviar, en la mayoría de casos
                GameObject enemyCompleteObject;
                if(transform.parent != null) enemyCompleteObject = transform.parent.gameObject;
                else enemyCompleteObject = gameObject;                
                enemyManager.SendToReserve(managerIndex, enemyCompleteObject);
            }

            if (deadBodyPrefab != null)
                PutDeadBody(receivedForce);
            EnemyAnalyzer.Release();

            //
            //Debug.Log("Death log: " + gameObject.name + ", " + transform.position + ", " + damageReceived);
            // Esto para los voladores mas que nada
            //rb.constraints = RigidbodyConstraints.None;

            // 
            //if (deathBloodPrefab != null)
            //    PlaceDeathBlood();
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
            // Si sigue vivo le aplicamos la fuerza
            // TODO: Probar con explosive force
            //rb.AddExplosionForce(receivedForce, )
            rb.AddForce(receivedForce, ForceMode.Impulse);
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
    void DetermineIfMultipart()
    {
        //
        if (bodyColliders == null) return;
        //
        targetableColliders = new List<EnemyCollider>(5);
        //
        for(int i = 0; i < bodyColliders.Length; i++)
        {
            if (bodyColliders[i].isTargeteable)
            {
                isMultipart = true;
                targetableColliders.Add(bodyColliders[i]);
            }
        }
        //
        //if (isMultipart)
        //    Debug.Log("Targeteable parts: " + targetableColliders.Count);
    }

    //
    public void RemoveTargeteablePart(EnemyCollider damagedPart)
    {
        //
        targetableColliders.Remove(damagedPart);
        //
        springCamera.SwitchTarget(transform);
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

    #endregion

}
