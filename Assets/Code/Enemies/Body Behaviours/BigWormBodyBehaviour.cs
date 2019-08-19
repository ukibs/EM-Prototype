using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigWormBodyBehaviour : MonoBehaviour
{
    public enum BigWormStatus
    {
        Invalid = -1,

        GoingToPlayer,
        ApproachingPlayer,
        Lunging,
        ReturningToIdealHeight,

        Count
    }

    public float maxSpeed = 30;
    public float rotationSpeed = 180;
    public float minimalApproachDistance = 100;
    public float minimalLungeDistance = 50;
    public float maxAngleToLunge = 30;
    public float lungeForce = 100;

    public float underSandIdealHeight = -50;
    public Rigidbody headRb;
    public Transform[] bodyParts;

    private BigWormStatus bigWormStatus = BigWormStatus.GoingToPlayer;
    private RobotControl player;
    private float movementStatus = 1;
    private float minimalTimeToCheckLungeEnd = 5;
    private float currentTimeToCheckLungeEnd = 0;

    void Start()
    {
        // Ponemos todo el cuerpo en la altura idonea
        Vector3 startPosition = transform.position;
        startPosition.y = underSandIdealHeight;
        transform.position = startPosition;
        //
        player = FindObjectOfType<RobotControl>();
    }

    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        ExecuteCurrentAction(dt);
    }

    void OnCollisionEnter(Collision collision)
    {
        //base.OnCollisionEnter(collision);
        // TODO: Chequear con la cabeza
        //if (collision.collider.tag == "Sand" && bigWormStatus == BigWormStatus.Lunging)
        //{
        //    //
        //    //trailEmmiter.rateOverDistance = 100;
        //    bigWormStatus = BigWormStatus.ReturningToIdealHeight;
        //}
    }

    void DecideActionToDo()
    {
        //base.DecideActionToDo();
        Vector3 playerDistanceAndDirection = player.transform.position - headRb.transform.position;
        
    }

    //
    void ExecuteCurrentAction(float dt)
    {
        //
        if (player == null) return;
        //
        Vector3 playerDistanceAndDirection = player.transform.position - headRb.transform.position;
        // Y aquí las específicas de bichos
        // Primero que el player siga vivo, si no mal
        switch (bigWormStatus)
        {
            case BigWormStatus.GoingToPlayer:
                headRb.transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(headRb.transform, player.transform.position, rotationSpeed, dt);
                Move();
                //
                if(playerDistanceAndDirection.magnitude < minimalApproachDistance)
                {
                    bigWormStatus = BigWormStatus.ApproachingPlayer;
                }
                break;
            case BigWormStatus.ApproachingPlayer:
                //
                float directionAngle = Vector3.SignedAngle(transform.forward, playerDistanceAndDirection.normalized, transform.up);
                float pureDistance = playerDistanceAndDirection.magnitude;
                //
                if (pureDistance < minimalLungeDistance/* && Mathf.Abs(directionAngle) < maxAngleToLunge*/)
                {
                    //Debug.Log("Direction angle: " + directionAngle);
                    //Debug.Log("Performing lunge");
                    //Lunge();
                    headRb.transform.LookAt(player.transform);
                    headRb.velocity *= 2;
                    bigWormStatus = BigWormStatus.Lunging;
                    ApplyGravityOnBodyParts(true);
                    currentTimeToCheckLungeEnd = 0;
                    // TODO: Que Carol de un aviso
                    return;
                }
                //
                headRb.transform.rotation = GeneralFunctions.UpdateRotation(headRb.transform, player.transform.position, rotationSpeed, dt);
                Move();
                break;
            case BigWormStatus.Lunging:
                // Que la cabeza vaya rotando en la dirección que mira
                headRb.transform.LookAt(headRb.transform.position + headRb.velocity);
                //
                currentTimeToCheckLungeEnd += dt;
                //
                if (headRb.transform.position.y < -20 && currentTimeToCheckLungeEnd >= minimalTimeToCheckLungeEnd)
                {
                    Debug.Log("Returning to ideal height");
                    bigWormStatus = BigWormStatus.ReturningToIdealHeight;
                    ApplyGravityOnBodyParts(false);
                }
                    
                break;
            case BigWormStatus.ReturningToIdealHeight:
                //
                Move();
                //
                if (headRb.transform.position.y <= underSandIdealHeight)
                    bigWormStatus = BigWormStatus.GoingToPlayer;
                break;
        }
    }

    //
    void Move()
    {
        //
        Vector3 movingDirection = headRb.transform.forward;
        headRb.velocity = (movingDirection * maxSpeed * movementStatus);
        //
        //bodyParts[0].LookAt(headRb.transform);
        ////
        //for(int i = 0; i < bodyParts.Length; i++)
        //{
        //    //
        //    if (i > 0)
        //        bodyParts[i].LookAt(bodyParts[i - 1]);
        //    //
        //    movingDirection = bodyParts[i].forward;
        //    //bodyPartsRb[i].velocity = (movingDirection * maxSpeed * movementStatus);
        //    bodyParts[i].position += movingDirection * maxSpeed * movementStatus * Time.deltaTime;
        //}
        
    }

    //
    void Lunge()
    {
        //
        Vector3 playerDistanceAndDirection = player.transform.position - headRb.transform.position;
        //
        headRb.AddForce(playerDistanceAndDirection * lungeForce, ForceMode.Impulse);
    }

    //
    void ApplyGravityOnBodyParts(bool yesNo)
    {
        headRb.useGravity = yesNo;
        //
        //
        //for (int i = 0; i < bodyPartsRb.Length; i++)
        //{
        //    bodyPartsRb[i].useGravity = yesNo;
        //}
    }
}
