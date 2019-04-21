using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormBodyBehaviour : BugBodyBehaviour
{
    //
    //public float groundingSpeed = 0.5f;
    public float lungeSpeed;
    //
    //protected float groundedLevel = 1;
    protected bool grounded = false;
    protected BoxCollider bodyCollider;
    protected MeshRenderer meshRenderer;
    protected bool lunging = false;

    //
    protected override void Start()
    {
        //
        base.Start();
        //
        bodyCollider = GetComponent<BoxCollider>();
        //
        SwitchGrounding();
        //
        if (HasGroundUnderneath() && !grounded)
        {
            grounded = false;
            SwitchGrounding();
        }
            
    }
    //
    protected override void Update()
    {
        base.Update();

    }
    //
    private void OnCollisionStay(Collision collision)
    {
        if(collision.collider.tag.Equals("Sand") && !lunging && !HasGroundUnderneath())
        {
            grounded = false;
            SwitchGrounding();
        }
    }
    //
    protected override void OnCollisionEnter(Collision collision)
    {
        //
        base.OnCollisionEnter(collision);
        //
        if (collision.collider.tag == "Sand" && !grounded)
        {
            lunging = false;
            SwitchGrounding();
        }
        // TODO: Montarlo bien y asegurarse de que funciona
        if(collision.collider.tag.Equals("Hard Terrain") && grounded)
        {
            Destroy(gameObject);
        }
    }

    //
    //protected void UpdateGrounding()

    //
    protected void SwitchGrounding()
    {
        if (!grounded)
        {
            grounded = true;
            bodyCollider.size = new Vector3(1, 0.1f, 1);
            bodyCollider.center = new Vector3(0, 0.5f, 0);
            bodyConsistency.centralPointOffset = new Vector3(0, 0.45f, 0);
            transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            grounded = false;
            bodyCollider.size = Vector3.one;
            bodyCollider.center = Vector3.zero;
            bodyConsistency.centralPointOffset = Vector3.zero;
        }
    }

    //protected override void ExecuteCurrentAction(float dt)
    //{
    //    // Las geenrales las chequeamos en el base
    //    base.ExecuteCurrentAction(dt);
    //    // Y aquí las específicas de bichos
    //    // Primero que el player siga vivo, si no mal
    //    if (player != null)
    //    {
    //        switch (currentAction)
    //        {
    //            case Actions.Lunging:
    //                transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
    //                Lunge();
    //                break;
    //        }
    //    }
    //}

    //
    protected new void Lunge()
    {
        //
        if (HasGroundUnderneath() && onFloor && !lunging)
        {
            Debug.Log("Performing lunge");
            Vector3 playerDirection = (player.transform.position - transform.position).normalized;
            rb.AddForce(playerDirection * lungeSpeed, ForceMode.Impulse);
            onFloor = false;
            lunging = true;
            SwitchGrounding();
        }
    }
}
