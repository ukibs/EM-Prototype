using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cuando lo usemos usaremos los ratios que usan el TNT como referencia (valor 1)
public enum MixtureType
{
    Invalid = -1,

    TNT,

    Count
}

public enum ExplosiveType
{
    Invalid = -1,

    SimpleArea,
    Sharpnel,

    Count
}

/// <summary>
/// Clase explosivo
/// Lemetos como componente adjunto en vez de herenci
/// </summary>
public class ExplosiveBullet : MonoBehaviour
{
    // Genereic ones
    //[Tooltip("Manual stablished explosion range to use with simple explosion")]
    //public float simpleExplosionRange = 30;
    //public float simpleExplosionSharpnelSpeed = 2000;
    // Esto lo miraremos con más deteniemiento en otro momento
    //https://fas.org/man/dod-101/navy/docs/es310/warheads/Warheads.htm

    //public float explosionForce;
    //public float explosionDamage;

    // Realistic ones
    [Tooltip("Explosive load in g of TNT equivalent")]
    public float explosiveLoad;
    //
    //public ExplosiveType explosiveType = ExplosiveType.SimpleArea;

    [Tooltip("Energy 'bombs' shouldn't generate fragments")]
    public bool generatesFragments = false;
    public GameObject fragmentPrefab;
    public int fragmentsPerHeight = 4;
    public int fragmentsPerWidth = 4;

    private float fragmentMass;

    private float explosionForce;
    //private float explosionForceOwnMeasure;
    private float shockWaveRange;

    private Rigidbody proyectileRb;
    // TODO: Decidir si usamos fragmentos

    protected void Start()
    {
        //
        proyectileRb = GetComponent<Rigidbody>();

        // Tenemos que mirar bien la conversion kg/TNT -> julios -> newton
        // Julios = newtons/m
        // Para aplicar la cantidad correcta de fuerza
        // Recordando también kilos -> toneladas


        // We calculate the force with the proportion of kilograms in TNT
        //explosionForce = explosiveLoad * 4184000;
        explosionForce = explosiveLoad * 4.184f;
        shockWaveRange = Mathf.Sqrt(explosionForce);
        Debug.Log("Explosion range: " + shockWaveRange);
        // Recordar que el peso de los rigidbodies lo medimos en toneladas
        

        // Vamos a asumir que la masa de cada fragmento es la fracción correspondiente del proyectil
        // Una vez descontada la carga explosiva
        if (generatesFragments)
            fragmentMass = (proyectileRb.mass - (explosiveLoad / 1000)) / (fragmentsPerHeight * fragmentsPerWidth);
    }

    protected void OnCollisionEnter(Collision collision)
    {
        //
        //Debug.Log("Explosive bullet collided with: " + collision.gameObject.name);
        //Time.timeScale = 0;
        //
        GenerateExplosion();
    }
    

    //
    public void GenerateExplosion()
    {
        // Primero aplicamos la onda de choque
        // Vamos a hacer que este daño vaya contra la "estrucura"
        Collider[] affectedColliders = Physics.OverlapSphere(transform.position, shockWaveRange);
        //
        for(int i = 0; i < affectedColliders.Length; i++)
        {
            //
            Vector3 affectedColliderDirection = affectedColliders[i].transform.position - transform.position;
            float receivedBlastForce = explosionForce / Mathf.Pow(affectedColliderDirection.magnitude, 2);
            Vector3 blastForceWithDirection = affectedColliderDirection.normalized * receivedBlastForce;
            //
            PlayerIntegrity playerIntegrity = affectedColliders[i].GetComponent<PlayerIntegrity>();
            if (playerIntegrity != null)
                playerIntegrity.ReceiveBlastDamage(blastForceWithDirection);
            //
            EnemyCollider enemyCollider = affectedColliders[i].GetComponent<EnemyCollider>();
            if (enemyCollider != null)
                enemyCollider.ReceivePulseDamage(blastForceWithDirection);
            // Después enemy consistencies
            EnemyConsistency enemyConsistency = affectedColliders[i].GetComponent<EnemyConsistency>();
            if (enemyConsistency == null)
                enemyConsistency = affectedColliders[i].GetComponentInParent<EnemyConsistency>();
            if (enemyConsistency != null)
                enemyConsistency.ReceivePulseDamage(blastForceWithDirection);
            //
            DestructibleTerrain destructibleTerrain = affectedColliders[i].GetComponent<DestructibleTerrain>();
            if (destructibleTerrain != null)
                destructibleTerrain.ReceivePulseImpact(blastForceWithDirection);
            // Aplicamos fuerza directa a los rigidbodies que no son el player ni los enemigos
            // Estos se lo gestionan en la funcióbn de recibir daño de explosión
            Rigidbody rigidbody = affectedColliders[i].GetComponent<Rigidbody>();
            if (rigidbody != null && enemyConsistency == null && playerIntegrity == null && rigidbody != proyectileRb)
                rigidbody.AddForce(blastForceWithDirection / 1000);
        }
        //
        if(generatesFragments)
            GenerateFragments();
        //
        Destroy(gameObject);
    }

    //
    public void GenerateFragments()
    {
        //
        float angleOffsetInHeight = 360 / fragmentsPerHeight;
        float angleOffsetInWidth = 360 / fragmentsPerWidth;
        //
        float chargeMassRatio = explosiveLoad / proyectileRb.mass;
        float squareKCoefficient = 1 / 3;
        float fragmentSpeed = Mathf.Sqrt(2 * explosionForce * chargeMassRatio / (1 + squareKCoefficient * chargeMassRatio));
        //
        for(int i = 0; i < fragmentsPerHeight; i++)
        {
            for(int j = 0; j < fragmentsPerWidth; j++)
            {
                //RaycastHit hit;
                Vector3 proyectileDirection = (Quaternion.AngleAxis((angleOffsetInHeight * i) + (angleOffsetInHeight/2), Vector3.right) *
                                                Quaternion.AngleAxis((angleOffsetInWidth * j) + (angleOffsetInWidth/2), Vector3.up)) * Vector3.one;
                //Debug.Log("Fragment " + i + ", " + j + ", direction: " + proyectileDirection);

                GameObject nextFragment = Instantiate(fragmentPrefab, transform.position, Quaternion.LookRotation(proyectileDirection));
                Rigidbody fragmentRb = nextFragment.GetComponent<Rigidbody>();
                //
                fragmentRb.velocity = proyectileDirection.normalized * fragmentSpeed;
            }
        }
    }

    // Esta la haremos para los proyectiles disñeados para explotar dentro del objetivo
    // Esto se traducirán en daño adicional en función de la carga explosiva
    public void GenerateInternalExplosion(EnemyConsistency enemyConsistency)
    {
        // TODO: Hcaerla. Hacer función dentro de 
        //enemyConsistency.
    }
}
