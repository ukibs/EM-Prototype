using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    //
    public Transform previousBodyPart;
    public GigaWormBehaviour gigaWormBehaviour;

    //
    private float previousBodyPartOriginalDistance;
    

    // Start is called before the first frame update
    void Start()
    {
        previousBodyPartOriginalDistance = (previousBodyPart.position - transform.position).magnitude;
        //gigaWormBehaviour = GetComponentInParent<GigaWormBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        transform.LookAt(previousBodyPart);
        // TODO: Hacerlo adaptable
        float currentDistance = (previousBodyPart.position - transform.position).magnitude;
        if(currentDistance > previousBodyPartOriginalDistance)
            transform.Translate(Vector3.forward * gigaWormBehaviour.CurrentSpeed * dt);
        // Para cuando la parte del cuerpo se queda atrás
        if(currentDistance > previousBodyPartOriginalDistance * 1.2f)
            transform.Translate(Vector3.forward * gigaWormBehaviour.CurrentSpeed * dt * 1.5f);
    }
}
