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

public enum TypeOfFire
{
    Invalid = -1,

    Direct,
    Indirect,

    Count
}

public class EnemyWeapon : MonoBehaviour
{
    public bool mainWeapon;
    // TODO: Ver si dejamos solo una al final
    public Vector2 rotationSpeed = new Vector2(0, 30);
    [Tooltip("Bullets per second.")]
    public float rateOfFire = 2;    // Bullets per second
    [Tooltip("Minimum range for turret starting to attack.")]
    public float range = 50;
    public GameObject proyectilePrefab;
    // Probablemente guardemos estos valores en la balas
    public float shootForce = 10;
    public float muzzleSpeed = 500;
    //
    public TypeOfFire typeOfFire = TypeOfFire.Direct;

    //
    public bool debugging = false;

    public ShootCalculation shootCalculation = ShootCalculation.MuzzleSpeed;
    public Vector2 maxRotationOffset;
    public GameObject shootParticlesPrefab;
    //
    public AudioClip shootingClip;

    private Transform shootPoint;
    private RobotControl player;
    private Rigidbody playerRB;
    private float timeFromLastShoot;
    //
    private Quaternion originalRotation;
    //private Vector2 originalRotationXY;
    //
    private AudioSource audioSource;
    //private EnemyManager enemyManager;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<RobotControl>();
        if(player != null)
            playerRB = player.GetComponent<Rigidbody>();
        //
        shootPoint = transform.Find("Shoot Point");
        if(shootPoint == null)
            shootPoint = transform.Find("Barrel/Shoot Point");
        //
        originalRotation = transform.localRotation;
        //originalRotationXY = new Vector2(transform.localEulerAngles.x, transform.localEulerAngles.y);
        //
        //if(originalRotation)
        //
        audioSource = GetComponent<AudioSource>();
        //
        //enemyManager = FindObjectOfType<EnemyManager>();
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
        //if (player != null)
        //{
        //    Debug.DrawRay(transform.position, transform.forward * 10, Color.blue);
        //    Vector3 playerDirection = player.transform.position - transform.position;
        //    Debug.DrawRay(transform.position, playerDirection, Color.red);
        //}

    }

    void UpdateCanonRotation(Vector3 playerDirection, float dt)
    {
        // TODO: Mirar como hacer fuego indirecto
        // Mirar angulo, si por debajo de 45
        // Debería haber otro por encima válido
        Vector3 anticipatedPlayerPosition = GeneralFunctions.AnticipateObjectivePositionForAiming(
            shootPoint.position, player.transform.position, playerRB.velocity, muzzleSpeed, dt);
        // Gravity
        anticipatedPlayerPosition.y -= GeneralFunctions.GetProyectileFallToObjective(shootPoint.position, anticipatedPlayerPosition, muzzleSpeed);

        // And check if direct or indirect fire
        if (typeOfFire == TypeOfFire.Indirect)
        {
            // TODO: Probar con el vector normalizado
            // Lo que nos importa es la dirección
            Vector3 playerDirNormalized = anticipatedPlayerPosition.normalized;
            float angle = Mathf.Acos(playerDirNormalized.y) * Mathf.Rad2Deg;
            //
            angle = 90 - angle;
            playerDirNormalized.y = Mathf.Cos(angle);

        }

        // Rotamos
        // TODO: Ver como manejar la velocidad de rotación
        transform.rotation = GeneralFunctions.UpdateRotation(transform, anticipatedPlayerPosition, rotationSpeed.x, dt);
        //transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, anticipatedPlayerPosition, rotationSpeed.y, dt, Vector3.right);
        // TODO: Hacerlo en general functions
        ConstrainRotation();
        //
    }

    // TODO: Mover a funciones generales
    void ConstrainRotation()
    {
        // Convert the rotation Cosntraints to radians
        Quaternion rotationConstrains = Quaternion.Euler(maxRotationOffset.x, maxRotationOffset.y, 0);
        //
        Quaternion constrainedRotation = transform.localRotation;
        //
        constrainedRotation.x = Mathf.Clamp(constrainedRotation.x, 
                                                originalRotation.x - rotationConstrains.x,
                                                originalRotation.x + rotationConstrains.x);
        constrainedRotation.y = Mathf.Clamp(constrainedRotation.y, 
                                                originalRotation.y - rotationConstrains.y,
                                                originalRotation.y + rotationConstrains.y);
        //
        transform.localRotation = constrainedRotation;
    }

    //
    void UpdateShooting(float dt)
    {
        // Vamos a chequear ambas para que no se pongan a duspara como locos
        // TODO: QUe no disparen cuando vayan a darle a un aliado
        if (PlayerOnSight() && PlayerInTheSight() && !ComradeInPath())
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
                if(shootParticlesPrefab != null)
                    Instantiate(shootParticlesPrefab, shootPoint.position, Quaternion.identity);
                //
                timeFromLastShoot -= 1 / rateOfFire;
                //
                //if (!enemyManager.IsFiringClipActive(shootingClip))
                //{
                //    enemyManager.AddClip(shootingClip);
                    GeneralFunctions.PlaySoundEffectWithoutOverlaping(audioSource, shootingClip);
                //}
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool ComradeInPath()
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hitInfo))
        {
            EnemyCollider enemyCollider = hitInfo.transform.GetComponent<EnemyCollider>();
            if (enemyCollider != null)
            {
                // Debug.Log("Comrade " + enemyCollider.gameObject.name + " in path");
                return true;
            }
                
        }

        return false;
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
        RaycastHit hitInfo;
        Vector3 playerDirection = player.transform.position - shootPoint.transform.position;
        if (Physics.Raycast(shootPoint.transform.position, playerDirection, out hitInfo, playerDirection.magnitude))
        {
            RobotControl robotControl = hitInfo.transform.GetComponent<RobotControl>();
            if (robotControl != null)
                return true;
        }
        return false;
    }
}
