using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Comportamiento de misil para prouectiles
/// </summary>
public class Missile : MonoBehaviour
{
    //
    public float propulsionForce;
    public float propulsionSpeed;
    public bool seeksObjective;
    public float rotationSpeed;
    //
    private Rigidbody rb;
    private Transform currentObjective;

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
        //
        float dt = Time.deltaTime;
        //
        if(rb != null)
        {
            //rb.AddForce(transform.forward * propulsionForce * dt, ForceMode.Impulse);
            rb.velocity = transform.forward * propulsionSpeed;
            //Debug.Log("Missile velocity :" + rb.velocity);
        }
        //
        if (seeksObjective && currentObjective != null)
        {
            transform.rotation = GeneralFunctions.UpdateRotation(transform, currentObjective.position, rotationSpeed, dt);
        }
        else
        {
            Debug.Log("Objective should be assigned");
        }
    }

    //
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawLine(transform.position, currentObjective.position);
    //}

    //
    public void AssignObjective(Transform objective)
    {
        currentObjective = objective;
    }
}
