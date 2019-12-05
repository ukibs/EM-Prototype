using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// La funcionalidad de estos puntos es avanzar fases en el enfrntamiento
// Demomento lo hacemos específico del gusano
// Luego lo generalizaroremos

public class WeakPoint : Targeteable
{
    #region Public Attibutes

    public float maxHealthPoints = 30;
    public Material unveiledMaterial;
    public bool reactionOnDamage = false;
    public BossBaseBehaviour bossBehaviour;
    public GameObject destructionParticles;
    public string tagForBoss;

    #endregion

    #region Private Attributes


    private float currentHealthPoints;
    private CarolBaseHelp carolBaseHelp;
    private EnemyCollider enemyCollider;

    #endregion

    public float CurrentHealthPoins { get { return currentHealthPoints; } }

    // Start is called before the first frame update
    void Start()
    {
        //gigaWormBehaviour = GetComponentInParent<GigaWormBehaviour>();
        currentHealthPoints = maxHealthPoints;
        carolBaseHelp = FindObjectOfType<CarolBaseHelp>();
        enemyCollider = GetComponent<EnemyCollider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        ReceiveBulletImpact(collision.rigidbody, collision.collider.GetComponent<Bullet>());
    }

    public void ReceiveBulletImpact(Rigidbody impactingRigidbody, Bullet bulletComponent)
    {
        // TODO: Cambiar la detección a proximidad
        // Seguramente lo haga la propia Carol
        if(active == false && currentHealthPoints > 0)
        {
            Unveil();
            // TODO: Revisar esto
            carolBaseHelp.TriggerIt();
        }
        //
        if(currentHealthPoints > 0)
        {
            //currentHealthPoints--;
            if(bulletComponent != null)
                ManageImpact(impactingRigidbody, bulletComponent);
            else { /* Algo pondremos aqui*/ }
            //
            if (reactionOnDamage)
            {
                bossBehaviour.RespondToDamagedWeakPoint(tagForBoss);
            }
            //
            if (currentHealthPoints <= 0)
            {
                bossBehaviour.LoseWeakPoint(tagForBoss);
                carolBaseHelp.WeakPointDestroyed();
                //TODO: Meteremos el churrazo de sangre
                active = false;
                Instantiate(destructionParticles, transform.position, Quaternion.identity);
                gameObject.SetActive(false);
                //Destroy(this);
                //Destroy(gameObject);
                EnemyAnalyzer.Release();
            }
        }
        
    }

    private void ManageImpact(Rigidbody impactingRigidbody, Bullet bulletComponent)
    {
        // Primero determinamos penetración
        float penetrationValue = 
            GeneralFunctions.Navy1940PenetrationCalc(impactingRigidbody.mass, 
            bulletComponent.diameter, impactingRigidbody.velocity.magnitude);
        //
        float penetrationResult = Mathf.Max(penetrationValue - enemyCollider.armor, 0);
        // Pasamos en qué proporción ha penetrado
        if (penetrationResult > 0)
        {
            penetrationResult = 1 - (enemyCollider.armor / penetrationValue);
            //
            float kineticEnergy = GeneralFunctions.GetBodyKineticEnergy(impactingRigidbody);
            float damageReceived = kineticEnergy * penetrationResult;
            //
            if(damageReceived < 1) carolBaseHelp.TriggerGeneralAdvice("NoPenetration");
            //
            currentHealthPoints -= damageReceived;
        }
        else
        {
            carolBaseHelp.TriggerGeneralAdvice("NoPenetration");
        }
        
    }

    //
    public void ReceivePulseDamage(Vector3 forceAndDirection)
    {
        // Repetiemos un poco el codigo de bullet damage y a correr
        if (active == false && currentHealthPoints > 0)
        {
            Unveil();
            // TODO: Revisar esto
            carolBaseHelp.TriggerIt();
        }
        
        //
        if (currentHealthPoints > 0)
        {
            //
            float impactForce = forceAndDirection.magnitude;
            //
            float damageReceived = impactForce/* - defense*/;
            damageReceived = Mathf.Max(damageReceived, 0);
            //
            currentHealthPoints -= (int)damageReceived;
            //
            if (reactionOnDamage)
            {
                bossBehaviour.RespondToDamagedWeakPoint(tagForBoss);
            }
            //
            if (currentHealthPoints <= 0)
            {
                bossBehaviour.LoseWeakPoint(tagForBoss);
                carolBaseHelp.WeakPointDestroyed();
                //TODO: Meteremos el churrazo de sangre
                active = false;
                Instantiate(destructionParticles, transform.position, Quaternion.identity);
                gameObject.SetActive(false);
                //Destroy(this);
                //Destroy(gameObject);
                EnemyAnalyzer.Release();
            }
        }
    }

    public void Unveil()
    {
        //
        active = true;
        //
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = unveiledMaterial;
    }
}
