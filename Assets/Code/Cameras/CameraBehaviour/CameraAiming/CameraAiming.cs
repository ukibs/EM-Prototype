using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAiming : MonoBehaviour
{
    #region Private
    private Transform currentObjective;
    private SpringCamera sp;
    private InputManager inputManager;
    private CameraReference cr;
    private float transitionProg = 0.0f;
    #endregion
    
    #region Public
    
    public Transform player;
    public bool iAmCurrentCamera = false;
    public float pov = 5f;
    #endregion
    
    #region Properties
    public Transform CurrentObjective
    {
        get { return currentObjective; }
        set { currentObjective = value; }
    }
    
    #endregion
    
    #region MonoBehaviour

    private void Start()
    {
       sp = FindObjectOfType<SpringCamera>();
       cr = FindObjectOfType<CameraReference>();
       inputManager = FindObjectOfType<InputManager>();
       
    }

    private void Update()
    {
        currentObjective = cr.transform;
        ProcessCurrentObjective();
    }

    #endregion

    private void ProcessCurrentObjective()
    {
            float spd = 2.75f;
            // if this camera is the current objective.
            if (!currentObjective) return;
            // use camera reference's position to make an instant following.
            transform.position = cr.transform.position - cr.transform.forward * pov + Vector3.up * 3f;
            Quaternion rot = cr.transform.rotation;
            if (transitionProg < 1f)
            {
                rot = Quaternion.Lerp(transform.rotation, cr.transform.rotation, transitionProg / 0.75f);
                transitionProg += Time.deltaTime * spd;
            }
            else
            {
                transitionProg = 0f;
            }
            transform.rotation = rot;
    }
}
