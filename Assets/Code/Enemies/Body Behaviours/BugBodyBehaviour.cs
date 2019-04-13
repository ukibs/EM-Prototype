﻿using System.Collections;
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

    //
    protected void OnCollisionEnter(Collision collision)
    {
        
    }

    #region Methods

    protected override void ExecuteCurrentAction(float dt)
    {
        // Las geenrales las chequeamos en el base
        base.ExecuteCurrentAction(dt);
        // Y aquí las específicas de bichos
        // Primero que el player siga vivo, si no mal
        if (player != null)
        {
            //
            Vector3 playerDirection = player.transform.position - transform.position;
            playerDirection.y = 0.0f;
            //
            switch (currentAction)
            {
                case Actions.Lunging:
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
                    Lunge();
                    break;
                case Actions.ZigZagingTowardsPlayer:
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
                    Move();
                    break;
            }

            // Damp para que no se desmadren
            //float dampForce = 10.0f;
            //rb.velocity = rb.velocity * ( 1 - dampForce * dt);
        }
    }

    /// <summary>
    /// Decide que acción del comportamiento utilizar
    /// </summary>
    protected override void DecideActionToDo()
    {
        //
        //base.DecideActionToDo();
        //
        //Debug.Log("Decding action");
        for (int i = 0; i < behaviour.Length; i++)
        {
            switch (behaviour[i])
            {
                case Actions.ZigZagingTowardsPlayer:
                    // Esta de momento sin condición
                    currentAction = behaviour[i];
                    return;
                case Actions.Lunging:
                    Vector3 playerDistance = player.transform.position - transform.position;
                    if (playerDistance.magnitude < minimalLungeDistance)
                    {
                        currentAction = behaviour[i];
                    }
                    return;
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
        //
        if (HasGroundUnderneath())
        {
            //
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
            //rb.velocity = movingDirection * maxSpeed * speedMultiplier;
            rb.AddForce(movingDirection * maxSpeed * speedMultiplier);
        }
        
    }

    //
    protected void Lunge()
    {
        //
        if (HasGroundUnderneath())
        {
            Debug.Log("Performing lunge");
            Vector3 playerDirection = (player.transform.position - transform.position).normalized;
            rb.AddForce(playerDirection * maxSpeed * 1, ForceMode.Impulse);
        }
    }

    // De momento usamos este aqui
    bool HasGroundUnderneath()
    {
        //
        //if (Physics.Raycast(transform.position, -transform.up, 2f))
        //{
        //    return true;
        //}
        // TODO: Declararlo public en generales
        Vector3 heightFromFloor = new Vector3(0, -0.55f, 0);
        Collider[] possibleFloor  = Physics.OverlapBox(transform.TransformPoint(heightFromFloor), new Vector3(1, 0.1f, 1));
        // TODO: Pillar el collider/s al princio
        Collider bodyCollider = GetComponent<Collider>();
        //
        for (int i = 0; i < possibleFloor.Length; i++)
        {
            if(possibleFloor[i] != bodyCollider)
                return true;
        }
        //
        return false;
    }

    // TODO: Hacerlo bien
    bool AnyOfItsColliders(Collider colliderToCheck)
    {
        Collider[] bodyColliders = null;

        for(int i = 0; i < bodyColliders.Length; i++)
        {
            return true;
        }

        return false;
    }

    #endregion
}