using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyBodyBehaviour : EnemyBaseBodyBehaviour
{
    //
    public float idealHeight;

    public float liftForcePerSecond;

    protected override void Start()
    {
        // Ñapa
        transform.position = new Vector3(transform.position.x, idealHeight, transform.position.z);
        //
        base.Start();
    }

    //
    protected override void Update()
    {
        //
        VerticalMovement();
        //
        base.Update();
    }

    protected void VerticalMovement()
    {
        if (transform.position.y < idealHeight)
        {
            rb.AddForce(Vector3.up * liftForcePerSecond);
        }
    }
}
