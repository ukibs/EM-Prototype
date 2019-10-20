using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreecherBodyBehaviour : FlyingEnemyBodyBehaviour
{
    public float minIdealHeight = 150;
    public float maxIdealHeight = 200;
    //public float liftForcePerSecond = 200;
    public float timeToChargeBall = 20;
    public float electricArcDamage = 50;

    public Transform loadingBall;

    private float timeCharguingBall = 0;
    private float currentIdealHeight = 0;


    // Start is called before the first frame update
    protected override void Start()
    {
        //
        //currentIdealHeight = UnityEngine.Random.Range(minIdealHeight, maxIdealHeight);
        // Ñapa
        //transform.position = new Vector3(transform.position.x, currentIdealHeight, transform.position.z);
        //
        base.Start();
        //
        //currentIdealHeight = idealHeight;
    }

    // Update is called once per frame
    protected override void Update()
    {
        //
        base.Update();
        //
        float dt = Time.deltaTime;
        //
        //timeCharguingBall += dt;
        // Hardocdeamos el 2 que es la escala que usamos
        //float ballScale = Mathf.Min((timeCharguingBall / timeToChargeBall) * 2, 2);
        //loadingBall.transform.localScale = Vector3.one * ballScale;
        //
        //VerticalMovement();
    }

    protected override void OnDrawGizmos()
    {
        //
        base.OnDrawGizmos();
        //
        //if (player != null)
        //{
        //    Debug.DrawRay(transform.position, transform.forward * 10, Color.blue);
        //    Vector3 playerDirection = player.transform.position - transform.position;
        //    Debug.DrawRay(transform.position, playerDirection, Color.red);
        //}

    }

    // Ahora trabajaremos aqui los comporatmientos
    #region Methods

    protected override void DecideActionToDo()
    {
        //
        Vector3 playerDistance = player.transform.position - transform.position;
        if (player.transform.position.y > 100)
        {
            currentAction = Actions.ApproachingPlayer3d;
        }
        //
        base.DecideActionToDo();
        //
        //if (enemyFormation != null && enemyFormation.FormationLeader != this)
        //{
        //    currentAction = Actions.GoInFormation;
        //    return;
        //}

        ////
        //Vector3 playerDistance = player.transform.position - transform.position;
        //if (player.transform.position.y > 100)
        //{
        //    currentAction = Actions.ApproachingPlayer3d;
        //}
        //else if (playerDistance.magnitude < minimalShootDistance)
        //{
        //    currentAction = Actions.EncirclingPlayerForward;
        //}
        //else
        //{
        //    currentAction = Actions.GoingToPlayer;
        //}
    }

    //
    protected override void ExecuteCurrentAction(float dt)
    {

        //
        //if (ofFoot) return;
        //
        base.ExecuteCurrentAction(dt);

        if (timeCharguingBall >= timeToChargeBall)
        {
            weapons[0].Shoot(Time.deltaTime);
            timeCharguingBall = 0;
        }

        //
        //switch (currentAction)
        //{
        //    case Actions.ApproachingPlayer3d:
        //        transform.rotation = GeneralFunctions.UpdateRotation(transform, player.transform.position, rotationSpeed, dt);
        //        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
        //        //
        //        ElectricArcField();
        //        break;
        //    case Actions.EncirclingPlayerForward:
        //        transform.rotation = GeneralFunctions.UpdateRotationOnCross(transform, player.transform.position, rotationSpeed, dt);
        //        // Y cargado de proyectil
        //        if (timeCharguingBall >= timeToChargeBall)
        //        {
        //            weapons[0].Shoot(Time.deltaTime);
        //            timeCharguingBall = 0;
        //        }
        //        //VerticalMovement();
        //        //
        //        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        //        break;
        //    case Actions.GoingToPlayer:
        //        transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
        //        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        //        //VerticalMovement();
        //        break;
        //    case Actions.GoInFormation:
        //        Vector3 objectivePosition = enemyFormation.GetFormationPlaceInWorld(this);
        //        transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, objectivePosition, rotationSpeed * movementStatus, dt);
        //        //Move();
        //        break;
        //}

        // Y por último que mueva
        //Move(dt);
        //VerticalMovement(dt);
    }

    //
    //protected void VerticalMovement(float dt)
    //{
    //    if (transform.position.y < currentIdealHeight)
    //    {
    //        //
    //        //float forceToAdd = liftForcePerSecond * (1 - (transform.position.y / currentIdealHeight));
    //        //
    //        rb.AddForce(Vector3.up * liftForcePerSecond);
    //        //rb.velocity += Vector3.up * liftForcePerSecond * dt;
    //    }
    //}

    //protected override void Move(float dt)
    //{
    //    // Aqui no hacemos uso del Move padre
    //    //base.Move();
    //    //rb.velocity = transform.forward * maxSpeed;
    //    rb.AddForce(transform.forward * maxSpeed * dt, ForceMode.Impulse);
    //}

    // TODO: Hacerla bien
    void ElectricArcField()
    {
        //
        float playerDistance = (player.transform.position - transform.position).magnitude;
        float arcFieldReach = 50;
        //
        if(playerDistance < arcFieldReach)
        {
            PlayerReference.playerIntegrity.ReceiveEnvionmentalDamage(10);
            //GeneralFunctions.PlaySoundEffect(audioSource, electricClip);
        }
    }

    #endregion
}
