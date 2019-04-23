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
    //private Rigidbody bodyRb;

    public float Armor {
        get { return armor; }
        set { armor = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Cogemos el componente cuerpo del padre
        if (transform.parent != null)
            //body = transform.parent.GetComponent<EnemyConsistency>();
            body = transform.GetComponentInParent<EnemyConsistency>();
        // Para casos en los que el body solo tiene un collider
        // y por tanto lo lleva integrado
        if (body == null)
            body = GetComponent<EnemyConsistency>();

        
        //
        //bodyRb = body.GetComponent<Rigidbody>();
    }
    
    //private void OnCollisionEnter(Collision collision)
    //{
        //Que no esté muerto
        //if (body == null)
        //    return;

        // Trataremos de forma diferente los impactos de las balas y el resto
        //Bullet bullet = collision.collider.GetComponent<Bullet>();
        
        //
        //string bulletConfimation = (bullet != null) ? "Yes" : "No";
        //Debug.Log(collision.collider.gameObject.name + ", has bullet component: " + bulletConfimation);
        
        //
        //Rigidbody otherRb = collision.collider.GetComponent<Rigidbody>();
        //float impactForce = GeneralFunctions.GetCollisionForce(bodyRb, otherRb);
        
        //
        //if (bullet != null)
        //{
        //    Debug.Log("Bullet collision detected by EnemyCollider");
        //    body.ReceiveInternalImpact(impactForce, collision.contacts[0].point);
        //}

        //else if(collision.contacts[0].point != null)
        //    impactInfoManager.SendImpactInfo(collision.contacts[0].point, impactForce, "No damage");
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bulletRb"></param>
    /// <param name="impactPoint"></param>
    public void ReceiveBulletImpact(Rigidbody bulletRb, Vector3 impactPoint)
    {
        
        // Chequeamos que siga vivo
        if(body != null)
        {
            //
            // TODO: Revisar velocidades relativas
            Bullet bulletData = bulletRb.transform.GetComponent<Bullet>();
            float diameter = bulletData.diameter;

            float penetrationValue = GeneralFunctions.Navy1940PenetrationCalc(bulletRb.mass, diameter, bulletRb.velocity.magnitude);
            //Debug.Log("Penetration value: " + penetrationValue + ", mass: " + bulletRb.mass + 
            //    ", diameter: " + diameter + ", velocity: " + bulletRb.velocity.magnitude);
            float penetrationResult = Mathf.Max(penetrationValue - armor, 0);
            // TODO: Unificar esta funcionalidad entre consistncy y collider
            // Pasamos en qué proporción ha penetrado
            if (penetrationResult > 0)
                penetrationResult = 1 - (armor / penetrationValue);
            //
            body.ReceiveProyectileImpact(penetrationResult, impactPoint, bulletRb);
        }
        
    }

    // FUncion provisional para los fragmentos
    public void ReceiveSharpnelImpact(FakeRB sharpnelRb, Vector3 impactPoint)
    {
        // Chequeamos que siga vivo
        if (body != null)
        {
            //
            // TODO: Revisar velocidades relativas
            // Vamos a asumir un diametro para fragmentos
            // De momento 10 mm
            float diameter = 10;

            float penetrationValue = GeneralFunctions.Navy1940PenetrationCalc(sharpnelRb.mass, diameter, sharpnelRb.velocity.magnitude);
            //Debug.Log("Penetration value: " + penetrationValue + ", mass: " + bulletRb.mass + 
            //    ", diameter: " + diameter + ", velocity: " + bulletRb.velocity.magnitude);
            float penetrationResult = Mathf.Max(penetrationValue - armor, 0);
            //
            body.ReceiveSharpnelImpact(penetrationResult, impactPoint, sharpnelRb);
        }
    }

}
