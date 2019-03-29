using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    //
    public float propulsionForce;
    //
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.Log(gameObject.name + "should have the Rigidbody component");
    }

    // Update is called once per frame
    void Update()
    {
        if(rb != null)
        {
            float dt = Time.deltaTime;
            rb.AddForce(transform.forward * propulsionForce * dt, ForceMode.Impulse);
        }
    }
}
