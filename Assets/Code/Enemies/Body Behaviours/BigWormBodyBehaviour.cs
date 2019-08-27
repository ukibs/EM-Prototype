using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigWormBodyBehaviour : EnemyBaseBodyBehaviour
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

    public float movementSpeed = 30;
    //public float rotationSpeed = 180;
    public float minimalXyApproachDistance = 100;
    //public float minimalLungeDistance = 50;
    //public float maxAngleToLunge = 30;
    public float lungeSpeed = 100;
    //
    public AudioClip lungeClip;

    public float underSandIdealHeight = -50;
    public Rigidbody headRb;
    public Transform[] bodyParts;

    // Público para algunos chequeos
    public BigWormStatus bigWormStatus = BigWormStatus.GoingToPlayer;
    //private RobotControl player;
    private CarolBaseHelp carolHelp;
    //private float movementStatus = 1;
    private float minimalTimeToCheckLungeEnd = 10;
    private float currentTimeToCheckLungeEnd = 0;
    //private AudioSource audioSource;

    protected override void Start()
    {
        // Ponemos todo el cuerpo en la altura idonea
        Vector3 startPosition = transform.position;
        startPosition.y = underSandIdealHeight;
        transform.position = startPosition;
        //
        player = FindObjectOfType<RobotControl>();
        carolHelp = FindObjectOfType<CarolBaseHelp>();
        // Este va en la cabeza
        audioSource = headRb.GetComponent<AudioSource>();
    }

    protected override void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        ExecuteCurrentAction(dt);
    }

    protected override void OnCollisionEnter(Collision collision)
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

    protected override void DecideActionToDo()
    {
        //base.DecideActionToDo();
        Vector3 playerDistanceAndDirection = player.transform.position - headRb.transform.position;
        
    }

    //
    protected override void ExecuteCurrentAction(float dt)
    {
        //
        if (player == null) return;
        //
        Vector3 playerDistanceAndDirection = player.transform.position - headRb.transform.position;
        Vector2 xZDistanceToPlayerAndDirection = new Vector2(player.transform.position.x - headRb.transform.position.x, 
                                                player.transform.position.z - headRb.transform.position.z);
        // Y aquí las específicas de bichos
        // Primero que el player siga vivo, si no mal
        switch (bigWormStatus)
        {
            case BigWormStatus.GoingToPlayer:
                headRb.transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(headRb.transform, player.transform.position, rotationSpeed, dt);
                Move();
                //
                if(xZDistanceToPlayerAndDirection.magnitude < minimalXyApproachDistance)
                {
                    bigWormStatus = BigWormStatus.ApproachingPlayer;
                    Debug.Log("Approaching player");

                    // TODO: Que Carol de un aviso
                    carolHelp.TriggerGeneralAdvice("DangerIncoming");
                }
                break;
            case BigWormStatus.ApproachingPlayer:
                //
                //float directionAngle = Vector3.SignedAngle(transform.forward, playerDistanceAndDirection.normalized, transform.up);
                float pureDistance = playerDistanceAndDirection.magnitude;

                //
                if (pureDistance < minimalLungeDistance/* && Mathf.Abs(directionAngle) < maxAngleToLunge*/)
                {
                    //Debug.Log("Direction angle: " + directionAngle);
                    Debug.Log("Performing lunge");
                    //Lunge();
                    GeneralFunctions.PlaySoundEffect(audioSource, lungeClip);
                    headRb.transform.LookAt(player.transform);
                    headRb.velocity = headRb.velocity.normalized * lungeSpeed;
                    bigWormStatus = BigWormStatus.Lunging;
                    ApplyGravityOnBodyParts(true);
                    currentTimeToCheckLungeEnd = 0;
                    return;
                }

                //
                headRb.transform.rotation = GeneralFunctions.UpdateRotation(headRb.transform, player.transform.position, rotationSpeed, dt);
                Move();
                break;
            case BigWormStatus.Lunging:
                // Que la cabeza vaya rotando en la dirección que mira
                if(headRb.velocity.magnitude >= Mathf.Epsilon)
                    headRb.transform.LookAt(headRb.transform.position + headRb.velocity);
                else
                {
                    Debug.Log("Returning to ideal height");
                    bigWormStatus = BigWormStatus.ReturningToIdealHeight;
                    headRb.transform.eulerAngles = new Vector3(90, headRb.transform.eulerAngles.y, headRb.transform.eulerAngles.z);
                }
                //
                currentTimeToCheckLungeEnd += dt;
                //
                if (currentTimeToCheckLungeEnd >= minimalTimeToCheckLungeEnd && HeadIsUnderSand())
                {
                    Debug.Log("Returning to ideal height");
                    bigWormStatus = BigWormStatus.ReturningToIdealHeight;
                    //ApplyGravityOnBodyParts(false);
                    //
                    headRb.transform.eulerAngles = new Vector3(90, headRb.transform.eulerAngles.y, headRb.transform.eulerAngles.z);
                }
                    
                break;
            case BigWormStatus.ReturningToIdealHeight:
                //
                Move();
                
                //
                if (headRb.transform.position.y <= underSandIdealHeight)
                {
                    //
                    bigWormStatus = BigWormStatus.GoingToPlayer;
                    //
                    headRb.transform.eulerAngles = new Vector3(0, headRb.transform.eulerAngles.y, headRb.transform.eulerAngles.z);

                    ApplyGravityOnBodyParts(false);
                }

                break;
        }
    }

    //
    protected override void Move()
    {
        //
        Vector3 movingDirection = headRb.transform.forward;
        headRb.velocity = (movingDirection * movementSpeed * movementStatus);
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
    //void Lunge()
    //{
    //    //
    //    Vector3 playerDistanceAndDirection = player.transform.position - headRb.transform.position;
    //    //
    //    headRb.AddForce(playerDistanceAndDirection * lungeForce, ForceMode.Impulse);
    //}

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

    bool HeadIsUnderSand()
    {
        return headRb.transform.position.y < -5;
    }
}
