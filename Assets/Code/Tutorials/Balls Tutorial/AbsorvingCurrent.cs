using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbsorvingCurrent : MonoBehaviour
{
    public float absortionForce = 500;
    public float clampingEffect = 30;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        MurderBall murderBall = other.GetComponent<MurderBall>();
        if(murderBall != null)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            //
            Vector3 centerOffset = new Vector3(transform.position.x - other.transform.position.x, 0.0f,
                                                transform.position.z - other.transform.position.z).normalized / 10.0f;
            //
            rb.velocity *= (1 - clampingEffect * Time.deltaTime);
            //
            Vector3 forceDirection = centerOffset + Vector3.up;
            rb.AddForce(forceDirection * absortionForce, ForceMode.Impulse);
        }
        
    }
}
