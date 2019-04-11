using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Actions
{
    Invalid = -1,

    GoingToPlayer,
    EncirclingPlayer,
    FacingPlayer,
    Fleeing,
    ZigZagingTowardsPlayer,
    Lunging,

    Count
}

public class EnemyBaseBodyBehaviour : MonoBehaviour
{

    public float timeBetweenActionChecking = 1.0f;
    
    [Tooltip("Rotatin in degrees per second.")]
    public float rotationSpeed = 90;
    
    public Actions[] behaviour;     // Luego trabajaremos bien esto

    protected RobotControl player;
    protected Rigidbody rb;

    protected Actions currentAction = Actions.GoingToPlayer;
    protected float timeFromLastCheck = 0;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        player = FindObjectOfType<RobotControl>();
        rb = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //
        float dt = Time.deltaTime;
        // Que el player siga vivo
        if(player != null)
        {
            //
            timeFromLastCheck += dt;
            // Si huyen qe no chequeen mas acciones
            if (timeFromLastCheck > timeBetweenActionChecking && currentAction != Actions.Fleeing)
            {
                CheckActionToDo();
                DecideActionToDo();
                timeFromLastCheck -= timeBetweenActionChecking;
            }
        }
        
        //
        ExecuteCurrentAction(dt);
        
    }

    protected void OnDrawGizmos()
    {
        //
        if (player != null)
        {
            Debug.DrawRay(transform.position, rb.velocity, Color.blue);
            //Vector3 playerDirection = player.transform.position - transform.position;
            //Debug.DrawRay(transform.position, playerDirection, Color.red);
        }

    }

    //
    protected virtual void Move()
    {

    }

    //
    protected virtual void GiveItGas()
    {
        
    }

    protected virtual void ExecuteCurrentAction(float dt)
    {
        // Primero que el player siga vivo, si no mal
        if (player != null)
        {
            //
            Vector3 playerDirection = player.transform.position - transform.position;
            playerDirection.y = 0.0f;
            //
            switch (currentAction)
            {
                case Actions.FacingPlayer:
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
                    break;
                case Actions.GoingToPlayer:
                    //transform.rotation = Quaternion.LookRotation(playerDirection);
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
                    Move();
                    break;
                case Actions.EncirclingPlayer:
                    Vector3 playerCross = Vector3.Cross(playerDirection, Vector3.up);
                    //transform.rotation = Quaternion.LookRotation(playerCross);
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, playerCross, rotationSpeed, dt);
                    Move();
                    break;
                case Actions.Fleeing:
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, -playerDirection, rotationSpeed, dt);
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
    protected virtual void DecideActionToDo()
    {
        
    }

    /// <summary>
    /// Ejecutar la acción elegida
    /// </summary>
    protected virtual void CheckActionToDo()
    {
        // De momento aqui snecillo, luego nos curramos más la IA
        float distanceToPlayer = (transform.position - player.transform.position).magnitude; // Ya veremos si hacemos sqrt magnitude para ahorrar
        
    }
}
