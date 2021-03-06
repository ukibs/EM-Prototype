﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    //
    public bool playerBullet = false;
    // Seguramente metamos también la velocidad de salid
    [Tooltip("Diameter in mm")]
    public float diameter;
    public float length;
    [Tooltip("Short for quick bullets long for artillery and other slow ones")]
    public float lifeTime = 10;
    // De momento lo manejamos así
    public bool dangerousEnough = false;
    public bool drawTrayectory = false;
    //
    public GameObject impactParticlesPrefab;
    public GameObject bulletHolePrefab;
    //
    public GameObject impactOnBugParticlesPrefab;
    public GameObject bulletHoleBugPrefab;

    // De momento hacemos dos, para player y enemigo
    // TODO: hacer un atrapado más general
    public AudioClip impactOnPlayer;
    public AudioClip impactOnEnemy;

    // TODO: Hcaerlo bien
    public bool restarted = false;

    // TODO: Decidir si implementamos el funcionamiendo de misil aqui como opción (impulso constante en vez de inicial)

    protected Rigidbody rb;
    protected Vector3 previousPosition;
    // TODO: hacer un atrapado más general
    //protected AudioSource audioSource;
    // Provisional, para que se destruya en ek explosivo en vez de en este
    protected ExplosiveBullet explosiveBullet;
    //
    protected AudioObjectManager bulletSoundManager;
    //
    protected CarolBaseHelp carolHelp;
    protected GameObject detectionTrail;
    protected LineRenderer detectionTrailRenderer;
    protected BulletPool bulletPool;
    protected Missile missileComponent;
    // De momento hardcodeado
    protected float maxTimeBetweenRecalculation = 2;
    protected float currentTimeBetweenRecalculation = 0;
    //
    protected float currentLifeTime = 0;
    //
    protected TrailRenderer trailRenderer;

    public float CurrentLifeTime { set { currentLifeTime = value; } }
    public Rigidbody Rb { get { return rb; } }

	// Use this for initialization
	protected virtual void Start () {
        //Debug.Log("Starting bullet");
        rb = GetComponent<Rigidbody>();
        //Destroy(gameObject, lifeTime);
        //audioSource = GetComponent<AudioSource>();
        //
        explosiveBullet = GetComponent<ExplosiveBullet>();
        //
        bulletSoundManager = FindObjectOfType<AudioObjectManager>();
        //
        bulletPool = FindObjectOfType<BulletPool>();
        //
        missileComponent = GetComponent<Missile>();
        //
        trailRenderer = GetComponent<TrailRenderer>();
        // TODO: Esto ahora va en el POOL
        if (dangerousEnough)
        {
            // Instanciamos el trail renderer
            carolHelp = FindObjectOfType<CarolBaseHelp>();
            if (drawTrayectory)
            {
                detectionTrail = Instantiate(carolHelp.dangerousProyetilesTrailPrefab, transform.position, Quaternion.identity);
                detectionTrailRenderer = detectionTrail.GetComponent<LineRenderer>();
                //
                //AllocateTrailRenderer();
            }            
            
            //
            //carolHelp.TriggerGeneralAdvice("DangerIncoming");
            //
            //bulletPool.AddDangerousBulletToList(gameObject);
        }
    }

    protected void FixedUpdate()
    {
        previousPosition = transform.position;
    }

    // Update is called once per frame
    protected void Update () {
        //
        float dt = Time.deltaTime;
        UpdateLifeTime(dt);
        // Hacemos que vaya cambiando la orientación acorde a la trayectoria
        // Ahora que estamos haciendo probatinas con esfericas no se notará
        // TODO: Chequear cuando tengamos balas más definidas
        if(missileComponent == null)
            transform.LookAt(rb.velocity);
        //
        CheckTravelDone(dt);
        // TODO: Revisar porque no cuadra bien el primer cáclculo
        // Mientras nos apañamos con esto
        if (drawTrayectory)
        {
            //
            if (restarted)
            {
                AllocateTrayectoryLineRenderer();
                restarted = false;
            }
            //
            currentTimeBetweenRecalculation += dt;
            if (currentTimeBetweenRecalculation > maxTimeBetweenRecalculation)
            {
                AllocateTrayectoryLineRenderer();
                currentTimeBetweenRecalculation -= maxTimeBetweenRecalculation;
            }
        }
        
	}

    protected virtual void OnCollisionEnter(Collision collision)
    {
        //
        //Debug.Log("Bullet collision with " + collision.collider.gameObject.name);
        GenerateImpact(collision.collider, collision.GetContact(0).point, collision.GetContact(0).normal);
    }

    protected void OnDrawGizmos()
    {
        if(rb != null)
        {
            Debug.DrawRay(transform.position, rb.velocity, Color.blue);
            //Vector3 playerDirection = player.transform.position - transform.position;
            //Debug.DrawRay(transform.position, transform.forward * Time.deltaTime, Color.red);
            Debug.DrawRay(previousPosition, rb.velocity * Time.deltaTime, Color.red);
        }
    }

    //protected void OnDestroy()
    //{
    //    //
    //    if(bulletPool != null)
    //        bulletPool.RemoveDangerousBulletFromList(gameObject);
    //    //
    //    // Destroy(detectionTrail);
    //}

    #region Methods

    //
    void UpdateLifeTime(float dt)
    {
        currentLifeTime += dt;
        if (currentLifeTime >= lifeTime)
        {
            Debug.Log("Bullet lifetime expired");
            //rb.velocity = Vector3.zero;
            //if (trailRenderer)
            //    trailRenderer.Clear();
            //bulletPool.ReturnBullet(gameObject);
            ReturnBulletToPool();
            //
            //if (dangerousEnough)
            //    bulletPool.RemoveDangerousBulletFromList(gameObject);
        }
            
    }

    //
    protected void CheckTravelDone(float dt)
    {
        //
        float distanceToMoveThisStep = rb.velocity.magnitude * dt;
        //
        RaycastHit raycastInfo;
        // Nota: usar la dirección de la velocidad en vez del forward
        if (Physics.Raycast(previousPosition, rb.velocity, out raycastInfo, distanceToMoveThisStep))
        {
            //
            if (raycastInfo.collider.gameObject == gameObject)
                //Debug.Log("Hit itself");
                return;
            //
            //Debug.Log("Bullet raycasting with " + raycastInfo.collider.gameObject.name);
            GenerateImpact(raycastInfo.collider, raycastInfo.point, raycastInfo.normal, dt);
        }
    }

    //
    protected void GenerateImpact(Collider collider, Vector3 hitPoint, Vector3 hitNormal, float dt = 0)
    {
        //
        transform.position = hitPoint;
        GameObject particlesToUse = impactParticlesPrefab;
        AudioClip clipToUse = null;

        // Chequeamos si ha impactado a un enemigo y aplicamos lo necesario
        EnemyCollider enemyCollider = collider.GetComponent<EnemyCollider>();
        if(enemyCollider != null)
        {
            enemyCollider.ReceiveBulletImpact(rb, hitPoint);
            particlesToUse = impactOnBugParticlesPrefab;
            clipToUse = impactOnEnemy;
            // TODO: Buscar otro sitio donde ponerlo
            // Aquí no suena porque se destruye el objeto
            //GeneralFunctions.PlaySoundEffect(audioSource, impactOnEnemy);
            //bulletSoundManager.CreateAudioObject(impactOnEnemy, transform.position);
        }

        // Y el player, joputa
        PlayerIntegrity playerIntegrity = collider.GetComponent<PlayerIntegrity>();
        if(playerIntegrity != null)
        {
            clipToUse = impactOnPlayer;
            playerIntegrity.ReceiveImpact(transform.position, gameObject, rb, hitNormal);
            
            //GeneralFunctions.PlaySoundEffect(audioSource, impactOnPlayer);
            //bulletSoundManager.CreateAudioObject(impactOnPlayer, transform.position);
            //
            //Rigidbody playerRB = playerIntegrity.gameObject.GetComponent<Rigidbody>();
            //playerRB.AddForce(rb.velocity * rb.mass, ForceMode.Impulse);
        }

        // Weakpoints
        // TODO: Gestionarlos mejor
        WeakPoint weakPoint = collider.GetComponent<WeakPoint>();
        if(weakPoint != null)
        {
            clipToUse = impactOnEnemy;
            particlesToUse = impactOnBugParticlesPrefab;
            weakPoint.ReceiveBulletImpact(rb, this);
        }

        // Efecto de sonido
        if(clipToUse != null)
            bulletSoundManager.CreateAudioObject(clipToUse, transform.position, 0.1f);
        
        // Partículas
        if(particlesToUse != null)
        {
            GameObject impactParticles = Instantiate(particlesToUse, hitPoint, Quaternion.identity);
            SpawnBulletHole(hitPoint, hitNormal, collider.gameObject);
            Destroy(impactParticles, 2);
        }
        
        // TODO: Ver por qué nos hacía falta esto
        // Si es explosiva gestionamos la destrucción ahí
        // Y si es player lo gestionams en player
        if(explosiveBullet == null && playerIntegrity == null)
        {
            //Debug.Log("Not explosive component, destroying object");
            // Destroy(gameObject);
            ReturnBulletToPool();
            // TODO: Hcaerlo mas limpio
            //if(dangerousEnough)
            //    bulletPool.RemoveDangerousBulletFromList(gameObject);
        }
            
    }

    //
    public void ReturnBulletToPool()
    {
        //
        if (bulletPool == null)
        {
            Destroy(gameObject);
            return;
        }
        //
        rb.velocity = Vector3.zero;
        if (trailRenderer)
            trailRenderer.Clear();
        if (detectionTrailRenderer)
            detectionTrailRenderer.positionCount = 0;
        bulletPool.ReturnBullet(gameObject);
    }
    

    // 
    void SpawnBulletHole(Vector3 point, Vector3 normal, GameObject objectToParent)
    {
        // 
        PlayerIntegrity playerIntegrity = objectToParent.GetComponent<PlayerIntegrity>();
        
        // Error control vago
        if (bulletHolePrefab == null || playerIntegrity != null )
            return;
        
        // Decidimos si crear agujero de bala o churrete de sangre
        EnemyCollider enemyCollider = objectToParent.GetComponent<EnemyCollider>();
        WeakPoint weakPoint = objectToParent.GetComponent<WeakPoint>();
        GameObject prefabToUse = (enemyCollider != null || weakPoint != null) ? bulletHoleBugPrefab : bulletHolePrefab;
        
        // Y lo creamos
        GameObject newBulletHole = Instantiate(prefabToUse, point, Quaternion.identity);
        newBulletHole.transform.rotation = Quaternion.LookRotation(newBulletHole.transform.forward, normal);
        
        // Lo movemos un pelin para evitar el z clipping
        newBulletHole.transform.position += newBulletHole.transform.up * 0.01f;
        newBulletHole.transform.SetParent(objectToParent.transform);
    }

    // Colocamos trail renderer indicando trayectoria
    public void AllocateTrayectoryLineRenderer()
    {
        //
        float timePerTic = 0.5f;
        int stepsToCheck = (int)(lifeTime / timePerTic);
        Vector3[] positions = new Vector3[stepsToCheck];
        //
        float anticipatedDragEffect = 1 - (rb.drag * timePerTic);
        float speedInStep = rb.velocity.magnitude;
        //
        positions[0] = transform.position;

        //
        for (int i = 1; i < stepsToCheck; i++)
        {
            // TODO: Habrá que tener en cuenta el drag
            // Creo
            //GeneralFunctions.AnticipateObjectivePositionForAiming();
            //GeneralFunctions.GetVelocityWithDistanceAndDrag(rb.velocity.magnitude, , rb.drag, rb.mass);
            float fallInThatTime = Physics.gravity.y * Mathf.Pow(timePerTic * i, 2) / 2;
            //
            speedInStep = speedInStep * anticipatedDragEffect;
            //
            positions[i] = transform.position + (rb.velocity.normalized * speedInStep * timePerTic * i) + new Vector3(0,fallInThatTime,0);
            //positions[i] = positions[i-1] + (rb.velocity.normalized * speedInStep * timePerTic) + new Vector3(0, fallInThatTime, 0);
        }
        //
        if (detectionTrailRenderer)
        {
            detectionTrailRenderer.positionCount = stepsToCheck;
            detectionTrailRenderer.SetPositions(positions);
        }
        else
        {
            Debug.Log("Trying to allocate non-existant line renderer");
        }
        
    }

    #endregion
}
