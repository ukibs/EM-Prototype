using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityFuze : MonoBehaviour
{
    //
    float detectionRadius;
    //
    private ExplosiveBullet explosiveBullet;
    // Start is called before the first frame update
    void Start()
    {
        explosiveBullet = GetComponent<ExplosiveBullet>();
        if (explosiveBullet == null)
            Debug.Log(gameObject.name + "should have the ExplosiveBullet component");
    }

    // Update is called once per frame
    void Update()
    {
        if (explosiveBullet != null)
        {
            // Encuanto detecte algo en el radio que explote
            // OJO: Probablemente salta con el propio lanzador
            // TODO: Revisarlo cuando ocurra
            // Bien con timer para que mpiece a chequear
            // Bien con filto
            if(Physics.OverlapSphere(transform.position, detectionRadius).Length > 0)
            {
                explosiveBullet.GenerateExplosion();
            }
        }
    }

    
}
