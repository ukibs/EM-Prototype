using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawyBot : MonoBehaviour {

    //public LegPair[] legPairs;
    public Transform[] legs;
    public LegStatus[] legsStatuses;   // Un poco españolizado, pero me entiendo mejor
    public float legRotationSpeed = 10;
    public float maxRotOffsetX = 20;
    public float maxRotOffsetY = 20;

    //
    private Transform[] postLegs;
    private float originalLegRotX;
    private float originalLegRotY;
    private Quaternion originalLegRot;

    // Use this for initialization
    void Start()
    {
        //
        postLegs = new Transform[legs.Length];
        for (int i = 0; i < legs.Length; i++)
        {
            postLegs[i] = legs[i].GetChild(0);
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
        originalLegRot = legs[0].rotation;
    }

	// Update is called once per frame
	void Update ()
    {
        //
        float dt = Time.deltaTime;
        // De momento solo chequeamos la primera pata
        float progressThisStep = legRotationSpeed * dt;
        // De momento chequeamos solo la primera
        switch (legsStatuses[0])
        {
            case LegStatus.GoingUp:
                if(Mathf.Abs((legs[0].rotation.x - progressThisStep) - originalLegRotX) > maxRotOffsetX)
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
        //
        for (int i = 0; i < legs.Length; i++)
        {
            switch (legsStatuses[i])
            {
                case LegStatus.GoingUp:
                    legs[i].Rotate(-Vector3.right, progressThisStep);
                    break;
                case LegStatus.Advancing:
                    legs[i].Rotate(Vector3.up, progressThisStep);
                    break;
                case LegStatus.GoingDown:
                    legs[i].Rotate(Vector3.right, progressThisStep);
                    break;
                case LegStatus.Returning:
                    legs[i].Rotate(-Vector3.up, progressThisStep);
                    break;
            }
        }

        // PROBAR: Usar joints para que las patas agarren
    }

    //
    private void UpdateLegsStatuses()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            legsStatuses[i] = legsStatuses[i] != LegStatus.Returning ? legsStatuses[i] + 1 : LegStatus.GoingUp;
        }
    }
}

// No lo usamos por ahora
public class LegPair
{
    Transform leftLeg;
    Transform rightLeg;
}

public enum LegStatus
{
    Invalid = -1,

    GoingUp,
    Advancing,
    GoingDown,
    Returning,

    Count
}