using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    // Seguramente metamos también la velocidad de salid

    [Tooltip("Diameter in mm")]
    public float diameter;
    [Tooltip("Short for quick bullets long for artillery and other slow ones")]
    public float lifeTime = 10;
    public GameObject impactParticlesPrefab;
    public GameObject bulletHolePrefab;

    // De momento hacemos dos, para player y enemigo
    // TODO: hacer un atrapado más general
    public AudioClip impactOnPlayer;
    public AudioClip impactOnEnemy;

    // TODO: Decidir si implementamos el funcionamiendo de misil aqui como opción (impulso constante en vez de inicial)

    protected Rigidbody rb;
    protected Vector3 previousPosition;
    // TODO: hacer un atrapado más general
    //protected AudioSource audioSource;
    // Provisional, para que se destruya en ek explosivo en vez de en este
    protected ExplosiveBullet explosiveBullet;
    //
    protected BulletSoundManager bulletSoundManager;

	// Use this for initialization
	protected virtual void Start () {
        //Debug.Log("Starting bullet");
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);
        //audioSource = GetComponent<AudioSource>();
        //
        explosiveBullet = GetComponent<ExplosiveBullet>();
        //
        bulletSoundManager = FindObjectOfType<BulletSoundManager>();
    }

    protected void FixedUpdate()
    {
        previousPosition = transform.position;
    }

    // Update is called once per frame
    protected void Update () {
        //
        float dt = Time.deltaTime;
        // Hacemos que vaya cambiando la orientación acorde a la trayectoria
        // Ahora que estamos haciendo probatinas con esfericas no se notará
        // TODO: Chequear cuando tengamos balas más definidas
        transform.LookAt(rb.velocity);
        //
        CheckTravelDone(dt);
	}

    protected virtual void OnCollisionEnter(Collision collision)
    {
        //
        Debug.Log("Bullet collision with " + collision.collider.gameObject.name);
        GenerateImpact(collision.collider, collision.GetContact(0).point, collision.GetContact(0).normal);
    }

    protected void OnDrawGizmos()
    {
        if(rb != null)
        {
            //Debug.DrawRay(transform.position, rb.velocity * Time.deltaTime, Color.blue);
            //Vector3 playerDirection = player.transform.position - transform.position;
            //Debug.DrawRay(transform.position, transform.forward * Time.deltaTime, Color.red);
            Debug.DrawRay(previousPosition, rb.velocity * Time.deltaTime, Color.red);
        }
    }

    #region Methods

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
            Debug.Log("Bullet raycasting with " + raycastInfo.collider.gameObject.name);
            GenerateImpact(raycastInfo.collider, raycastInfo.point, raycastInfo.normal, dt);
            // TODO: Aplicar fuerzas

        }
    }

    //
    protected void GenerateImpact(Collider collider, Vector3 hitPoint, Vector3 hitNormal, float dt = 0)
    {
        //
        transform.position = hitPoint;
        // Chequeamos si ha impactado a un enemigo y aplicamos lo necesario
        EnemyCollider enemyCollider = collider.GetComponent<EnemyCollider>();
        if(enemyCollider != null)
        {
            enemyCollider.ReceiveBulletImpact(rb, hitPoint);
            // TODO: Buscar otro sitio donde ponerlo
            // Aquí no suena porque se destruye el objeto
            //GeneralFunctions.PlaySoundEffect(audioSource, impactOnEnemy);
            bulletSoundManager.CreateAudioObject(impactOnEnemy, transform.position);
        }
        // Y el player, joputa
        PlayerIntegrity playerIntegrity = collider.GetComponent<PlayerIntegrity>();
        if(playerIntegrity != null)
        {
            playerIntegrity.ReceiveImpact(rb.velocity, gameObject, rb);
            //GeneralFunctions.PlaySoundEffect(audioSource, impactOnPlayer);
            bulletSoundManager.CreateAudioObject(impactOnPlayer, transform.position);
            //
            Rigidbody playerRB = playerIntegrity.gameObject.GetComponent<Rigidbody>();
            playerRB.AddForce(rb.velocity * rb.mass, ForceMode.Impulse);
        }
        //
        GameObject impactParticles = Instantiate(impactParticlesPrefab, hitPoint, Quaternion.identity);
        SpawnBulletHole(hitPoint, hitNormal, collider.gameObject);
        Destroy(impactParticles, 2);
        // TODO: Ver por qué nos hacía falta esto
        if(explosiveBullet == null)
            Destroy(gameObject);
    }

    // 
    void SpawnBulletHole(Vector3 point, Vector3 normal, GameObject objectToParent)
    {
        // 
        PlayerIntegrity playerIntegrity = objectToParent.GetComponent<PlayerIntegrity>();
        // Error control vago
        if (bulletHolePrefab == null || playerIntegrity != null )
            return;
            //
        GameObject newBulletHole = Instantiate(bulletHolePrefab, point, Quaternion.identity);
        newBulletHole.transform.rotation = Quaternion.LookRotation(newBulletHole.transform.forward, normal);
        // Lo movemos un pelin para evitar el z clipping
        newBulletHole.transform.position += newBulletHole.transform.up * 0.01f;
        newBulletHole.transform.SetParent(objectToParent.transform);
    }

    #endregion
}
