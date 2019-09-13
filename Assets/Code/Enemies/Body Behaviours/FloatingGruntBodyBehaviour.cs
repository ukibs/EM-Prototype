using System.Collections;
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
        //
        float floatingMarging = 10;
        // Para controlar que se mantenga en la altura idónea
        //if (transform.position.y < idealHeight - floatingMarging)
        //{
        //    //
        //    float distanceFromIdeal = idealHeight - transform.position.y;
        //    float offsetCompensation = Mathf.Pow(distanceFromIdeal / floatingMarging, 3);
        //    //
        //    Vector3 verticalSpeed = Vector3.up * rb.velocity.y;
        //    //
        //    rb.AddForce(Vector3.up * offsetCompensation * liftForcePerSecond - verticalSpeed/2);
        //}
        //else 
        if(transform.position.y < idealHeight)
        {
            rb.AddForce(Vector3.up * liftForcePerSecond);
        }
        else
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        }
        // Para evitar obstáculos
        //if(CheckIfObstacleInMovingDirection())
            //rb.velocity += Vector3.up * maxSpeed;

    }
}
