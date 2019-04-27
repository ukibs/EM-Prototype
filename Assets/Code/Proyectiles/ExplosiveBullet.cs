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
    [Tooltip("Manual stablished explosion range to use with simple explosion")]
    public float simpleExplosionRange = 30;
    public float simpleExplosionSharpnelSpeed = 2000;
    // Esto lo miraremos con más deteniemiento en otro momento
    //https://fas.org/man/dod-101/navy/docs/es310/warheads/Warheads.htm

    //public float explosionForce;
    //public float explosionDamage;

    // Realistic ones
    [Tooltip("Explosive load in kg")]
    public float explosiveLoad;
    //
    public ExplosiveType explosiveType = ExplosiveType.SimpleArea;

    public int fragmentsPerHeight = 4;
    public int fragmentsPerWidth = 4;
    private float fragmentMass;

    private float explosionForce;
    private float explosionForceOwnMeasure;
    private float explosionRange;

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
        explosionForce = explosiveLoad * 4184;
        explosionForceOwnMeasure = explosiveLoad * 4.184f;
        // Recordar que el peso de los rigidbodies lo medimos en toneladas

        // Tenemos en cuenta la disminución de la potencia con el cuadrado de la distancia para el alcance
        // explosiveForce / Mathf.Pow(distance, 2)
        if (explosiveType == ExplosiveType.SimpleArea)
            explosionRange = simpleExplosionRange;
        else
            explosionRange = Mathf.Sqrt(explosionForceOwnMeasure);
        //Debug.Log("Explosion range: " + explosionRange);

        // Vamos a asumir que la masa de cada fragmento es la fracción correspondiente del proyectil
        // Una vez descontada la carga explosiva
        fragmentMass = (proyectileRb.mass - (explosiveLoad / 1000)) / (fragmentsPerHeight * fragmentsPerWidth);
    }

    protected void OnCollisionEnter(Collision collision)
    {

        GenerateExplosion();
    }

    // Se detona automáticamente al destruirse
    // Al menos de momento
    //private void OnDestroy()
    //{
    //    GenerateExplosion();
    //}

    public void GenerateExplosion()
    {
        // De momento, por simplificar
        // Aparte de la fuerza
        // Vamos a imaginar un impacto de fragmento por objetivo alcanzado
        Collider[] affectedBodies = Physics.OverlapSphere(transform.position, explosionRange);
        //Debug.Log("Affected bodies by explosion: " + affectedBodies.Length);
        //
        for(int i = 0; i < affectedBodies.Length; i++)
        {
            // Fuerza física a aplicar
            Rigidbody nextBody = affectedBodies[i].attachedRigidbody;
            Vector3 directionAndDistance = affectedBodies[i].transform.position - transform.position;
            // Minimo de 1 para que no se vaya de madre
            float distanceToCount = Mathf.Max(directionAndDistance.magnitude, 1);
            // OJO: Force to apply sería para la onda expansiva
            float forceToApply = explosionForceOwnMeasure / Mathf.Pow(distanceToCount, 2);
            //
            if (nextBody != null)
            {
                
                //Debug.Log();
                // Recordar que el peso de los rigidbodies va en toneladas
                // Ejemplo de despalzamiento por fuerza explosiva
                // Coche de 1,8 toneladas, 150 kg de TNT, entre 10-15 metros de altura
                nextBody.AddForce(directionAndDistance.normalized * forceToApply, ForceMode.Impulse);
                //
                if(explosiveType == ExplosiveType.SimpleArea)
                {
                    SimpleExplosion(affectedBodies[i], directionAndDistance, distanceToCount, forceToApply);
                }
            }

            
        }
        //
        switch (explosiveType)
        {
            case ExplosiveType.Sharpnel:
                GenerateSharpnels();
                break;
            //case ExplosiveType.SimpleArea:
            //    SimpleExplosion(affectedBodies);
            //    break;
        }
            
        //
        Destroy(gameObject);
    }
    
    //
    public void SimpleExplosion(Collider affectedBody, Vector3 directionAndDistance, float distanceToCount, float forceToApply)
    {
        // Hacemos un falso RB
        FakeRB sharpnelRb = new FakeRB();
        // 
        sharpnelRb.mass = fragmentMass;
        //sharpnelRb.velocity = directionAndDistance.normalized * explosionForce;
        //sharpnelRb.velocity = directionAndDistance.normalized * explosionForceOwnMeasure;
        sharpnelRb.velocity = GeneralFunctions.GetVelocityWithDistanceAndDrag(simpleExplosionSharpnelSpeed, directionAndDistance.magnitude,
                                0.5f, sharpnelRb.mass) * directionAndDistance.normalized;
        //
        EnemyCollider enemyCollider = affectedBody.GetComponent<EnemyCollider>();
        // TODO: Hacer solo aplique impacto a un collider por enemigo
        if (enemyCollider != null)
        {

            //
            //Debug.Log("Sharpnel impacting in " + enemyCollider.transform.name + " with force " + sharpnelRb.Force);
            enemyCollider.ReceiveSharpnelImpact(sharpnelRb, enemyCollider.transform.position);
        }

        PlayerIntegrity playerIntegrity = affectedBody.GetComponent<PlayerIntegrity>();
        if (playerIntegrity != null)
        {
            //
            //Debug.Log("Sharpnel impacting in player with mass " + sharpnelRb.mass + ", velocity " + sharpnelRb.velocity.magnitude + ", force " + sharpnelRb.Force);
            //
            Vector3 impactPositionForDirection = playerIntegrity.transform.position - transform.position;
            //
            playerIntegrity.ReceiveSharpnelImpact(impactPositionForDirection, gameObject, sharpnelRb);

        }
    }

    //
    public void GenerateSharpnels()
    {
        //
        float angleOffsetInHeight = 360 / fragmentsPerHeight;
        float angleOffsetInWidth = 360 / fragmentsPerWidth;
        for(int i = 0; i < fragmentsPerHeight; i++)
        {
            for(int j = 0; j < fragmentsPerWidth; j++)
            {
                RaycastHit hit;
                Vector3 proyectileDirection = (Quaternion.AngleAxis((angleOffsetInHeight * i) + (angleOffsetInHeight/2), Vector3.right) *
                                                Quaternion.AngleAxis((angleOffsetInWidth * j) + (angleOffsetInWidth/2), Vector3.up)) * Vector3.one;
                Debug.Log("Fragment " + i + ", " + j + ", direction: " + proyectileDirection);
                if (Physics.Raycast(transform.position, proyectileDirection, out hit, explosionRange))
                {
                    //
                    Debug.Log(hit.transform.name);
                    // Hacemos un falso RB
                    FakeRB sharpnelRb = new FakeRB();
                    // 
                    sharpnelRb.mass = fragmentMass;
                    //sharpnelRb.velocity = directionAndDistance * explosionForce;
                    sharpnelRb.velocity = proyectileDirection * explosionForceOwnMeasure;
                    // TODO: Aplicar daños bien
                    Debug.Log("Fragment " + i + ", " + j + ", mass: " + sharpnelRb.mass + ", velocity magnitude: " + sharpnelRb.velocity.magnitude);
                    // 
                    EnemyCollider enemyCollider = hit.collider.GetComponent<EnemyCollider>();
                    // TODO: Hacer solo aplique impacto a un collider por enemigo
                    if (enemyCollider != null)
                    {

                        //
                        Debug.Log("Sharpnel impacting in " + enemyCollider.transform.name + " with force " + sharpnelRb.Force);
                        enemyCollider.ReceiveSharpnelImpact(sharpnelRb, enemyCollider.transform.position);
                    }

                    PlayerIntegrity playerIntegrity = hit.collider.GetComponent<PlayerIntegrity>();
                    if (playerIntegrity != null)
                    {
                        //
                        Debug.Log("Sharpnel impacting in player with force " + sharpnelRb.Force);
                        //
                        Vector3 impactPositionForDirection = playerIntegrity.transform.position - transform.position;
                        //
                        playerIntegrity.ReceiveSharpnelImpact(impactPositionForDirection, gameObject, sharpnelRb);

                    }
                }
            }
        }
    }

    // Esta la haremos para los proyectiles disñeados para explotar dentro del objetivo
    // Esto se traducirán en daño adicional en función de la carga explosiva
    public void GenerateInternalExplosion(EnemyConsistency enemyConsistency)
    {

    }
}

public class FakeRB
{
    public float mass;
    public Vector3 velocity;

    public float Force
    {
        get
        {
            return mass * Mathf.Pow(velocity.magnitude, 2);
        }
    }
}