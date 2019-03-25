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

	// Use this for initialization
	protected virtual void Start () {
        //Debug.Log("Starting bullet");
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);
    }
	
	// Update is called once per frame
	protected void FixedUpdate () {
        //
        float dt = Time.deltaTime;
        // Hacemos que vaya cambiando la orientación acorde a la trayectoria
        // Ahora que estamos haciendo probatinas con esfericas no se notará
        // TODO: Chequear cuando tengamos balas más definidas
        transform.LookAt(rb.velocity);
        //
        float distanceToMoveThisStep = rb.velocity.magnitude * dt;
        //
        RaycastHit raycastInfo;
        // Nota: usar la dirección de la velocidad en vez del forward
        if (Physics.Raycast(transform.position, transform.forward, out raycastInfo, distanceToMoveThisStep))
        {
            // Nota: Probar a hacer el impacto "a mano"
            //transform.position = raycastInfo.point;
            GenerateImpact(raycastInfo, dt);
            // TODO: Aplicar fuerzas

        }
	}

    protected virtual void OnCollisionEnter(Collision collision)
    {
        //
        EnemyCollider enemyCollider = collision.collider.GetComponent<EnemyCollider>();
        ContactPoint collisionPoint = collision.GetContact(0);
        if (enemyCollider != null)
            enemyCollider.ReceiveBulletImpact(rb, collisionPoint.point);
        //Debug.Log(collision.collider.gameObject.name + " impacted with " + collision.relativeVelocity + " speed.");
        //Debug.Log(collision.collider.gameObject.gameObject.name + " impacted with " + collision.relativeVelocity +
        //    " speed and " + rb.mass +
        //        " mass. With a total force of " + (rb.velocity.magnitude * rb.mass) + ".");
        //
        GameObject impactParticles = Instantiate(impactParticlesPrefab, transform.position, Quaternion.identity);
        SpawnBulletHole(collisionPoint.point, collisionPoint.normal, collision.gameObject);
        Destroy(impactParticles, 2);
        //
        Destroy(gameObject, 0.5f);
    }

    protected void OnDrawGizmos()
    {
        if(rb != null)
        {
            Debug.DrawRay(transform.position, rb.velocity * Time.deltaTime, Color.blue);
            //Vector3 playerDirection = player.transform.position - transform.position;
            //Debug.DrawRay(transform.position, playerDirection, Color.red);
        }
    }

    #region Methods

    protected void GenerateImpact(RaycastHit raycastInfo, float dt)
    {
        // Chequeamos si ha impactado a un enemigo y aplicamos lo necesario
        EnemyCollider enemyCollider = raycastInfo.collider.GetComponent<EnemyCollider>();
        if(enemyCollider != null)
        {
            //Rigidbody enemyRB = enemyCollider.gameObject.GetComponent<Rigidbody>();
            //enemyRB.AddForce(rb.velocity * rb.mass);

            enemyCollider.ReceiveBulletImpact(rb, raycastInfo.point);

            //Debug.Log(enemyConsistency.gameObject.name + " impacted with " + rb.velocity + " speed and " + rb.mass + 
            //    " mass. With a total force of " + (rb.velocity.magnitude * rb.mass) + ".");
        }
        // Y el player, joputa
        PlayerIntegrity playerIntegrity = raycastInfo.collider.GetComponent<PlayerIntegrity>();
        if(playerIntegrity != null)
        {
            playerIntegrity.ReceiveImpact(rb.velocity, gameObject, rb);
            //
            Rigidbody playerRB = playerIntegrity.gameObject.GetComponent<Rigidbody>();
            rb.AddForce(rb.velocity * rb.mass);
        }
        //
        GameObject impactParticles = Instantiate(impactParticlesPrefab, raycastInfo.point, Quaternion.identity);
        SpawnBulletHole(raycastInfo.point, raycastInfo.normal, raycastInfo.collider.gameObject);
        Destroy(impactParticles, 2);
        //
        Destroy(gameObject, dt);
    }

    // TODO: Emparentarlo con el objeto impactado
    void SpawnBulletHole(Vector3 point, Vector3 normal, GameObject objectToParent)
    {
        // TODO: Que no se peguen a EM (al menos mientras le quede escudo)
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
