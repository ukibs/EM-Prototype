﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingGruntBodyBehaviour : EnemyBaseBodyBehaviour
{

    public float minIdealHeight = 30;
    public float maxIdealHeight = 60;
    public float liftForcePerSecond = 50;

    protected float idealHeight;

    // Start is called before the first frame update
    protected override void Start()
    {
        //
        idealHeight = UnityEngine.Random.Range(minIdealHeight, maxIdealHeight);
        // Ñapa
        transform.position = new Vector3(transform.position.x, idealHeight, transform.position.z);
        //
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        
        //
        base.Update();
        //
        if (!ofFoot)
            VerticalMovement();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        //
        rb.velocity += Vector3.up * maxSpeed;
    }

    //
    protected void VerticalMovement()
    {
        // Para controlar que se mantenga en la altura idónea
        if (transform.position.y < idealHeight)
        {
            //
            //float forceToAdd = liftForcePerSecond * (1 - (transform.position.y / currentIdealHeight));
            //float forceToApply = 
            //
            //rb.AddForce(Vector3.up * liftForcePerSecond);
            //
            float forceToApply = maxSpeed * (1 - (transform.position.y / idealHeight));
            rb.velocity += Vector3.up * forceToApply;
        }
        // Para evitar obstáculos
        //if(CheckIfObstacleInMovingDirection())
            //rb.velocity += Vector3.up * maxSpeed;

    }
}