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
    public float minimalShootDistance = 100;
    public float ofFootMaxTime = 5;

    // Esto para los que hagan zig zag
    protected float currentZigZagDirection = 0;
    protected float currentZigZagVariation = 0.1f;

    //Varaible para determinar si ha paerido el equilibrio
    protected bool ofFoot = true;
    protected float ofFootCurrentTime = 0;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        // TODO: Que el offoot sea directamente aplicado por acciones del player

        if (bodyConsistency.ReceivedStrongImpact)
        {
            //
            //Debug.Log(gameObject.name + " set off foot");
            //
            //ofFoot = true;
            //ofFootCurrentTime = 0;
        }
        //
        base.Update();
        
    }

    protected override void OnDrawGizmos()
    {
        //base.OnDrawGizmos();
        //
        Color colorToUse = Color.red;
        //
        if(pathToUse != null)
        {
            for(int i = 0; i < pathToUse.Count - 1; i++)
            {
                colorToUse.r -= 0.1f;
                colorToUse.b += 0.1f;
                Gizmos.color = colorToUse;
                Gizmos.DrawLine(pathToUse[i].transform.position, pathToUse[i + 1].transform.position);
            }
        }
    }

    #region Methods

    protected override void ExecuteCurrentAction(float dt)
    {
        // Avanzamos el chequeo de desequilibrio y si está desequilibrado que no pueda actuar
        if (ofFoot)
        {
            ofFootCurrentTime += dt;
            if (ofFootCurrentTime >= ofFootMaxTime)
            {
                ofFootCurrentTime = 0;
                ofFoot = false;
            }
            else
                return;
        }
        // Las geenrales las chequeamos en el base
        base.ExecuteCurrentAction(dt);
        // Y aquí las específicas de bichos
        // Primero que el player siga vivo, si no mal
        //if (player != null)
        //{
        //    //
        //    Vector3 playerDirection = player.transform.position - transform.position;
        //    playerDirection.y = 0.0f;
        //    //
        //    switch (currentAction)
        //    {
        //        //case Actions.Lunging:
        //        //    //transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
        //        //    Lunge();
        //        //    break;
        //        case Actions.ZigZagingTowardsPlayer:
        //            if (HasGroundUnderneath())
        //            {
        //                transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
        //                Move();
        //            }
        //            break;
        //    }

        //    // Damp para que no se desmadren
        //    //float dampForce = 10.0f;
        //    //rb.velocity = rb.velocity * ( 1 - dampForce * dt);
        //}
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
        Vector3 playerDistance = player.transform.position - transform.position;
        for (int i = 0; i < behaviour.Length; i++)
        {
            switch (behaviour[i])
            {
                case Actions.FacingPlayer:
                    if (playerDistance.magnitude < minimalShootDistance)
                    {
                        currentAction = behaviour[i];
                        return;
                    }
                    break;
                case Actions.GoingToPlayer:

                    // Esta no lleva condición
                    currentAction = behaviour[i];
                    //
                    pathToUse = terrainManager.GetPathToPlayer(transform);
                    //TODO: Meter aqui el A*
                    return;
                case Actions.EncirclingPlayerSideward:
                    if (playerDistance.magnitude < minimalShootDistance)
                    {
                        currentAction = behaviour[i];
                        return;
                    }                    
                    break;
                case Actions.EncirclingPlayerForward:
                    if (playerDistance.magnitude < minimalShootDistance)
                    {
                        currentAction = behaviour[i];
                        return;
                    }
                    break;
                case Actions.RetreatingFromPlayer:
                    if (playerDistance.magnitude < minimalShootDistance)
                    {
                        currentAction = behaviour[i];
                        return;
                    }
                    break;
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
            //
            float speedMultiplier = 1;
            //
            switch (currentAction)
            {
                case Actions.EncirclingPlayerForward:
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
                case Actions.EncirclingPlayerSideward:
                    movingDirection = transform.right;
                    speedMultiplier = 0.2f;
                    break;
                case Actions.RetreatingFromPlayer:
                    movingDirection = -transform.forward;
                    speedMultiplier = 1f;
                    break;
            }
            //
            rb.velocity = (movingDirection * maxSpeed * speedMultiplier * movementStatus);
            //rb.AddForce(movingDirection * maxSpeed * speedMultiplier);
            //
            if (!onFloor)
                rb.velocity += Physics.gravity;
        }
        
    }

    // NOTA: Este no es el de los gusanos
    protected void Lunge()
    {
        //
        if (HasGroundUnderneath() && onFloor)
        {
            //Debug.Log("Performing lunge");
            Vector3 playerDirection = (player.transform.position - transform.position).normalized;
            //rb.AddForce(playerDirection * maxSpeed * 5, ForceMode.Impulse);
            rb.velocity = playerDirection * maxSpeed * 5;
            onFloor = false;
        }
    }

    // De momento usamos este aqui
    protected bool HasGroundUnderneath()
    {
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
    //bool AnyOfItsColliders(Collider colliderToCheck)
    //{
    //    Collider[] bodyColliders = null;

    //    for(int i = 0; i < bodyColliders.Length; i++)
    //    {
    //        return true;
    //    }

    //    return false;
    //}

    // TODO: Mover a base
    public void LoseFoot()
    {
        ofFoot = true;
        ofFootCurrentTime = 0;
    }

    #endregion
}
