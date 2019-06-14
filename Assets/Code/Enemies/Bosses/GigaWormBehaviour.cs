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


    private WormStatus currentState = WormStatus.Wandering;
    private RobotControl player;
    private Rigidbody rb;
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
        switch (currentState)
        {
            case WormStatus.Wandering:
                
                float playerDistance = (transform.position - player.transform.position).magnitude;
                Vector3 playerDirection = player.transform.position - transform.position;
                // Que rote alrededor del player si se aleja demasiado
                // Así no se va a cuenca
                if(playerDistance > 500)
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
                break;
        }
    }
}
