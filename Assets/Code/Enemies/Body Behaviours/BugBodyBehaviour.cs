using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Comportamiendo de bicho
/// De momento hacemos todo aqui
/// Aunque probablemente haya que subdividirlo cuando metamos voladores
/// </summary>
public class BugBodyBehaviour : EnemyBaseBodyBehaviour
{
    public float maxSpeed = 10;
    public float minimalLungeDistance = 15;

    // Esto para los que hagan zig zag
    private float currentZigZagDirection = 0;
    private int currentZigZagVariation = 1;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

    }

    #region Methods

    //protected override void ExecuteCurrentAction(float dt)
    //{
    //    // Primero que el player siga vivo, si no mal
    //    if (player != null)
    //    {
    //        //
    //        Vector3 playerDirection = player.transform.position - transform.position;
    //        playerDirection.y = 0.0f;
    //        //
    //        switch (currentAction)
    //        {
    //            case Actions.FacingPlayer:
    //                transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
    //                break;
    //            case Actions.GoingToPlayer:
    //                //transform.rotation = Quaternion.LookRotation(playerDirection);
    //                transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
    //                GiveItGas();
    //                break;
    //            case Actions.EncirclingPlayer:
    //                Vector3 playerCross = Vector3.Cross(playerDirection, Vector3.up);
    //                //transform.rotation = Quaternion.LookRotation(playerCross);
    //                transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, playerCross, rotationSpeed, dt);
    //                GiveItGas();
    //                break;
    //            case Actions.Fleeing:
    //                transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, -playerDirection, rotationSpeed, dt);
    //                GiveItGas();
    //                break;
    //        }

    //        // Damp para que no se desmadren
    //        //float dampForce = 10.0f;
    //        //rb.velocity = rb.velocity * ( 1 - dampForce * dt);
    //    }
    //}

    /// <summary>
    /// Decide que acción del comportamiento utilizar
    /// </summary>
    protected override void DecideActionToDo()
    {
        //
        for (int i = 0; i < behaviour.Length; i++)
        {
            switch (behaviour[i])
            {
                case Actions.ZigZagingTowardsPlayer:
                    // Esta de momento sin condición
                    currentAction = behaviour[i];
                    break;
                case Actions.Lunging:
                    Vector3 playerDistance = player.transform.position - transform.position;
                    if (playerDistance.magnitude > minimalLungeDistance)
                    {
                        currentAction = behaviour[i];
                    }
                    break;
                case Actions.EncirclingPlayer:

                    break;
                case Actions.FacingPlayer:

                    break;
                case Actions.GoingToPlayer:
                    // Esta no lleva condición
                    currentAction = behaviour[i];
                    return;
            }
        }
    }

    //
    protected override void Move()
    {
        Vector3 movingDirection = transform.forward;
        float speedMultiplier = 1;
        switch (currentAction)
        {
            case Actions.GoingToPlayer:
                // Aqui nada de momento porque ya es forward por defecto
                break;
            case Actions.ZigZagingTowardsPlayer:
                currentZigZagDirection += currentZigZagVariation * Time.deltaTime;
                if (Mathf.Abs(currentZigZagDirection) >= 1)
                {
                    currentZigZagVariation *= -1;
                }
                movingDirection += transform.right * currentZigZagDirection;
                movingDirection = movingDirection.normalized;
                break;

        }
        rb.velocity = movingDirection * maxSpeed * speedMultiplier;
        //
        if(currentAction == Actions.Lunging && HasGroundUnderneath())
        {
            Vector3 playerDirection = (player.transform.position - transform.position).normalized;
            rb.AddForce(playerDirection * maxSpeed * 2, ForceMode.Impulse);
        }
    }

    // De momento usamos este aqui
    bool HasGroundUnderneath()
    {
        //
        if (Physics.Raycast(transform.position, -transform.up, 2f))
        {
            return true;
        }
        //
        return false;
    }

    #endregion
}
