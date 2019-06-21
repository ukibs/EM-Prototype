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
    private enum SubmersionStatus
    {
        Invalid = -1,

        Up,
        GoingDown,
        Down,
        GoingUp,

        Count
    }

    private enum MawStatus
    {
        Invalid = -1,

        Closed,
        Opening,
        Opened,
        Closing,

        Count
    }

    public float wanderingMovementSpeed = 10;
    public float chasingMovementSpeed = 10;
    public float rotationSpeed = 30;

    public float timeUnderground = 10;
    public float overGroundHeight = 0;
    public float underGroundHeight = -20;
    public float heightChangeSpeed = 5;

    public Transform head;
    public Transform[] maws;
    public float maxMawOpeningAngle = 45;
    public float mawSpeed = 180;

    //
    public int exteriorWeakPoints;
    public int interiorWeakPoints;

    private WormStatus currentState = WormStatus.Wandering;
    private RobotControl player;
    private Rigidbody rb;

    //
    private float currentTimeUnderground;
    private bool goesUnderground = false;

    private float currentSpeed = 0;

    //
    private float currentMawOpeningStatus;
    private MawStatus mawStatus = MawStatus.Closed;

    // Usaremos esta varaible para ver si dos o más mandíbulas están tocando al player cuando cierra la boca
    private int mawsCollidingPlayer = 0;

    public float CurrentSpeed { get { return currentSpeed; } }

    // TODO: En colisiones entre cuerpos
    // Tratar la inclinación de los cuerpos
    // No es lo mismo un choque perpedicular que uno más directo
    // Seno?

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<RobotControl>();
        //Debug.Log("Player found? " + player);
        rb = GetComponent<Rigidbody>();
        currentSpeed = wanderingMovementSpeed;
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
        UpdateBehaviour(playerDirection, dt);
        // Lo reseteamos a cada step
        mawsCollidingPlayer = 0;
    }

    private void OnCollisionStay(Collision collision)
    {
        CheckMawsCollision(collision);
    }

    private void OnDrawGizmos()
    {
        //
        if (player != null)
        {
            //
            Vector3 playerDirection = player.transform.position - transform.position;
            //
            Gizmos.DrawLine(transform.position, player.transform.position);
        }
    }

    void UpdateBehaviour(Vector3 playerDirection, float dt)
    {
        //
        switch (currentState)
        {
            // Cuando está vagando por ahí
            case WormStatus.Wandering:

                float playerDistance = (transform.position - player.transform.position).magnitude;

                // Que rote alrededor del player si se aleja demasiado
                // Así no se va a cuenca
                if (playerDistance > 700)
                {
                    // Sacar la cruz
                    Vector3 playerCross = Vector3.Cross(playerDirection, Vector3.up);
                    //transform.rotation = Quaternion.LookRotation(playerCross);
                    head.rotation = GeneralFunctions.UpdateRotationInOneAxis(head, playerCross, rotationSpeed, dt);
                }
                else
                {
                    head.Rotate(Vector3.up * dt);
                }

                // Velocity no sirve con kinematicos
                //rb.velocity = transform.forward * 100;
                //
                head.Translate(Vector3.forward * wanderingMovementSpeed * dt);
                //Debug.Log("I'm wandering");

                //
                if (goesUnderground)
                {
                    //
                    if (head.position.y > underGroundHeight)
                        head.Translate(Vector3.up * heightChangeSpeed * dt * -1);
                    //
                    currentTimeUnderground += dt;
                    if (currentTimeUnderground >= timeUnderground)
                    {
                        goesUnderground = false;

                    }

                }
                else if (!goesUnderground && head.position.y < overGroundHeight)
                {
                    head.Translate(Vector3.up * heightChangeSpeed * dt);
                }
                else if (exteriorWeakPoints == 0)
                {
                    // Aquí pasa a la fase de perseguir
                    currentState = WormStatus.Chasing;
                    currentSpeed = chasingMovementSpeed;
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
                head.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
                head.Translate(Vector3.forward * chasingMovementSpeed * dt);
                // TODO: Que abra la boca cuando lo tenga cerca
                UpdateMaw(playerDirection, dt);
                // TODO: Que intente atraparlo de un mordisco (muerte mortísima)
                // TODO: (En el script de collision del terreno de momento de momento)
                //      Que pase a estado stun
                break;
        }
    }

    // TODO: Funciona, pero hay que revisar el modelado de la mandíbula
    void CheckMawsCollision(Collision collision)
    {
        //
        if (mawStatus != MawStatus.Closing)
            return;
        //
        PlayerIntegrity robotControl = collision.collider.GetComponent<PlayerIntegrity>();
        if(collision.GetContact(0).thisCollider.tag == "Maw" && robotControl != null)
        {
            mawsCollidingPlayer++;
        }
        //
        if (mawsCollidingPlayer >= 2)
            robotControl.Die();
    }

    //
    void UpdateMaw(Vector3 playerDirection, float dt)
    {
        //
        float playerDistance = playerDirection.magnitude;
        //
        if(playerDistance > 30 && playerDistance < 100)
        {
            //
            switch (mawStatus)
            {
                case MawStatus.Closing:
                case MawStatus.Closed:
                    mawStatus = MawStatus.Opening;
                    break;
            }
        }
        //
        else if (playerDistance <= 30 || playerDistance >= 100)
        {
            //
            switch (mawStatus)
            {
                case MawStatus.Opening:
                case MawStatus.Opened:
                    mawStatus = MawStatus.Closing;
                    break;
            }
        }
        //
        //int rotationDirection = 0;
        switch (mawStatus)
        {
            case MawStatus.Opening:
                currentMawOpeningStatus -= mawSpeed * dt;
                currentMawOpeningStatus = Mathf.Max(currentMawOpeningStatus, -maxMawOpeningAngle);
                if (currentMawOpeningStatus <= -maxMawOpeningAngle)
                    mawStatus = MawStatus.Opened;
                //rotationDirection = -1;
                break;
            case MawStatus.Closing:
                currentMawOpeningStatus += mawSpeed * dt;
                currentMawOpeningStatus = Mathf.Min(currentMawOpeningStatus, 0);
                if (currentMawOpeningStatus >= maxMawOpeningAngle)
                    mawStatus = MawStatus.Closed;
                //rotationDirection = 1;
                break;
        }
        // Vamos a trabajar esto con euler ngles
        for(int i = 0; i < maws.Length; i++)
        {
            Vector3 mawRotation = maws[i].localEulerAngles;
            // Ñapaaaa
            switch (i)
            {
                case 0: mawRotation.x = currentMawOpeningStatus; break;
                case 1: mawRotation.y = currentMawOpeningStatus; break;
                case 2: mawRotation.x = -currentMawOpeningStatus; break;
                case 3: mawRotation.y = -currentMawOpeningStatus; break;
            }

            maws[i].localEulerAngles = mawRotation;

            //maws[i].Rotate(transform.right * rotationDirection * mawSpeed * dt);
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
