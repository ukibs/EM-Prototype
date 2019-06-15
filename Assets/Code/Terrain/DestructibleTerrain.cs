using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleTerrain : MonoBehaviour
{
    //
    public GameObject brokenVersion;
    //
    private bool destroyed = false;
    

    private void Start()
    {
        //brokenVersion = transform.Find
    }

    private void OnCollisionEnter(Collision collision)
    {
        //
        Debug.Log("Colliding with " + collision.gameObject.name);
        // De momento chequeo simple
        // Quedetecte si es el gusano
        // Y si lo es se destruye
        GigaWormBehaviour wormBehaviour = collision.collider.GetComponentInParent<GigaWormBehaviour>();
        //
        if(wormBehaviour != null)
        {
            gameObject.SetActive(false);
            if(brokenVersion != null)
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
