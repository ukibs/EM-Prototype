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

    // TODO: Decidir si implementamos el funcionamiendo de misil aqui como opción (impulso constante en vez de inicial)

    protected Rigidbody rb;
    protected Vector3 previousPosition;

	// Use this for initialization
	protected virtual void Start () {
        //Debug.Log("Starting bullet");
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);
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
        // Chequeamos si hemos chocado con un enemigo o el player
        // Y actuamos en consecuencia
        EnemyCollider enemyCollider = collision.collider.GetComponent<EnemyCollider>();
        PlayerIntegrity playerIntegrity = collision.collider.GetComponent<PlayerIntegrity>();
        ContactPoint collisionPoint = collision.GetContact(0);
        if (enemyCollider != null)
            enemyCollider.ReceiveBulletImpact(rb, collisionPoint.point);
        else if (playerIntegrity != null)
            playerIntegrity.ReceiveImpact(collisionPoint.point, gameObject, rb);
        // Ponemos las particulas y el agujero
        GameObject impactParticles = Instantiate(impactParticlesPrefab, transform.position, Quaternion.identity);
        SpawnBulletHole(collisionPoint.point, collisionPoint.normal, collision.gameObject);
        //
        Destroy(impactParticles, 2);
        //
        Destroy(gameObject);
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
            GenerateImpact(raycastInfo, dt);
            // TODO: Aplicar fuerzas

        }
    }

    //
    protected void GenerateImpact(RaycastHit raycastInfo, float dt)
    {
        // Chequeamos si ha impactado a un enemigo y aplicamos lo necesario
        EnemyCollider enemyCollider = raycastInfo.collider.GetComponent<EnemyCollider>();
        if(enemyCollider != null)
        {
            enemyCollider.ReceiveBulletImpact(rb, raycastInfo.point);
        }
        // Y el player, joputa
        PlayerIntegrity playerIntegrity = raycastInfo.collider.GetComponent<PlayerIntegrity>();
        if(playerIntegrity != null)
        {
            playerIntegrity.ReceiveImpact(rb.velocity, gameObject, rb);
            //
            Rigidbody playerRB = playerIntegrity.gameObject.GetComponent<Rigidbody>();
            playerRB.AddForce(rb.velocity * rb.mass, ForceMode.Impulse);
        }
        //
        GameObject impactParticles = Instantiate(impactParticlesPrefab, raycastInfo.point, Quaternion.identity);
        SpawnBulletHole(raycastInfo.point, raycastInfo.normal, raycastInfo.collider.gameObject);
        Destroy(impactParticles, 2);
        //
        Destroy(gameObject, dt);
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
