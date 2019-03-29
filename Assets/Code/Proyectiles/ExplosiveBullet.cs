﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cuando lo usemos usaremos los ratios que usan el TNT como referencia (valor 1)
public enum ExplosiveType
{
    Invalid = -1,

    TNT,

    Count
}

/// <summary>
/// Clase explosivo
/// Lemetos como componente adjunto en vez de herenci
/// </summary>
public class ExplosiveBullet : MonoBehaviour
{
    // Genereic ones
    //public float explosionRange;
    //public float explosionForce;
    //public float explosionDamage;

    // Realistic ones
    [Tooltip("Explosive load in kg")]
    public float explosiveLoad;

    private float explosionForce;
    private float explosionRange;

    // TODO: Decidir si usamos fragmentos

    protected void Start()
    {
        
        // We calculate the force with the proportion of kilograms in TNT
        //explosionForce = explosiveLoad * 4184000;
        explosionForce = explosiveLoad * 4184;
        // Recordar que el peso de los rigidbodies lo medimos en toneladas

        // Tenemos en cuenta la disminución de la potencia con el cuadrado de la distancia para el alcance
        // explosiveForce / Mathf.Pow(distance, 2)
        explosionRange = Mathf.Sqrt(explosionForce);


    }

    //protected void OnCollisionEnter(Collision collision)
    //{
        
    //    GenerateExplosion();
    //}

    // Se detona automáticamente al destruirse
    // Al menos de momento
    private void OnDestroy()
    {
        GenerateExplosion();
    }

    public void GenerateExplosion()
    {
        // De momento, por simplificar
        // Aparte de la fuerza
        // Vamos a imaginar un impacto de fragmento por objetivo alcanzado
        Collider[] affectedBodies = Physics.OverlapSphere(transform.position, explosionRange);
        //
        for(int i = 0; i < affectedBodies.Length; i++)
        {
            // Fuerza física a aplicar
            Rigidbody nextBody = affectedBodies[i].attachedRigidbody;
            if (nextBody != null)
            {
                Vector3 directionAndDistance = affectedBodies[i].transform.position - transform.position;
                float distanceToCount = Mathf.Max(directionAndDistance.magnitude, 1);
                //Debug.Log();
                // Recordar que el peso de los rigidbodies va en toneladas
                // nextBody.AddForce(directionAndDistance.normalized * explosionForce / Mathf.Pow(distanceToCount, 2) / 1000, ForceMode.Impulse);
            }

            // TODO: Aplicar daños bien

            // 
            //EnemyCollider enemyCollider = affectedBodies[i].GetComponent<EnemyCollider>();
            //if(enemyCollider != null)
            //{

            //}

            //PlayerIntegrity playerIntegrity = affectedBodies[i].GetComponent<PlayerIntegrity>();

        }
    }
}
