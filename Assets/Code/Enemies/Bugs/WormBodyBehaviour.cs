using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormBodyBehaviour : EnemyBaseBodyBehaviour
{
    //
    //public float groundingSpeed = 0.5f;
    public float lungeSpeed;
    public float lungeCooldown = 2;
    public AudioClip lungClip;
    public float maxAngleToLunge = 10;
    
    //
    //protected float groundedLevel = 1;
    protected bool grounded = false;
    protected bool lunging = false;
    protected float timeOnCooldown = 0;

    //
    protected ParticleSystem trailParticleSystem;
    protected ParticleSystem.EmissionModule trailEmmiter;

    //
    protected override void Start()
    {
        //
        base.Start();
        //
        SwitchGrounding();
        //
        if (HasGroundUnderneath() && !grounded)
        {
            grounded = false;
            SwitchGrounding();
        }
        //
        trailParticleSystem = GetComponentInChildren<ParticleSystem>();
        //
        if (trailParticleSystem == null)
            trailParticleSystem = transform.parent.GetComponentInChildren<ParticleSystem>();
        //
        trailEmmiter = trailParticleSystem.emission;
        // Para el gusano grande
        if (rb == null)
            rb = transform.parent.GetComponentInChildren<Rigidbody>();
    }
    //
    protected override void Update()
    {
        base.Update();
        //
        float dt = Time.deltaTime;
        //
        timeOnCooldown += dt;
    }
    //
    protected override void OnCollisionStay(Collision collision)
    {
        //
        base.OnCollisionStay(collision);
    }
    //
    protected override void OnCollisionEnter(Collision collision)
    {
        //
        base.OnCollisionEnter(collision);
        //
        if (collision.collider.tag == "Sand" && !grounded && lunging)
        {
            lunging = false;
            SwitchGrounding();
            //
            trailEmmiter.rateOverDistance = 100;
        }
        // TODO: Montarlo bien y asegurarse de que funciona
        if (collision.collider.tag.Equals("Hard Terrain") && grounded)
        {
            //Destroy(gameObject);
            //TODO: Ñapa como una catedral
            // Coger bien la referencia arriba
            //FindObjectOfType<EnemyManager>().SubtractOne(GetComponent<EnemyConsistency>().ManagerIndex);
            if(EnemyAnalyzer.isActive && EnemyAnalyzer.enemyTransform == transform)
                EnemyAnalyzer.Release();
            FindObjectOfType<EnemyManager>().SendToReserve(GetComponent<EnemyConsistency>().ManagerIndex, gameObject);
        }
    }
    
    //
    protected void SwitchGrounding()
    {
        if (!grounded)
        {
            grounded = true;
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
        }
        else
        {
            grounded = false;
        }
    }

    protected override void DecideActionToDo()
    {
        //base.DecideActionToDo();
        //
        Vector3 playerDistance = player.transform.position - transform.position;
        //
        for (int i = 0; i < behaviour.Length; i++)
        {
            switch (behaviour[i])
            {
                case Actions.Lunging:
                    //float pureDistance = playerDistance.magnitude;
                    //float directionAngle = Vector3.SignedAngle(transform.forward, playerDistance.normalized, transform.up);
                    // De momento vamos a establecer un angulo puro de 15 a ambos lados
                    /*if (pureDistance < minimalLungeDistance && Mathf.Abs(directionAngle) < 10)
                    {
                        //Debug.Log("Direction angle: " + directionAngle);
                        currentAction = behaviour[i];
                    }*/
                    // VAmos a probar a hacer que entre si o si
                    currentAction = behaviour[i];
                    return;
                case Actions.ApproachingPlayer3d:
                    // Esta de momento sin condición
                    currentAction = behaviour[i];
                    return;
            }
        }
    }

    //
    protected override void ExecuteCurrentAction(float dt)
    {
        // Como es terrestre que no pueda hacer nada en el aire
        if (!onFloor)
            return;
        // Las geenrales las chequeamos en el base
        base.ExecuteCurrentAction(dt);
        // Y aquí las específicas de bichos
        // Primero que el player siga vivo, si no mal
        if (player != null)
        {
            switch (currentAction)
            {
                case Actions.Lunging:
                    // TODO: Desguarrear esto
                    if (timeOnCooldown >= lungeCooldown)
                    {
                        Vector3 playerDistance = player.transform.position - transform.position;
                        float pureDistance = playerDistance.magnitude;
                        float directionAngle = Vector3.SignedAngle(transform.forward, playerDistance.normalized, transform.up);
                        if (pureDistance < minimalLungeDistance && Mathf.Abs(directionAngle) < maxAngleToLunge)
                        {
                            //Debug.Log("Direction angle: " + directionAngle);
                            Lunge();
                        }
                        // Si no que avance normal
                        else
                        {
                            //
                            currentAction = Actions.GoingToPlayer;
                            ExecuteCurrentAction(dt);
                        }
                    }
                    else
                    {
                        //
                        currentAction = Actions.GoingToPlayer;
                        ExecuteCurrentAction(dt);
                    }
                    break;
                case Actions.ApproachingPlayer3d:
                    transform.rotation = GeneralFunctions.UpdateRotation(transform, player.transform.position, rotationSpeed, dt);
                    break;
            }
        }
    }

    //
    protected override void Move()
    {
        //
        if (grounded)
        {
            base.Move();
        }
        
    }

    //
    protected new void Lunge()
    {
        //
        if (HasGroundUnderneath() && grounded && !lunging)
        {
            //Debug.Log("Performing lunge");
            float estimatedFlyingTimeToPlayer = GeneralFunctions.EstimateFlyingTimeWithDrag(transform.position, player.transform.position,
                lungeSpeed, rb.drag);
            Vector3 estimatedPlayerPosition = player.transform.position + (PlayerReference.playerRb.velocity * estimatedFlyingTimeToPlayer);
            //estimatedPlayerPosition = GeneralFunctions.AnticipateObjectivePositionForAiming(transform.position, )
            Vector3 playerDirection = (estimatedPlayerPosition - transform.position).normalized;

            //
            //rb.AddForce(playerDirection * lungeSpeed, ForceMode.Impulse);
            rb.velocity = playerDirection * lungeSpeed;

            onFloor = false;
            lunging = true;
            timeOnCooldown = 0;
            // Vamos a cambiarle el comportamiento para que no se ponga a salta como un loco
            // TODO: Hacerlo más limpio
            currentAction = Actions.ZigZagingTowardsPlayer;
            //
            SwitchGrounding();
            //
            trailEmmiter.rateOverDistance = 0;
            //
            GeneralFunctions.PlaySoundEffect(audioSource, lungClip);
        }
    }
}
