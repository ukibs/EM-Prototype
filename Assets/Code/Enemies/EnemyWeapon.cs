using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShootCalculation
{
    Invalid = -1,

    Force,
    MuzzleSpeed,

    Count
}

public class EnemyWeapon : MonoBehaviour
{
    public bool mainWeapon;
    public Vector2 rotationSpeed = new Vector2(0, 30);
    [Tooltip("Bullets per second.")]
    public float rateOfFire = 2;    // Bullets per second
    [Tooltip("Minimum range for turret starting to attack.")]
    public float range = 50;
    public GameObject proyectilePrefab;
    // Probablemente guardemos estos valores en la balas
    public float shootForce = 10;
    public float muzzleSpeed = 500;

    public ShootCalculation shootCalculation = ShootCalculation.MuzzleSpeed;
    public Vector2 maxRotationOffset;
    public GameObject shootParticlesPrefab;

    private Transform shootPoint;
    private RobotControl player;
    private Rigidbody playerRB;
    private float timeFromLastShoot;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<RobotControl>();
        playerRB = player.GetComponent<Rigidbody>();
        shootPoint = transform.Find("Barrel/Shoot Point");
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        if(player != null)
        {
            //
            Vector3 playerDirection = player.transform.position - transform.position;
            //
            UpdateCanonRotation(playerDirection, dt);
            //
            UpdateShooting(dt);
        }
    }

    private void OnDrawGizmos()
    {
        //
        if (player != null)
        {
            Debug.DrawRay(transform.position, transform.forward * 10, Color.blue);
            Vector3 playerDirection = player.transform.position - transform.position;
            Debug.DrawRay(transform.position, playerDirection, Color.red);
        }

    }

    void UpdateCanonRotation(Vector3 playerDirection, float dt)
    {
        Vector3 anticipatedPlayerPosition = GeneralFunctions.AnticipatePlayerPositionForAiming(
            transform.position, player.transform.position, playerRB.velocity, muzzleSpeed, dt);
        transform.rotation = GeneralFunctions.UpdateRotation(transform, player.transform.position, rotationSpeed.y, dt, Vector3.right);
    }

    void UpdateShooting(float dt)
    {
        // Vamos a chequear ambas para que no se pongan a duspara como locos
        if (PlayerOnSight() && PlayerInTheSight())
        {
            timeFromLastShoot += dt;
            if (timeFromLastShoot >= 1 / rateOfFire)
            {
                //Debug.Log(shootPoint);
                // TODO: Incluir shootCalculation
                GeneralFunctions.ShootProjectile(proyectilePrefab, shootPoint.position,
                    shootPoint.rotation, shootPoint.forward, muzzleSpeed, dt, ShootCalculation.MuzzleSpeed);
                //GeneralFunctions.ShootProjectile(proyectilePrefab, shootPoint.position,
                //    shootPoint.rotation, shootPoint.forward, shootForce, dt);
                //
                Instantiate(shootParticlesPrefab, shootPoint.position, Quaternion.identity);
                //
                timeFromLastShoot -= 1 / rateOfFire;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool PlayerInTheSight()
    {
        Vector3 forward = transform.forward.normalized;
        Vector3 pointDirection = (player.transform.position - transform.position).normalized;
        float forwardAngle = Mathf.Atan2(forward.z, forward.x);
        float pDAnlge = Mathf.Atan2(pointDirection.z, pointDirection.x);
        float offset = (pDAnlge - forwardAngle) * Mathf.Rad2Deg;

        //A fix for when the number overflows the half circle
        if (Mathf.Abs(offset) > 180.0f)
        {
            offset -= 360.0f * Mathf.Sign(offset);
        }

        //De momento vamos a trabajr con 15 grados
        if (Mathf.Abs(offset) < 15.0f)
            return true;
        else
            return false;
    }

    bool PlayerOnSight()
    {
        return true;
    }
}
