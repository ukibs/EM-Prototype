using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollider : MonoBehaviour
{
    [Tooltip("Armor thickness on this side")]
    public float armor = 10;

    // TODO: Manejar dureza de material
    // Y otras propiedades en el futuro

    private EnemyConsistency body;

    public float Armor {
        get { return armor; }
        set { armor = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Cogemos el componente cuerpo del padre
        body = transform.parent.GetComponent<EnemyConsistency>();
        // Para casos en los que el body solo tiene un collider
        // y por tanto lo lleva integrado
        if(body == null)
            body = GetComponent<EnemyConsistency>();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
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

        if (bullet == null && impactForce > body.Defense)
            body.ReceiveImpact(impactForce, collision.contacts[0].point);
        // 
        else if (bullet != null && impactForce > armor)
        {
            //CheckImpactedPart(collision.collider);
            body.ReceiveInternalImpact(impactForce, collision.contacts[0].point, armor);
        }

        //else if(collision.contacts[0].point != null)
        //    impactInfoManager.SendImpactInfo(collision.contacts[0].point, impactForce, "No damage");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bulletRb"></param>
    /// <param name="impactPoint"></param>
    public void ReceiveBulletImpact(Rigidbody bulletRb, Vector3 impactPoint)
    {
        //
        // TODO: Revisar velocidades relativas
        //Vector3 relativeVelocity = bulletRb.velocity - rb.velocity;
        float impactForce = bulletRb.velocity.magnitude * bulletRb.mass;
        // Pasamos julios
        impactForce *= 1000;
        // Chequeamos que siga vivo
        if(body != null)
        {
            //
            //impactInfoManager.SendImpactInfo(impactPoint, impactForce, "No damage");
            //
            body.ReceiveInternalImpact(impactForce, impactPoint, armor);
        }
        
    }
}
