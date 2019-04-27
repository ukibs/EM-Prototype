using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollManualAdjustment : MonoBehaviour
{
    //
    public Vector3 gravityCenter;
    //
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = gravityCenter;
    }
    
}
