using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawyBotRig : MonoBehaviour {

    //public LegPair[] legPairs;
    public Transform[] legs;
    public LegStatus[] legsStatuses;   // Un poco españolizado, pero me entiendo mejor
    public float legRotationSpeed = 10;
    public float legForce = 10;
    public float maxRotOffsetX = 20;
    public float maxRotOffsetY = 20;
    public float timeBetweenChanges = 0.5f;

    //
    private Rigidbody[] legsRGs;
    private HingeJoint[] legsHGs;
    private Transform[] postLegs;
    private Rigidbody[] postLegsRGs;
    private float originalLegRotX;
    private float originalLegRotY;
    //private Quaternion originalLegRot;
    private float timeFromLastChange;

    // Use this for initialization
    void Start()
    {
        //
        legsRGs = new Rigidbody[legs.Length];
        legsHGs = new HingeJoint[legs.Length];
        postLegs = new Transform[legs.Length];
        postLegsRGs = new Rigidbody[legs.Length];
        for (int i = 0; i < legs.Length; i++)
        {
            legsRGs[i] = legs[i].gameObject.GetComponent<Rigidbody>();
            //Debug.Log(legs[i].name);
            legsHGs[i] = legs[i].gameObject.GetComponent<HingeJoint>();
            postLegs[i] = legs[i].GetChild(0);
            postLegsRGs[i] = postLegs[i].gameObject.GetComponent<Rigidbody>();
        }
        //
        //legsStatuses = new LegStatus[legs.Length];
        //for (int i = 0; i < legs.Length; i += 2)
        //{
        //    legsStatuses[i] = LegStatus.GoingUp;
        //    legsStatuses[i + 1] = LegStatus.GoingDown;
        //}

        //
        originalLegRotX = legs[0].rotation.x;
        originalLegRotY = legs[0].rotation.y;
        //originalLegRot = legs[0].rotation;
        //
        //UpdateLegsStatuses();
        //
        SetHingeAxis();
    }

	// Update is called once per frame
	void Update ()
    {
        //
        float dt = Time.deltaTime;
        // De momento solo chequeamos la primera pata
        //float progressThisStep = legRotationSpeed * dt;
        //
        //CheckLegsStatuses(progressThisStep);
        
        //
        timeFromLastChange += dt;
        if(timeFromLastChange >= timeBetweenChanges)
        {
            //
            UpdateLegsStatuses();
            //
            SetHingeAxis();
            //
            timeFromLastChange -= timeBetweenChanges;
        }
        //
        //for (int i = 0; i < legs.Length; i++)
        //{
        //    Vector3 legEulers = legs[i].localEulerAngles;
        //    legEulers.z = 0;
        //    legs[i].localEulerAngles = legEulers;
        //}
    }

    //
    void CheckLegsStatuses(float progressThisStep)
    {
        // De momento chequeamos solo la primera
        switch (legsStatuses[0])
        {
            case LegStatus.GoingUp:
                if (Mathf.Abs((legs[0].rotation.x - progressThisStep) - originalLegRotX) > maxRotOffsetX)
                    UpdateLegsStatuses();
                break;
            case LegStatus.Advancing:
                if (Mathf.Abs((legs[0].rotation.y + progressThisStep) - originalLegRotY) > maxRotOffsetY)
                    UpdateLegsStatuses();
                break;
            case LegStatus.GoingDown:
                if (Mathf.Abs((legs[0].rotation.x + progressThisStep) - originalLegRotX) > maxRotOffsetX)
                    UpdateLegsStatuses();
                break;
            case LegStatus.Returning:
                if (Mathf.Abs((legs[0].rotation.y - progressThisStep) - originalLegRotY) > maxRotOffsetY)
                    UpdateLegsStatuses();
                break;
        }
    }

    //
    private void UpdateLegsStatuses()
    {
        //
        for (int i = 0; i < legs.Length; i++)
        {
            legsStatuses[i] = legsStatuses[i] != LegStatus.Returning ? legsStatuses[i] + 1 : LegStatus.GoingUp;
        }

    }

    void UpdateLegMovement()
    {
        //
        for (int i = 0; i < legs.Length; i++)
        {
            // Vamos a probar fuerza para compensar el peso
            //legsRGs[i].AddRelativeTorque(Vector3.right * legForce, ForceMode.Impulse);
            //legsRGs[i].angularVelocity = Vector3.right * legRotationSpeed * legForce;
            //
            switch (legsStatuses[i])
            {
                case LegStatus.GoingUp:
                    //legs[i].Rotate(-Vector3.right, progressThisStep);
                    legsRGs[i].angularVelocity = -Vector3.right * legRotationSpeed * legForce;
                    //legsRGs[i].AddRelativeTorque(-Vector3.right * legForce, ForceMode.Impulse);
                    Debug.Log("Leg " + i + ", angular veolicty" + legsRGs[i].angularVelocity);
                    break;
                case LegStatus.Advancing:
                    //legs[i].Rotate(Vector3.up, progressThisStep);
                    legsRGs[i].angularVelocity = Vector3.up * legRotationSpeed * legForce;
                    //legsRGs[i].AddRelativeTorque(Vector3.up * legForce, ForceMode.Impulse);
                    break;
                case LegStatus.GoingDown:
                    //legs[i].Rotate(Vector3.right, progressThisStep);
                    legsRGs[i].angularVelocity = Vector3.right * legRotationSpeed * legForce;
                    //legsRGs[i].AddRelativeTorque(Vector3.right * legForce, ForceMode.Impulse);
                    break;
                case LegStatus.Returning:
                    //legs[i].Rotate(-Vector3.up, progressThisStep);
                    legsRGs[i].angularVelocity = -Vector3.up * legRotationSpeed * legForce;
                    //legsRGs[i].AddRelativeTorque(-Vector3.up * legForce, ForceMode.Impulse);
                    break;
            }
        }
    }

    //
    void SetHingeAxis()
    {
        JointMotor jointMotor;
        for (int i = 0; i < legs.Length; i++)
        {
            //
            switch (legsStatuses[i])
            {
                case LegStatus.GoingUp:
                    legsHGs[i].axis = Vector3.right;
                    jointMotor = legsHGs[i].motor;
                    jointMotor.targetVelocity = -legRotationSpeed;
                    jointMotor.force = legForce;
                    legsHGs[i].motor = jointMotor;
                    break;
                case LegStatus.Advancing:
                    legsHGs[i].axis = Vector3.forward;
                    jointMotor = legsHGs[i].motor;
                    jointMotor.targetVelocity = legRotationSpeed;
                    jointMotor.force = legForce;
                    legsHGs[i].motor = jointMotor;
                    break;
                case LegStatus.GoingDown:   // Aqui hay que meter más pitera para que no se hunda
                    legsHGs[i].axis = Vector3.right;
                    jointMotor = legsHGs[i].motor;
                    jointMotor.targetVelocity = legRotationSpeed;
                    jointMotor.force = legForce * 2;
                    legsHGs[i].motor = jointMotor;
                    break;
                case LegStatus.Returning:
                    legsHGs[i].axis = Vector3.forward;
                    jointMotor = legsHGs[i].motor;
                    jointMotor.targetVelocity = -legRotationSpeed;
                    jointMotor.force = legForce;
                    legsHGs[i].motor = jointMotor;
                    break;
            }
        }
    }
}

// No lo usamos por ahora
//public class LegPair
//{
//    Transform leftLeg;
//    Transform rightLeg;
//}

//public enum LegStatus
//{
//    Invalid = -1,

//    GoingUp,
//    Advancing,
//    GoingDown,
//    Returning,

//    Count
//}