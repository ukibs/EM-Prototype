using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Actions
{
    Invalid = -1,

    GoingToPlayer,
    EncirclingPlayerForward,
    EncirclingPlayerSideward,
    FacingPlayer,
    Fleeing,
    ZigZagingTowardsPlayer,
    Lunging,
    RetreatingFromPlayer,

    Count
}

public class EnemyBaseBodyBehaviour : MonoBehaviour
{

    public float timeBetweenActionChecking = 1.0f;
    
    [Tooltip("Rotatin in degrees per second.")]
    public float rotationSpeed = 90;
    
    public Actions[] behaviour;     // Luego trabajaremos bien esto

    public EnemyWeapon[] weapons;   // TODO: Que la busque él

    protected RobotControl player;
    protected Rigidbody rb;

    protected Actions currentAction = Actions.GoingToPlayer;
    protected float timeFromLastCheck = 0;

    protected EnemyConsistency bodyConsistency;
    protected TerrainManager terrainManager;
    protected AudioSource audioSource;

    // De momento lo hacemos con posiciones
    protected List<Waypoint> pathToUse;
    protected List<Vector3> pathToPlayer;

    //
    protected bool onFloor = true;
    

    // Start is called before the first frame update
    protected virtual void Start()
    {
        player = FindObjectOfType<RobotControl>();
        rb = GetComponent<Rigidbody>();
        bodyConsistency = GetComponent<EnemyConsistency>();
        terrainManager = FindObjectOfType<TerrainManager>();
        audioSource = GetComponent<AudioSource>();
    }

    protected virtual void FixedUpdate()
    {
        // Lo ponemos aqui a false
        // Si está en contacto con terreno se pondrá a true en el collision enter/stay
        onFloor = false;
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

            //
            ExecuteCurrentAction(dt);
        }        
        
    }

    //
    protected virtual void OnCollisionEnter(Collision collision)
    {
        //
        if (collision.collider.tag == "Sand" || collision.collider.tag == "Hard Terrain")
            onFloor = true;
    }

    //
    protected virtual void OnCollisionStay(Collision collision)
    {
        //
        if (collision.collider.tag == "Sand" || collision.collider.tag == "Hard Terrain")
            onFloor = true;
    }

    protected virtual void OnDrawGizmos()
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

    //
    protected void GetPathToPlayer()
    {

    }

    protected virtual void ExecuteCurrentAction(float dt)
    {
        // Primero que el player siga vivo, si no mal
        if (player != null)
        {
            //
            Vector3 playerDirection = player.transform.position - transform.position;
            //playerDirection.y = 0.0f;
            
            //
            switch (currentAction)
            {
                case Actions.FacingPlayer:
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
                    break;
                case Actions.GoingToPlayer:
                    // TODO: Cuidado con cuando se cambian los paneles. De momento no es un gran problema, pero habrá que abordarlo
                    //
                    Vector3 currentObjective;
                    //
                    if (pathToUse != null && pathToUse.Count > 0)
                    {
                        // Si estamos lo bastante cerca del punto que toca lo descartamos
                        if ((pathToUse[0].transform.position - transform.position).magnitude < 50)
                        {
                            pathToUse.RemoveAt(0);
                            //
                            if (pathToUse.Count == 0)
                            {
                                DecideActionToDo();
                                return;
                            }
                                
                            //
                            //Debug.Log("Next waypoint: " + pathToUse[0]);
                        }


                        // Ahora el objetivo lo sacaremos con el path
                        currentObjective = pathToUse[0].transform.position;

                        //
                        transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, currentObjective, rotationSpeed, dt);
                        Move();
                    }
                    else
                    {
                        //
                        transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
                        Move();
                    }
                    break;
                case Actions.EncirclingPlayerForward:
                    Vector3 playerCross = Vector3.Cross(playerDirection, Vector3.up);
                    //transform.rotation = Quaternion.LookRotation(playerCross);
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, playerCross, rotationSpeed, dt);
                    Move();
                    break;
                case Actions.EncirclingPlayerSideward:
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
                    Move();
                    break;
                case Actions.Fleeing:
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, -player.transform.position, rotationSpeed, dt);
                    Move();
                    break;
                case Actions.RetreatingFromPlayer:
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

    /// <summary>
    /// Cheque para ver si el jugador está a la vista
    /// Para determinar si usar waypoints o no
    /// </summary>
    /// <returns></returns>
    protected bool PlayerOnSight()
    {
        return false;
    }
}
