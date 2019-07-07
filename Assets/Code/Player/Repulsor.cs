using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repulsor : MonoBehaviour {

    public float idealDistanceFromFloor = 1;
    public float repulsionStrength = 10;
    public float repulsionDamp = 0.2f;

    public GameObject dustEmitterStatic;
    public GameObject dustEmitterMovement;

    private Rigidbody rb;
    private bool isOnFloor;
    //private BallsScene tutorial;

    // Variables para pelear con el particle system
    private ParticleSystem currentParticleSystem;
    private float currentParticleSpeed = 5;
    private float currentParticleRate = 10;

    #region Properties

    public bool IsOnFloor { get { return isOnFloor; } }

    #endregion

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        //dustEmitterStatic.SetActive(true);
        currentParticleSystem = dustEmitterStatic.GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //
        Vector3 floorPoint;
        isOnFloor = CheckFloor(out floorPoint);
        if (isOnFloor /*&& tutorial != null*/)
        {
            UpdateDustEmitter(floorPoint);
        }
        else
        {
            dustEmitterStatic.SetActive(false);
            dustEmitterMovement.SetActive(false);
        }
	}

    private void OnGUI()
    {
        //GUI.Label(new Rect(10, 10, 200, 20), "Y speed: " + rb.velocity.y);
    }

    #region Methods

    /// <summary>
    /// 
    /// </summary>
    bool CheckFloor(out Vector3 floorPoint)
    {
        RaycastHit hitInfo;
        floorPoint = Vector3.positiveInfinity;
        if(Physics.Raycast(transform.position, Vector3.down, out hitInfo, idealDistanceFromFloor))
        {
            //Debug.Log("Repulsing");
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
        float compensationOffset = Mathf.Pow( 1 - (distanceFromFloor / idealDistanceFromFloor), 2);
        // TODO: Montarlo para que funcione también cuando cambie el up
        // Recuerda, y negativa hacia abajo
        float fallingSpeed = Mathf.Min(rb.velocity.y, 0);
        fallingSpeed = Mathf.Abs(fallingSpeed);
        //float compensationOffset = 1 - (distanceFromFloor / idealDistanceFromFloor);
        //float compensationOffset = distanceFromFloor / idealDistanceFromFloor;
        // rb.AddForce(Vector3.up * repulsionStrength * compensationOffset);
        rb.AddForce(transform.up * repulsionStrength * (compensationOffset + Mathf.Pow(fallingSpeed / 2,1) ) );
        //
        UpdateDustEmitterParticleSystem(compensationOffset, fallingSpeed / 2);
        
    }

    void UpdateDustEmitterParticleSystem(float compensationOffset, float fallingSpeed)
    {
        //
        ParticleSystem.EmissionModule emissionModule = currentParticleSystem.emission;
        emissionModule.rateOverTime = currentParticleRate + (currentParticleRate * (compensationOffset + Mathf.Pow(fallingSpeed, 1)));
        //
        ParticleSystem.MainModule mainModule = currentParticleSystem.main;
        mainModule.startSpeed = currentParticleSpeed + (currentParticleSpeed * (compensationOffset + Mathf.Pow(fallingSpeed, 1)));
    }

    // Ahora no lo usamos
    // TODO: Borrar cuando estemos seguros
    void SoftenVerticalImpulse()
    {
        Vector3 currentVelocity = rb.velocity;
        currentVelocity.y *= 1 - repulsionDamp;
        //if (Mathf.Abs(currentVelocity.y) < 0.2) currentVelocity.y = 0;
        rb.velocity = currentVelocity;
    }

    // TODO: Revisar para que partículas no desaparezcan de repente
    // En vez de activar/desactivar trabajar con las emisiones
    void UpdateDustEmitter(Vector3 floorPoint)
    {
        //
        if (rb.velocity.sqrMagnitude <= 5.0f)
        {
            dustEmitterStatic.SetActive(true);
            dustEmitterMovement.SetActive(false);

            dustEmitterStatic.transform.position = floorPoint + (Vector3.up * 0.1f);

            //
            currentParticleSystem = dustEmitterStatic.GetComponent<ParticleSystem>();
            currentParticleSpeed = 5;
            currentParticleRate = 10;
        }
        else
        {
            dustEmitterStatic.SetActive(false);
            dustEmitterMovement.SetActive(true);

            dustEmitterMovement.transform.position = floorPoint + (Vector3.up * 0.1f);

            //
            currentParticleSystem = dustEmitterMovement.GetComponent<ParticleSystem>();
            currentParticleSpeed = 10;
            currentParticleRate = 100;
        }
    }

    #endregion

}
