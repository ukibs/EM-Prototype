using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// Floating grunt behaviour.
/// (GABI): I still need to add public variables for setting from the editor or from another class the maxIdealHeight etc.
/// It's maxIdealHeight etc are randomly generated all the time so no Grunt is floating at the same time.
/// </summary>
public class FloatingGruntBodyBehaviour : EnemyBaseBodyBehaviour
{
    #region Public
    public float minIdealHeight = 30;
    public float maxIdealHeight = 60;
    public float liftForcePerSecond = 50;
    public float liftOffsetForceU = 2f;
    public float liftOffsetForceD = 1.2f;
    public float liftOffsetBigGrunt = 15f;
    #endregion
    
    #region Protected
    protected float idealHeight;
    #endregion
    
    #region Private
    private bool goingUp = true;
    // MAX IDEAL HEIGHT RANDOM PARAMETERS
    [SerializeField]
    private float rMinMaxIdealHeight = 60f;
    [SerializeField]
    private float rMaxMaxIdealHeight = 60f;
    // MIN IDEAL HEIGHT RANDOM PARAMETERS
    [SerializeField]
    private float rMinMinIdealHeight = 10f;
    [SerializeField]
    private float rMaxMinIdealHeight = 20f;
    // todo variable: private bool goingDown = false;
    #endregion
    
    #region Properties

    /// <summary>
    /// Set the max ideal height exactly
    /// </summary>
    public float MaxIdealHeight
    {
        get { return maxIdealHeight; }
        set { maxIdealHeight = value; }
    }

    /// <summary>
    /// Set the min ideal height exactly
    /// </summary>
    public float MinIdealHeight
    {
        get { return minIdealHeight; }
        set { minIdealHeight = value; }
    }

    #endregion
    
    #region MonoBehaviour
    
    protected override void Start()
    {
        
        // Random maxIdealHeight for different movement patterns...
        // TODO: Do not hardcode the range params. Set it via a public variable.
        maxIdealHeight = UnityEngine.Random.Range(rMinMaxIdealHeight, rMaxMaxIdealHeight);
        minIdealHeight = UnityEngine.Random.Range(rMinMinIdealHeight, rMaxMinIdealHeight);
        // Set the position to a certain height (Change position from outside this class if you don't like this exact position).
        Vector3 position = transform.position;
        transform.position = new Vector3(position.x, 50f, position.z);
        // Random liftForce.
        liftForcePerSecond = UnityEngine.Random.Range(1, 1.5f);
        base.Start();
        if (GetComponent<EnemyConsistency>().inGameName != "Big Grunt")
        {
            liftOffsetBigGrunt = 1;
        }
    }

    
    protected override void Update()
    {
        base.Update();
        rb.useGravity = false;
//        VerticalMovement();
    }
    
    /// <summary>
    /// Todo: Check if Grunt is colliding on a ceiling so it goes down instead of up all the time.
    /// </summary>
    /// <param name="collision"></param>
    protected override void OnCollisionStay(Collision collision)
    {
        base.OnCollisionStay(collision);
        // _isGoingDown = true;
    }

    #endregion

    #region Methods

    /// <summary>
    /// If you wanna set the rMinMaxIdealHeight etc from a GameManager you can do it from here.
    /// </summary>
    /// <param name="minMin"></param>
    /// <param name="maxMin"></param>
    /// <param name="minMax"></param>
    /// <param name="maxMax"></param>
    public void ResetRandomnessHeights(float minMin, float maxMin, float minMax, float maxMax)
    {
        maxIdealHeight = UnityEngine.Random.Range(minMax, maxMax);
        minIdealHeight = UnityEngine.Random.Range(minMin, maxMin);
    }


    /// <summary>
    /// Adds forces for the vertical movement.
    /// </summary>
    private void VerticalMovement()
    {
        // (GABI): We calculate first the float because the order of multiplication with the vector matters for efficiency.
        float force = liftForcePerSecond * liftOffsetBigGrunt;
        if (transform.position.y > maxIdealHeight && !onFloor)
        {
            force *= -1f * liftOffsetForceD;
            rb.AddForce(transform.up * force, ForceMode.Impulse);
            goingUp = false;
        }
        else if (transform.position.y <= minIdealHeight || onFloor)
        {
            force *= liftOffsetForceU;
            rb.AddForce(transform.up * force, ForceMode.VelocityChange);
            goingUp = true;
        }
        else if (goingUp)
        {
            force *= liftOffsetForceU;
            rb.AddForce(transform.up * force, ForceMode.VelocityChange);
        }
        else
        {
            force *= -1f * liftOffsetForceD;
            rb.AddForce(transform.up * force, ForceMode.Impulse);
        }
    }
    
    /// <summary>
    /// Override Move() from EnemyBodyBehaviour for adding VerticalMovement() and ignoring gravity.
    /// </summary>
    protected override void Move()
    {
        Vector3 movingDirection = transform.forward;
        float speedMultiplier = 1;
        switch (currentAction)
        {
            case Actions.EncirclingPlayerForward:
            case Actions.GoingToPlayer:
                // Forward.
                break;
            case Actions.ZigZagingTowardsPlayer:
                currentZigZagDirection += currentZigZagVariation * Time.deltaTime;
                if (Mathf.Abs(currentZigZagDirection) >= 1)
                {
                    currentZigZagVariation *= -1;
                }
                movingDirection += transform.right * currentZigZagDirection;
                movingDirection = movingDirection.normalized;
                break;
            case Actions.EncirclingPlayerSideward:
                movingDirection = transform.right;
                speedMultiplier = 0.2f;
                break;
            case Actions.RetreatingFromPlayer:
                movingDirection = -transform.forward;
                speedMultiplier = 1f;
                break;
        }
        // (GABI): We calculate first the float because we ensure the order of multiplication (first the floats and then vector). Better for efficiency.
        float effCalcMaxSpeed = maxSpeed * speedMultiplier;
        rb.AddForce(movingDirection * effCalcMaxSpeed);
        VerticalMovement();
    }
    #endregion
    
    #region Deprecated 
//        float floatingMarging = 10;
//        // Para controlar que se mantenga en la altura idónea
//        //if (transform.position.y < idealHeight - floatingMarging)
//        //{
//        //    //
//        //    float distanceFromIdeal = idealHeight - transform.position.y;
//        //    float offsetCompensation = Mathf.Pow(distanceFromIdeal / floatingMarging, 3);
//        //    //
//        //    Vector3 verticalSpeed = Vector3.up * rb.velocity.y;
//        //    //
//        //    rb.AddForce(Vector3.up * offsetCompensation * liftForcePerSecond - verticalSpeed/2);
//        //}
//        //else 
//        if(transform.position.y < idealHeight)
//        {
//            rb.AddForce(Vector3.up * liftForcePerSecond);
//        }
//        else
//        {
//            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
//        }
//        // Para evitar obstáculos
//        //if(CheckIfObstacleInMovingDirection())
//            //rb.velocity += Vector3.up * maxSpeed;

    #endregion
}
