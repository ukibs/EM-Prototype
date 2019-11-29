using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    //
    public Transform previousBodyPart;
    public BossBaseBehaviour bossBehaviour;
    public BigWormBodyBehaviour bigWormBehaviour;
    

    //
    private float previousBodyPartOriginalDistance;
    private float speedToUse;
    private Rigidbody bigWormRb;


    // Start is called before the first frame update
    void Start()
    {
        previousBodyPartOriginalDistance = (previousBodyPart.position - transform.position).magnitude;
        //gigaWormBehaviour = GetComponentInParent<GigaWormBehaviour>();
        //
        if (bigWormBehaviour != null)
        {
            //bigWormRb = bigWormBehaviour.GetComponent<Rigidbody>();
            speedToUse = bigWormBehaviour.movementSpeed;
            transform.position = new Vector3(transform.position.x, bigWormBehaviour.underSandIdealHeight, transform.position.z);
        }
        //
        if(bossBehaviour != null)
        {
            speedToUse = bossBehaviour.CurrentSpeed;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        UpdateSpeedToUse();
        //
        transform.LookAt(previousBodyPart);
        // TODO: Hacerlo adaptable
        float currentDistance = (previousBodyPart.position - transform.position).magnitude;
        if(currentDistance > previousBodyPartOriginalDistance)
            transform.Translate(Vector3.forward * speedToUse * dt);
        // Para cuando la parte del cuerpo se queda atrás
        if(currentDistance > previousBodyPartOriginalDistance * 1.2f)
            transform.Translate(Vector3.forward * speedToUse * dt * 1.5f);
    }

    //
    void UpdateSpeedToUse()
    {
        // TODO: Trabajar bien la velocidad del gusano
        if (bigWormBehaviour != null) speedToUse = bigWormBehaviour.headRb.velocity.magnitude;
        if (bossBehaviour != null) {
            //
            if(speedToUse != bossBehaviour.CurrentSpeed)
            {
                //Debug.Log("Old speed: " + speedToUse + ", new speed: " + bossBehaviour.CurrentSpeed);
                //
                speedToUse = bossBehaviour.CurrentSpeed;                
            }
        }
    }
}
