using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurret : MonoBehaviour
{
    // TODO: Que los pille automaticamente
    public EnemyWeapon[] weapons;
    //public Transform[] sideWeapons;
    public float turretRotationSpeed = 90.0f;
    public float maxRotationOffset;
    
    private RobotControl player;
    private Rigidbody playerRB;     // Pillamos el rigibody del player para mirar su velocity
    private float timeFromLastShoot;

    #region UnityMethods

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<RobotControl>();
        playerRB = player.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Chequeamos player vivo por si acá
        if(player != null)
        {
            //
            float dt = Time.deltaTime;
            //
            Vector3 playerDirection = player.transform.position - transform.position;
            playerDirection.y = transform.position.y;
            //
            UpdateTurretRotation(playerDirection, dt);
        }
        
    }

    //private void OnDrawGizmos()
    //{
    //    //
    //    if(player != null)
    //    {
    //        Debug.DrawRay(transform.position, transform.forward * 5, Color.blue);
    //        Vector3 playerDirection = player.transform.position - transform.position;
    //        Debug.DrawRay(transform.position, playerDirection, Color.red);
    //    }

    //}

    private void OnJointBreak(float breakForce)
    {
        transform.parent = null;
    }

    #endregion

    #region Methods

    //Vector3 AnticipatePlayerPositionForShooting(float dt)
    //{
    //    Vector3 playerFutureEstimatedPosition = new Vector3();

    //    // Determinamos la distancia para ver cuanto anticipar en función de nuestra muzzle speed
    //    float distanceToPlayer = (player.transform.position - transform.position).magnitude;
    //    float timeForBulletToReachPlayer = weapons[0].muzzleSpeed / distanceToPlayer * dt;

    //    playerFutureEstimatedPosition = player.transform.position + (playerRB.velocity * timeForBulletToReachPlayer);

    //    return playerFutureEstimatedPosition;
    //}

    void UpdateTurretRotation(Vector3 playerDirection, float dt)
    {
        //
        //transform.rotation = GeneralFunctions.UpdateRotation(transform, player.transform.position, turretRotationSpeed, dt);
        Vector3 anticipatedPlayerPosition = GeneralFunctions.AnticipateObjectivePositionForAiming(
            transform.position, player.transform.position, playerRB.velocity, weapons[0].muzzleSpeed, dt);
        transform.rotation = GeneralFunctions.UpdateRotation(transform, anticipatedPlayerPosition, turretRotationSpeed, dt);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float MainWeaponsMinRange()
    {
        float minRange = Mathf.Infinity;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].mainWeapon && weapons[i].range < minRange)
            {
                minRange = weapons[i].range;
            }
        }

        //
        if (minRange == Mathf.Infinity) minRange = 0;

        return minRange;
    }

    #endregion
}
