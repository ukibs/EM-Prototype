using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WormStatus
{
    Invalid = -1,

    Wandering,
    Chasing,
    Stunned,

    Count
}

public class GigaWormBehaviour : Targeteable
{

    public float wanderingMovementSpeed = 10;
    public float chasingMovementSpeed = 10;
    public float rotationSpeed = 30;

    public float timeUnderground = 10;
    public float overGroundHeight = 0;
    public float underGroundHeight = -20;
    public float heightChangeSpeed = 5;

    //
    public int exteriorWeakPoints;
    public int interiorWeakPoints;

    private WormStatus currentState = WormStatus.Wandering;
    private RobotControl player;
    private Rigidbody rb;

    //
    private float currentTimeUnderground;
    private bool goesUnderground = false;

    

    // TODO: En colisiones entre cuerpos
    // Tratar la inclinación de los cuerpos
    // No es lo mismo un choque perpedicular que uno más directo
    // Seno?

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<RobotControl>();
        Debug.Log("Player found? " + player);
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //
        if (player == null)
            return;
        //
        float dt = Time.deltaTime;
        //
        Vector3 playerDirection = player.transform.position - transform.position;
        //
        switch (currentState)
        {
            // Cuando está vagando por ahí
            case WormStatus.Wandering:
                
                float playerDistance = (transform.position - player.transform.position).magnitude;
                
                // Que rote alrededor del player si se aleja demasiado
                // Así no se va a cuenca
                if(playerDistance > 700)
                {
                    // Sacar la cruz
                    Vector3 playerCross = Vector3.Cross(playerDirection, Vector3.up);
                    //transform.rotation = Quaternion.LookRotation(playerCross);
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, playerCross, rotationSpeed, dt);
                }
                
                // Velocity no sirve con kinematicos
                //rb.velocity = transform.forward * 100;
                //
                transform.Translate(Vector3.forward * wanderingMovementSpeed * dt);
                //Debug.Log("I'm wandering");

                //
                if (goesUnderground) 
                {
                    //
                    if(transform.position.y > underGroundHeight)
                        transform.Translate(Vector3.up * heightChangeSpeed * dt * -1);
                    //
                    currentTimeUnderground += dt;
                    if (currentTimeUnderground >= timeUnderground)
                    {
                        goesUnderground = false;
                        
                    }
                        
                }
                else if(!goesUnderground && transform.position.y < overGroundHeight)
                {
                    transform.Translate(Vector3.up * heightChangeSpeed * dt);
                }
                else if (exteriorWeakPoints == 0)
                {
                    currentState = WormStatus.Chasing;
                    active = true;
                    rotationSpeed *= 3;
                }
                else
                {
                    // Del targeteable, hacer variable más clara
                    active = true;
                }
                break;
            case WormStatus.Chasing:
                // TODO: Que persiga al player
                transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, playerDirection, rotationSpeed, dt);
                transform.Translate(Vector3.forward * chasingMovementSpeed * dt);
                // TODO: Que abra la boca cuando lo tenga cerca
                // TODO: Que intente atraparlo de un mordisco (muerte mortísima)
                // TODO: (En el script de collision del terreno de momento de momento)
                //      Que pase a estado stun
                break;
        }
    }

    // Llamada desde los weakpoints cuando son destruidos
    public void LoseWeakPoint()
    {
        switch (currentState)
        {
            // Para la etapa inicial
            case WormStatus.Wandering:
                goesUnderground = true;
                currentTimeUnderground = 0;
                // Del targeteable, hacer variable más clara
                active = false;
                //
                exteriorWeakPoints--;
                if(interiorWeakPoints == 0)
                {
                    // Luego veremos si hacemos aqui el cambio al siguiente paso
                    //currentState
                }
                break;
        }
    }
}
