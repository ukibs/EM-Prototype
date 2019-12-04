using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repulsor : MonoBehaviour {

    public float idealDistanceFromFloor = 1;
    public float maxRepulsionStrength = 10;
    public float repulsionDamp = 0.2f;

    public ParticleSystem dustEmitterStatic;
    public ParticleSystem dustEmitterMovement;

    public AudioClip jumpClip;

    // TODO: Meter clip de caída detenida
    // Y que suene fuerte si es un buen frenazo

    private Rigidbody rb;
    private bool isOnFloor;
    //private BallsScene tutorial;

    // Variables para pelear con el particle system
    private ParticleSystem currentParticleSystem;
    private float currentParticleSpeed = 5;
    private float currentParticleRate = 10;
    //
    private InputManager inputManager;
    private GameManager gameManager;
    private RobotControl robotControl;
    private AudioSource audioSource;
    private PlayerIntegrity playerIntegrity;

    // TODO: Mandarla a su sitio después del testeo
    private float offsetCompensation = 0;

    //
    private float timeWithoutFloor = 0;
    private float dashCooldown = 0;

    #region Properties

    public bool IsOnFloor { get { return isOnFloor; } }

    #endregion

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        //dustEmitterStatic.SetActive(true);
        currentParticleSystem = dustEmitterStatic;
        inputManager = FindObjectOfType<InputManager>();
        gameManager = FindObjectOfType<GameManager>();
        robotControl = GetComponent<RobotControl>();
        audioSource = GetComponent<AudioSource>();
        playerIntegrity = GetComponent<PlayerIntegrity>();
        //
        StopDustEmitterParticleSystem(dustEmitterMovement);
        //
        transform.position = Vector3.up * idealDistanceFromFloor;
	}
	
	// Update is called once per frame
	void Update () {

        //
        if (playerIntegrity != null && playerIntegrity.IsDead) return;
        //
        float dt = Time.deltaTime;

        //
        Vector3 floorPoint;

        //isOnFloor = CheckFloor(out floorPoint);
        isOnFloor = CheckFloorWithSphere(out floorPoint);

        // Para que no de error en la intro
        if (robotControl != null)
        {
            RepulsorJump();
        }

        if (isOnFloor /*&& tutorial != null*/)
        {
            UpdateDustEmitter(floorPoint);
            timeWithoutFloor = 0;
        }
        else
        {
            // TODO: Bajar la emisión y ya
            //dustEmitterStatic.SetActive(false);
            //dustEmitterMovement.SetActive(false);
            //StopDustEmitterParticleSystem(currentParticleSystem);
            StopDustEmitterParticleSystem(dustEmitterStatic);
            StopDustEmitterParticleSystem(dustEmitterMovement);
            //
            timeWithoutFloor += dt;
        }
        //
        dashCooldown += dt;
	}

    void RepulsorJump()
    {
        // Salto con el repulsor en vez de con las palas
        if (inputManager.JumpButton && robotControl.ActiveJumpMode == JumpMode.RepulsorJump)
        {
            // TODO: Meter un margen extra aqui
            if (isOnFloor)
            {
                //
                rb.AddForce(transform.up * gameManager.playerAttributes.jumpForce, ForceMode.Impulse);
                // Extra para no pasarnos de corto ni de largo
                // ÑApaaaaaa
                Vector3 fixedVelocidty = rb.velocity;
                fixedVelocidty.y = Mathf.Clamp(fixedVelocidty.y, gameManager.playerAttributes.jumpForce, gameManager.playerAttributes.jumpForce);
                rb.velocity = fixedVelocidty;
            }
            // Metemos aqui la opción de impulsarnos hacia el suelo
            else if (timeWithoutFloor > 1) 
            {
                //
                rb.AddForce(-transform.up * gameManager.playerAttributes.jumpForce * 10, ForceMode.Impulse);
                //
                timeWithoutFloor = 0;
            }
            else
            {
                return;
            }
            // Sonido de salto
            GeneralFunctions.PlaySoundEffect(audioSource, jumpClip);
        }
    }

    //
    public void RepulsorDash(Vector3 xzDirection)
    {
        // Salto con el repulsor en vez de con las palas
        if (dashCooldown > 1)
        {
            //
            //Vector3 xzVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            //
            robotControl.ChangeDampingType(DampingType.None);
            rb.AddForce(xzDirection * gameManager.playerAttributes.jumpForce * 10, ForceMode.Impulse);
            //
            dashCooldown = 0;
            // Sonido de dash
            GeneralFunctions.PlaySoundEffect(audioSource, jumpClip);
        }
    }

    private void OnGUI()
    {
        //GUI.Label(new Rect(10, 10, 200, 20), "Y speed: " + rb.velocity.y);
        //GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 30, 200, 20), "OC: " + offsetCompensation);
    }

    #region Methods

    // TODO: Hacerlo trabajar con los ataques
    //public void CompensateWeaponRecoil()
    //{

    //}

    /// <summary>
    /// 
    /// </summary>
    //bool CheckFloor(out Vector3 floorPoint)
    //{
    //    RaycastHit hitInfo;
    //    floorPoint = Vector3.positiveInfinity;
    //    if(Physics.Raycast(transform.position, Vector3.down, out hitInfo, idealDistanceFromFloor))
    //    {
    //        //Debug.Log("Repulsing");
    //        float distanceFromFloor = (transform.position - hitInfo.point).magnitude;

    //        ApplyRepulsion(distanceFromFloor);
    //        //
    //        floorPoint = hitInfo.point;
    //        return true;
    //    }
    //    return false;
    //}

    //
    bool CheckFloorWithSphere(out Vector3 floorPoint)
    {
        RaycastHit hitInfo;
        floorPoint = Vector3.positiveInfinity;
        // Cogemos el valor de proyectiles enemigos, e invertimos
        int layerMask = 1 << 12;
        layerMask = ~layerMask;
        //
        if (Physics.SphereCast(transform.position, 0.5f, Vector3.down, out hitInfo, idealDistanceFromFloor, layerMask))
        {
            float distanceFromFloor = (transform.position - hitInfo.point).magnitude;

            ApplyRepulsion(distanceFromFloor);
            //
            floorPoint = hitInfo.point;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="distanceFromFloor"></param>
    void ApplyRepulsion(float distanceFromFloor)
    {
        // Vamos a hacerlo al cuadrado para hacer más remarcado el efecto
        offsetCompensation = 1 + Mathf.Pow( 1 - (distanceFromFloor / idealDistanceFromFloor), 3);
        //offsetCompensation = Mathf.Max(offsetCompensation, 0);

        //
        if(offsetCompensation < 1.1f) offsetCompensation = 1;
        
        // TODO: Montarlo para que funcione también cuando cambie el up
        // Recuerda, y negativa hacia abajo
        float fallingSpeed = Mathf.Min(rb.velocity.y, 0);
        //fallingSpeed = Mathf.Abs(fallingSpeed);
        // No multiplicamos por peso del player porque vale 1
        float speedCompensation = Mathf.Min(-fallingSpeed, maxRepulsionStrength);
        //speedCompensation = 0;
        //speedCompensation = 0;

        //float compensationOffset = 1 - (distanceFromFloor / idealDistanceFromFloor);
        //float compensationOffset = distanceFromFloor / idealDistanceFromFloor;
        // rb.AddForce(Vector3.up * repulsionStrength * compensationOffset);
        //rb.AddForce(transform.up * repulsionStrength * (offsetCompensation +  speedCompensation) );

        // A tener en cuenta, 1 es 
        Vector3 forceToApply = transform.up * (offsetCompensation + speedCompensation) * rb.mass;
        rb.AddForce(forceToApply, ForceMode.Impulse);
        Debug.Log("Applying " + forceToApply + " repulsion force. Offset compensation: " + 
            offsetCompensation + ", speed compensation: " + speedCompensation);

        // TODO: Revisar esto
        UpdateDustEmitterParticleSystem(offsetCompensation, fallingSpeed / 2);
        
    }

    //
    void StopDustEmitterParticleSystem(ParticleSystem systemToStop)
    {
        //
        ParticleSystem.EmissionModule emissionModule = systemToStop.emission;
        emissionModule.rateOverTime = 0;
        //
        ParticleSystem.MainModule mainModule = systemToStop.main;
        mainModule.startSpeed = 0;
    }

    //
    void UpdateDustEmitterParticleSystem(float compensationOffset, float fallingSpeed)
    {
        //
        ParticleSystem.EmissionModule emissionModule = currentParticleSystem.emission;
        emissionModule.rateOverTime = currentParticleRate + (currentParticleRate * (compensationOffset + Mathf.Pow(fallingSpeed, 1)));
        //
        ParticleSystem.MainModule mainModule = currentParticleSystem.main;
        mainModule.startSpeed = currentParticleSpeed + (currentParticleSpeed * (compensationOffset + Mathf.Pow(fallingSpeed, 1)));
    }

    // TODO: Revisar para que partículas no desaparezcan de repente
    // En vez de activar/desactivar trabajar con las emisiones
    void UpdateDustEmitter(Vector3 floorPoint)
    {
        //
        if (rb.velocity.sqrMagnitude <= 5.0f)
        {
            //dustEmitterStatic.SetActive(true);
            //dustEmitterMovement.SetActive(false);

            dustEmitterStatic.transform.position = floorPoint + (Vector3.up * 0.1f);

            //
            currentParticleSystem = dustEmitterStatic;
            currentParticleSpeed = 5;
            currentParticleRate = 10;
        }
        else
        {
            //dustEmitterStatic.SetActive(false);
            //dustEmitterMovement.SetActive(true);

            dustEmitterMovement.transform.position = floorPoint + (Vector3.up * 0.1f);

            //
            currentParticleSystem = dustEmitterMovement;
            currentParticleSpeed = 2;
            currentParticleRate = 100;
        }
    }

    #endregion

}
