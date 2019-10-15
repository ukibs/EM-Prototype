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
        //if (transform.position.y < idealHeight)
        //{
        //    rb.AddForce(Vector3.up * liftForcePerSecond);
        //}
        
        switch (currentAction)
        {
            // En estos casos queremos que mantengan su altura ideal
            // TODO: Decidir si con fuerzas o trampeando
            case Actions.GoingToPlayer:
            case Actions.EncirclingPlayerSideward:
            case Actions.EncirclingPlayerForward:
            case Actions.FacingPlayer:
                Vector3 currentPositon = transform.position;
                currentPositon.y = idealHeight;
                transform.position = currentPositon;
                break;
        }
        
    }
}
