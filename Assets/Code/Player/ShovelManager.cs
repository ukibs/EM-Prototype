using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ShovelPostures
{
    Invalid = -1,

    Original,
    Sprint,
    Jump,
    PulseAttack,
    RapidFire,

    Canon,
    SphericDefense,
    Smash,
    Adherence,
    FrontalDefense,

    ParticleCascade,
    PiercingShot,

    Count
}

public class ShovelManager : MonoBehaviour {

    #region Public Attributes

    public float timeLerping = 0.2f;
    public Transform shovelParent;
    // TODO: Ir poniendo el resto
    [Tooltip("Positions order: 1.Original, 2.Sprint, 3.Jump, 4.PulseAttack, 5.RapidFire, " +
        "6.Canon, 7.SphericDefense, 8. Smash, 9. Adherence, 10. FrontalDefense, " +
        "10. Particle Cascade, 11. Piercing Shot")]
    public Transform[] shovelPositionsParents;

    #endregion


    #region Private Attributes

    private RobotControl robotControl;
    ShovelPostures previousShovelPosture;
    ShovelPostures currentShovelPosture;

    private Transform[] shovels;
    private List<Transform[]> shovelsPositions;

    private float timeFromLastChange = 0.0f;
    private float chargeAmount = 0;
    private float previousChargeAmount = 0;

    #endregion

    // Use this for initialization
    void Start () {
        //
        robotControl = FindObjectOfType<RobotControl>();
        previousShovelPosture = ShovelPostures.Original;
        currentShovelPosture = ShovelPostures.Original;
        //
        shovels = new Transform[shovelParent.childCount];
        shovelsPositions = new List<Transform[]>();
        //
        //if(sh)
        //
        shovelsPositions = new List<Transform[]>(shovelPositionsParents.Length);
        for (int i = 0; i < shovelPositionsParents.Length; i++)
        {
            shovelsPositions.Add(new Transform[4]);
        }
        //
        for (int i = 0; i < shovelParent.childCount; i++)
        {
            shovels[i] = shovelParent.GetChild(i);
            for (int j = 0; j < shovelPositionsParents.Length; j++)
            {
                // Chequeo para que no de error en cinemáticas
                if (shovelPositionsParents[j] != null)
                {
                    shovelsPositions[j][i] = shovelPositionsParents[j].GetChild(i);
                }
            }            
        }
		
	}
	
	// Update is called once per frame
	void Update () {

        //
        float dt = Time.deltaTime;
        //
        timeFromLastChange += dt;

        // Chequeo para que no de error en cinemáticas
        if (robotControl == null) return;

        //
        ActionCharguing playerActionCharging = robotControl.CurrentActionCharging;
        chargeAmount = robotControl.ChargedAmount;

        // First check what posture is the current
        if(chargeAmount > 0 && previousChargeAmount == 0)
        {
            switch (playerActionCharging)
            {
                case ActionCharguing.Sprint:
                    switch (robotControl.ActiveSprintMode)
                    {
                        case SprintMode.Normal:
                            currentShovelPosture = ShovelPostures.Sprint;
                            break;
                        // TODO: Hacer el de adeherencia
                        case SprintMode.Adherence:
                            currentShovelPosture = ShovelPostures.Adherence;
                            break;
                    }
                    
                    break;
                case ActionCharguing.Jump:
                    switch (robotControl.ActiveJumpMode)
                    {
                        case JumpMode.ChargedJump:
                            currentShovelPosture = ShovelPostures.Jump;
                            break;
                        case JumpMode.Smash:
                            // TODO: Hacer el de smash
                            currentShovelPosture = ShovelPostures.Smash;
                            break;
                    }
                    
                    break;
                case ActionCharguing.Attack:
                    switch (robotControl.ActiveAttackMode)
                    {
                        case AttackMode.Pulse:
                            currentShovelPosture = ShovelPostures.PulseAttack;
                            break;
                        case AttackMode.RapidFire:
                            currentShovelPosture = ShovelPostures.RapidFire;
                            break;
                        case AttackMode.Canon:
                            currentShovelPosture = ShovelPostures.Canon;
                            break;
                        case AttackMode.ParticleCascade:
                            currentShovelPosture = ShovelPostures.ParticleCascade;
                            break;
                    }
                    break;
                case ActionCharguing.Defense:
                    switch (robotControl.ActiveDefenseMode)
                    {
                        case DefenseMode.Spheric:
                            currentShovelPosture = ShovelPostures.SphericDefense;
                            break;
                        case DefenseMode.Front:
                            // TODO: Hacer el frontal
                            currentShovelPosture = ShovelPostures.FrontalDefense;
                            break;
                    }
                    break;
            }
            timeFromLastChange = 0;
        }
        else if(chargeAmount == 0)
        {
            currentShovelPosture = ShovelPostures.Original;
        }

        //
        previousChargeAmount = chargeAmount;

        //
        Transform[] shovelFrom = new Transform[4];
        shovelFrom = shovelsPositions[(int)previousShovelPosture];

        //
        Transform[] shovelTo = new Transform[4];
        shovelTo = shovelsPositions[(int)currentShovelPosture];

        //
        for (int i = 0; i < shovels.Length; i++)
        {
            //
            shovels[i].localRotation = Quaternion.Slerp(shovelFrom[i].localRotation, shovelTo[i].localRotation, timeFromLastChange / timeLerping);
            shovels[i].localPosition = Vector3.Lerp(shovelFrom[i].localPosition, shovelTo[i].localPosition, timeFromLastChange / timeLerping);
            //
            shovels[i].GetChild(0).localRotation =
                Quaternion.Slerp(shovelFrom[i].GetChild(0).localRotation, shovelTo[i].GetChild(0).localRotation, timeFromLastChange / timeLerping);
            shovels[i].GetChild(0).localPosition =
                Vector3.Lerp(shovelFrom[i].GetChild(0).localPosition, shovelTo[i].GetChild(0).localPosition, timeFromLastChange / timeLerping);
        }
	}

    private void OnGUI()
    {
        //GUI.Label(new Rect(20, Screen.height - 30, 200, 20), previousChargeAmount + ", " + chargeAmount);
        //GUI.Label(new Rect(20, Screen.height - 30, 200, 20), previousShovelPosture + ", " + currentShovelPosture);
    }
}
