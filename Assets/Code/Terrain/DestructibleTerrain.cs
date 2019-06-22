using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleTerrain : MonoBehaviour
{
    //
    public GameObject brokenVersion;
    // TODO: Trabajar esto más en un futuro
    // Con masa, densidad o lo que sea
    public bool hardEnough = false;
    //
    private bool destroyed = false;
    

    private void Start()
    {
        //brokenVersion = transform.Find
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckAndDestroy(collision);        
    }

    public void CheckAndDestroy(Collision collision)
    {
        //
        //Debug.Log(transform.parent.name + " colliding with " + collision.gameObject.name);
        // TODO: De momento chequeo simple
        // Quedetecte si es el gusano
        // Y si lo es se destruye
        GigaWormBehaviour wormBehaviour = collision.collider.GetComponentInParent<GigaWormBehaviour>();
        //
        if (wormBehaviour != null)
        {
            wormBehaviour.ImpactWithTerrain(hardEnough);
        }
        // Con kinematicos
        // Con suerte nos podremos quedar con este
        Rigidbody colliderRb = collision.rigidbody;
        if(colliderRb != null && colliderRb.isKinematic)
        {
            gameObject.SetActive(false);
            if (brokenVersion != null)
                brokenVersion.SetActive(true);
        }

    }

    // Llamaremos a esto cuando cambiemos la placa de sitio
    public void Restore()
    {
        // Aquí volveremos el elemento a su estado original
        gameObject.SetActive(true);
        if (brokenVersion != null)
            brokenVersion.SetActive(false);
    }
}
