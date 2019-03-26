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

    #region Properties

    public bool IsOnFloor { get { return isOnFloor; } }

    #endregion

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        //dustEmitterStatic.SetActive(true);
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
        //float compensationOffset = 1 - (distanceFromFloor / idealDistanceFromFloor);
        //float compensationOffset = distanceFromFloor / idealDistanceFromFloor;
        // rb.AddForce(Vector3.up * repulsionStrength * compensationOffset);
        rb.AddForce(transform.up * repulsionStrength * compensationOffset);
    }

    void SoftenVerticalImpulse()
    {
        Vector3 currentVelocity = rb.velocity;
        currentVelocity.y *= 1 - repulsionDamp;
        //if (Mathf.Abs(currentVelocity.y) < 0.2) currentVelocity.y = 0;
        rb.velocity = currentVelocity;
    }

    void UpdateDustEmitter(Vector3 floorPoint)
    {
        //
        if (rb.velocity.sqrMagnitude <= 5.0f)
        {
            dustEmitterStatic.SetActive(true);
            dustEmitterMovement.SetActive(false);

            dustEmitterStatic.transform.position = floorPoint;
        }
        else
        {
            dustEmitterStatic.SetActive(false);
            dustEmitterMovement.SetActive(true);

            dustEmitterMovement.transform.position = floorPoint;
        }
    }

    #endregion

}
