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
    public float structuralResistance = 1000;
    //
    public AudioClip destructionClip;

    //
    private bool destroyed = false;
    private AudioObjectManager audioObjectManager;
    private Rigidbody[] brokenVersionRigidbodies;
    

    private void Start()
    {
        //brokenVersion = transform.Find
        audioObjectManager = FindObjectOfType<AudioObjectManager>();
        //
        brokenVersionRigidbodies = brokenVersion.GetComponents<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckAndDestroy(collision);        
    }

    // TODO: Afina y completa
    public void ReceivePulseImpact(Vector3 directionWithForce)
    {
        if(directionWithForce.magnitude > structuralResistance)
        {
            DestroyTerrainElement(directionWithForce);
        }
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
        if(colliderRb != null)
        {
            // Si es kinematico destruir y punto
            if(colliderRb.isKinematic)
                DestroyTerrainElement();
            // Y chequeando fuerza
            float colliderForce = colliderRb.velocity.magnitude;
            if(colliderForce > structuralResistance)
                DestroyTerrainElement();
        }
        
    }

    //
    public void DestroyTerrainElement(Vector3 directionAndForce = new Vector3())
    {
        //
        gameObject.SetActive(false);
        if (brokenVersion != null)
            brokenVersion.SetActive(true);
        //
        if (audioObjectManager == null)
        {
            Debug.Log("Audio object manager still not ready");
            return;
        }
        //
        if (destructionClip != null)
            audioObjectManager.CreateAudioObject(destructionClip, transform.position);
    }

    //
    void ApplyForceToBrokenParts(Vector3 directionAndForce = new Vector3())
    {
        //
        for(int i = 0; i < brokenVersionRigidbodies.Length; i++)
        {
            brokenVersionRigidbodies[i].AddForce(directionAndForce, ForceMode.Impulse);
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
