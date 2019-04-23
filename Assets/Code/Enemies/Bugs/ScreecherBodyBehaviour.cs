using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreecherBodyBehaviour : BugBodyBehaviour
{
    public float idealLoadingHeight = 150;
    public float idealAttackHeight = 75;
    public float timeToChargeBall = 20;

    public Transform loadingBall;

    private float timeCharguingBall = 0;
    private float currentIdealHeight = 0;


    // Start is called before the first frame update
    protected override void Start()
    {
        // Ñapa
        transform.position = new Vector3(transform.position.x, idealLoadingHeight, transform.position.z);
        //
        base.Start();
        //
        currentIdealHeight = idealLoadingHeight;
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
        // Hardocdeamos el 2 que es la escala que usamos
        float ballScale = Mathf.Min((timeCharguingBall / timeToChargeBall) * 2, 2);
        loadingBall.transform.localScale = Vector3.one * ballScale;
        //
        if(transform.position.y < currentIdealHeight)
        {
            //
            float forceToAdd = maxSpeed * (1 - (transform.position.y / currentIdealHeight));
            //
            rb.AddForce(Vector3.up * maxSpeed, ForceMode.Impulse);
        }
            
        //
        Move();
    }

    // Ahora trabajaremos aqui los comporatmientos
    #region Methods

    protected override void DecideActionToDo()
    {
        //base.DecideActionToDo();
        //
        if (timeCharguingBall >= timeToChargeBall)
        {
            //weapons[0].Shoot(Time.deltaTime);
            currentAction = Actions.GoingToPlayer;
            //distan
            currentIdealHeight = idealAttackHeight;
        }
        else
        {
            currentAction = Actions.EncirclingPlayerForward;
            currentIdealHeight = idealLoadingHeight;
        }
    }

    //
    protected override void ExecuteCurrentAction(float dt)
    {
        base.ExecuteCurrentAction(dt);
        // Disparamos el arma principal del screecher
        if (timeCharguingBall >= timeToChargeBall)
        {
            Vector3 playerDistance = player.transform.position - transform.position;
            if (playerDistance.magnitude < minimalShootDistance)
            {
                weapons[0].Shoot(Time.deltaTime);
                timeCharguingBall = 0;
            }
                
        }
        // En ambos casos
        //Move();
    }

    protected override void Move()
    {
        //base.Move();
        //rb.velocity = transform.forward * maxSpeed * Time.deltaTime;
        rb.AddForce(transform.forward * maxSpeed);
    }

    #endregion
}
