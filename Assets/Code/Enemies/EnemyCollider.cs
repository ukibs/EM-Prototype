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
    public int locationHealth;

    // TODO: Manejar dureza de material
    // Y otras propiedades en el futuro

    private EnemyConsistency body;
    //private Rigidbody bodyRb;
    private Color[] originalAMRColors;

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

        //
        GetOriginalColors();
        // Testeo
        //SetPenetrationColors();
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
