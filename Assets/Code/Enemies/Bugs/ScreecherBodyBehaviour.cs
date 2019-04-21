using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreecherBodyBehaviour : BugBodyBehaviour
{
    public float idealHeight = 150;
    public float timeToChargeBall = 20;
    public Transform loadingBall;

    private float timeCharguingBall = 0;


    // Start is called before the first frame update
    protected override void Start()
    {
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
        float dt = Time.deltaTime;
        //
        timeCharguingBall += dt;
        //
        if(transform.position.y < idealHeight)
            rb.AddForce(Vector3.up * maxSpeed, ForceMode.Impulse);
        //
        Move();
    }

    // Ahora trabajaremos aqui los comporatmientos
    #region Methods

    //protected override void DecideActionToDo()
    //{
    //    base.DecideActionToDo();
    //}

    //protected override void ExecuteCurrentAction(float dt)
    //{
    //    base.ExecuteCurrentAction(dt);
    //}

    protected override void Move()
    {
        //base.Move();
        //rb.velocity = transform.forward * maxSpeed * Time.deltaTime;
        rb.AddForce(transform.forward * maxSpeed);
    }

    #endregion
}
