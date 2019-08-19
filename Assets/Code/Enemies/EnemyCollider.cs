using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollider : MonoBehaviour
{
    //
    public enum AdditionalEffectOnDamage
    {
        Invalid = -1,

        None,
        MovementCrippling,
        Stun,

        Count
    }

    [Tooltip("Armor thickness on this side")]
    public float armor = 10;
    //
    public MeshRenderer[] associatedMeshRenderers;
    //
    public bool isTargeteable;
    [Tooltip("Only used when the collider is lockable")]
    public int maxLocationHealth;
    public AdditionalEffectOnDamage onDamage = AdditionalEffectOnDamage.None;

    // TODO: Manejar dureza de material
    // Y otras propiedades en el futuro

    private EnemyConsistency body;
    private EnemyBaseBodyBehaviour bodyBehaviour;
    private Color[] originalAMRColors;
    private int currentLocationHealth;

    public float Armor {
        get { return armor; }
        set { armor = value; }
    }

    public float CurrentLocationHealth { get { return currentLocationHealth; } }

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
        // Intento extra para gusano garnde
        if(body == null)
        {
            body = transform.parent.GetComponentInChildren<EnemyConsistency>();
        }
        //
        bodyBehaviour = body.GetComponent<EnemyBaseBodyBehaviour>();

        //
        //bodyRb = body.GetComponent<Rigidbody>();

        //
        GetOriginalColors();
        // Si no hay efecto adicional asignado no tiene sentido poner vida aparte
        if (onDamage != AdditionalEffectOnDamage.None)
            currentLocationHealth = maxLocationHealth;
    }
    
    // 
    void GetOriginalColors()
    {
        //
        originalAMRColors = new Color[associatedMeshRenderers.Length];
        //
        for (int i = 0; i < associatedMeshRenderers.Length; i++)
        {
            originalAMRColors[i] = associatedMeshRenderers[i].material.color;
        }
    }

    //
    public void SetAffectableByPulseColors()
    {
        // TODO: Cuando lo tengamos claro aplicar
    }

    //
    public void SetPenetrationColors()
    {
        //
        float playerEstimatedPenetration = PlayerReference.GetCurrentWeaponPenetrationEstimation();
        float penetrationEstimatedResult = playerEstimatedPenetration - armor;
        float damageEffectivenes = 1 - (armor / playerEstimatedPenetration);
        //float penetrationAchieved = 1 - (bodyPart.armor / penetrationValue);
        //
        Color colorToUse;
        //if (penetrationEstimatedResult > 10)
        //    colorToUse = Color.green;
        //else if (penetrationEstimatedResult > 0)
        //    colorToUse = Color.yellow;
        //else
        //    colorToUse = Color.red;

        //
        if (penetrationEstimatedResult < 1)
            colorToUse = Color.red;
        else
            colorToUse = Color.Lerp(Color.yellow, Color.green, damageEffectivenes);

        //
        for (int i = 0; i < associatedMeshRenderers.Length; i++)
        {
            associatedMeshRenderers[i].material.color = colorToUse;
        }
    }

    //
    public void SetOriginalColors()
    {
        //
        for (int i = 0; i < associatedMeshRenderers.Length; i++)
        {
            associatedMeshRenderers[i].material.color = originalAMRColors[i];
        }
    }

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
            if (onDamage != AdditionalEffectOnDamage.None)
                ManageBodyPartDamage(penetrationResult, bulletRb);
            //
            body.ReceiveProyectileImpact(penetrationResult, impactPoint, bulletRb);
        }
        
    }

    // Para recibir daño del ataque de pulso en la parte del cuerpo
    public void ReceivePulseDamage(Vector3 directionWithForce)
    {
        //
        //Debug.Log("Receiving pulse damage with " + directionWithForce + " force");
        //
        float impactForce = directionWithForce.magnitude;
        //
        float damageReceived = impactForce - body.defense;
        damageReceived = Mathf.Max(damageReceived, 0);
        //
        currentLocationHealth -= (int)damageReceived;
        // TODO: Revisar cómo lo gestionamos
        // Depende del efecto se gestionará de una forma u otra
        switch (onDamage)
        {
            case AdditionalEffectOnDamage.MovementCrippling:
                if (currentLocationHealth < 0
                    && currentLocationHealth + damageReceived > 0) // Chequeo rapido para que solo se aplique una vez
                {
                    bodyBehaviour.MovementStatus -= 0.25f; // De momento hardcodeamos teniendo en cuenta 4 patas
                    body.RemoveTargeteablePart(this);
                }
                break;
            case AdditionalEffectOnDamage.Stun:
                // Este lo dejamos aparcado de momento
                break;
        }
    }

    //
    void ManageBodyPartDamage(float penetrationResult, Rigidbody bulletRb)
    {
        //
        int damageReceived = (int)(GeneralFunctions.GetBodyKineticEnergy(bulletRb) * penetrationResult);
        currentLocationHealth -= damageReceived;
        // Depende del efecto se gestionará de una forma u otra
        switch (onDamage)
        {
            case AdditionalEffectOnDamage.MovementCrippling:
                if (currentLocationHealth < 0
                    && currentLocationHealth + damageReceived > 0) // Chequeo rapido para que solo se aplique una vez
                {                           
                    bodyBehaviour.MovementStatus -= 0.25f; // De momento hardcodeamos teniendo en cuenta 4 patas
                    body.RemoveTargeteablePart(this);
                }
                break;
            case AdditionalEffectOnDamage.Stun:
                // Este lo dejamos aparcado de momento
                break;
        }
    }

    //
    public void ResetStatus()
    {
        if (isTargeteable)
        {
            currentLocationHealth = maxLocationHealth;

        }
    }
}
