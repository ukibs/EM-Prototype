using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormBodyBehaviour : BugBodyBehaviour
{
    //
    //public float groundingSpeed = 0.5f;
    public float lungeSpeed;
    public float lungeCooldown = 2;
    
    //
    //protected float groundedLevel = 1;
    protected bool grounded = false;
    protected BoxCollider bodyCollider;
    protected MeshRenderer meshRenderer;
    protected bool lunging = false;
    protected float timeOnCooldown = 0;

    //
    protected override void Start()
    {
        //
        base.Start();
        //
        bodyCollider = GetComponent<BoxCollider>();
        //
        SwitchGrounding();
        //
        if (HasGroundUnderneath() && !grounded)
        {
            grounded = false;
            SwitchGrounding();
        }
            
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
    private void OnCollisionStay(Collision collision)
    {
        if(collision.collider.tag.Equals("Sand") && !lunging && !HasGroundUnderneath())
        {
            grounded = false;
            SwitchGrounding();
        }
        // TODO: Montarlo bien y asegurarse de que funciona
        if (collision.collider.tag.Equals("Hard Terrain") && grounded)
        {
            Destroy(gameObject);
            // Ñapa como una catedral
            FindObjectOfType<EnemyManager>().SubtractOne(GetComponent<EnemyConsistency>().ManagerIndex);
        }
    }
    //
    protected override void OnCollisionEnter(Collision collision)
    {
        //
        base.OnCollisionEnter(collision);
        //
        if (collision.collider.tag == "Sand" && !grounded)
        {
            lunging = false;
            SwitchGrounding();
        }
        // TODO: Montarlo bien y asegurarse de que funciona
        if(collision.collider.tag.Equals("Hard Terrain") && grounded)
        {
            Destroy(gameObject);
        }
    }

    //
    //protected void UpdateGrounding()

    //
    protected void SwitchGrounding()
    {
        if (!grounded)
        {
            grounded = true;
            //bodyCollider.size = new Vector3(1, 0.1f, 1);
            //bodyCollider.center = new Vector3(0, 0.5f, 0);
            //bodyConsistency.centralPointOffset = new Vector3(0, 0.45f, 0);
            transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            grounded = false;
            //bodyCollider.size = Vector3.one;
            //bodyCollider.center = Vector3.zero;
            //bodyConsistency.centralPointOffset = Vector3.zero;
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
                    float pureDistance = playerDistance.magnitude;
                    float directionAngle = Vector3.SignedAngle(transform.forward, playerDistance.normalized, transform.up);
                    // De momento vamos a establecer un angulo puro de 15 a ambos lados
                    if (pureDistance < minimalLungeDistance && Mathf.Abs(directionAngle) < 10)
                    {
                        //Debug.Log("Direction angle: " + directionAngle);
                        currentAction = behaviour[i];
                    }
                    return;
                case Actions.ZigZagingTowardsPlayer:
                    // Esta de momento sin condición
                    currentAction = behaviour[i];
                    return;
            }
        }
    }

    //
    protected override void ExecuteCurrentAction(float dt)
    {
        // Las geenrales las chequeamos en el base
        base.ExecuteCurrentAction(dt);
        // Y aquí las específicas de bichos
        // Primero que el player siga vivo, si no mal
        if (player != null)
        {
            switch (currentAction)
            {
                case Actions.Lunging:
                    //
                    if (timeOnCooldown >= lungeCooldown)
                    {
                        Lunge();
                    }
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
            Vector3 playerDirection = (estimatedPlayerPosition - transform.position).normalized;
            rb.AddForce(playerDirection * lungeSpeed, ForceMode.Impulse);
            onFloor = false;
            lunging = true;
            timeOnCooldown = 0;
            // Vamos a cambiarle el comportamiento para que no se ponga a salta como un loco
            // TODO: Hacerlo más limpio
            currentAction = Actions.ZigZagingTowardsPlayer;
            //
            SwitchGrounding();
        }
    }
}
