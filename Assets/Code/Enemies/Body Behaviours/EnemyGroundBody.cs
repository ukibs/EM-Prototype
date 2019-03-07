using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyGroundBody : EnemyBodyBehaviour
{
    
    private bool touchingSomething;


    protected override void GiveItGas()
    {
        // Y movemos con el rigidvody
        if (touchingSomething && HasGroundUnderneath())
            rb.AddForce(transform.forward * motorForce, ForceMode.Impulse);
        // Lo negamos hasta el próximo chqueo de colisón
        touchingSomething = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        touchingSomething = true;
    }

    #region Methods

    bool HasGroundUnderneath()
    {
        //
        if(Physics.Raycast(transform.position, -transform.up, 2f))
        {
            return true;
        }
        //
        return false;
    }
    

    #endregion
}
