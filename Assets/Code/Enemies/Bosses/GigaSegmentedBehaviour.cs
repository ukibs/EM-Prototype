﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GigaSegmentedBehaviour : BossBaseBehaviour
{

    public enum Status
    {
        Invalid = -1,

        Wandering,
        Sprinting,

        Count
    }

    #region Public Attributes

    public float startSpeed = 30;
    public float verticaSpeed = 10;
    public float sprintSpeed = 60;
    public float startHeight = 75;
    public float rotationSpeed = 30;
    public float minHeight = 50;
    public float maxHeight = 200;
    public float sprintDuration = 10;

    public Material headMaterial;

    #endregion

    #region Private Attributes

    private GameObject previousSegment;
    private GameObject posteriorSegment;

    private BodyPart bodyPartBehaviour;
    private GigaSegmentedBehaviour previousSegmentBehaviour;
    private GigaSegmentedBehaviour posteriorSegmentBehaviour;

    private float currentDesiredHeight;
    private float sprintCurrentDuration;

    private Status currentStatus;

    private bool headAssigned = false;

    #endregion

    #region Properties

    public bool IsActiveHead { get { return previousSegment == null; } }
    public GigaSegmentedBehaviour HeadBehaviour
    {
        get
        {
            if (IsActiveHead)
            {
                return this;
            }
            else
            {
                return previousSegmentBehaviour;
            }
        }
        
    }

    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        //
        transform.position += Vector3.up * startHeight;
        //
        GetPreviousAndPosteriorSegements();
        //
        base.Start();
        //
        currentSpeed = startSpeed;
        //
        DecideNewHeight();
    }

    // Update is called once per frame
    protected override void Update()
    {
        // Vamos a hacer que la coja aqui una vez al empezar. Ya que parece que no está todavía bien asignado en el start
        if (!headAssigned)
        {
            headAssigned = true;
            if(!IsActiveHead)
                bodyPartBehaviour.bossBehaviour = HeadBehaviour;
        }

            
        //
        base.Update();
        //
        float dt = Time.deltaTime;
        //
        UpdateHeadBehaviour(dt);
        
    }

    #region Methods

    private void GetPreviousAndPosteriorSegements()
    {
        int index = transform.GetSiblingIndex();
        //
        if (index == 0)
        {
            // En este caso es la cabeza activa
            GetHeadMaterial();
        }
        else
        {
            previousSegment = transform.parent.GetChild(index - 1).gameObject;
            //
            previousSegmentBehaviour = previousSegment.GetComponent<GigaSegmentedBehaviour>();
            // TODO: Revisar esta parte
            bodyPartBehaviour = gameObject.AddComponent<BodyPart>();
            bodyPartBehaviour.previousBodyPart = previousSegment.transform;
            bodyPartBehaviour.bossBehaviour = HeadBehaviour;
        }
            
        //
        if (index == transform.parent.childCount - 1)
        {
            // En este caso es la cola, desactivamos el weakpoint de conexion
            transform.GetChild(2).gameObject.SetActive(false);
        }
        else
        {
            posteriorSegment = transform.parent.GetChild(index + 1).gameObject;
            //
            posteriorSegmentBehaviour = posteriorSegment.GetComponent<GigaSegmentedBehaviour>();
        }
            
    }

    private void UpdateHeadBehaviour(float dt)
    {
        if (IsActiveHead)
        {
            //
            float heightOffset = currentDesiredHeight - transform.position.y;
            Vector3 verticalMovement = Vector3.up * Mathf.Sign(heightOffset) * verticaSpeed;
            //
            if (Mathf.Abs(heightOffset) < 5) DecideNewHeight();
            // Manejamos el vertical movement un poco aparte por si le ponemos una velocidad propia
            transform.Translate(((Vector3.forward * currentSpeed) + (verticalMovement)) * dt);
            //transform.Rotate(Vector3.up * 1 * dt);
            transform.rotation = GeneralFunctions.UpdateRotationOnCross(transform, player.transform.position, rotationSpeed, dt);
            //
            if (currentStatus == Status.Sprinting)
            {
                sprintCurrentDuration += dt;
                if (sprintCurrentDuration >= sprintDuration)
                {
                    StopSprint();
                }
            }
        }
    }

    public void StartSprint()
    {
        //
        if (IsActiveHead)
        {
            currentSpeed = sprintSpeed;
            sprintCurrentDuration = 0;
            currentStatus = Status.Sprinting;
            Debug.Log("Starting sprint");
        }
        else
        {
            previousSegmentBehaviour.StartSprint();
        }
    }

    private void StopSprint()
    {
        currentSpeed = startSpeed;
        currentStatus = Status.Wandering;
        Debug.Log("Finishing sprint");
    }

    private void DecideNewHeight()
    {
        currentDesiredHeight = Random.Range(minHeight, maxHeight);
        //Debug.Log("New desired height: " + currentDesiredHeight);
    }

    public void LoseConnectionWithPrev()
    {
        //
        Destroy(bodyPartBehaviour);
        previousSegment = null;
        //
        GetHeadMaterial();
        // And tell your previous ones to reasign head reference
        posteriorSegmentBehaviour.ReassignHead(this);
    }

    public void GetHeadMaterial()
    {
        Transform chasis = transform.GetChild(3);
        //
        for (int i = 0; i < chasis.childCount; i++)
        {
            chasis.GetChild(i).GetComponent<MeshRenderer>().material = headMaterial;
        }
    }

    public void ReassignHead(GigaSegmentedBehaviour gigaSegmentedBehaviour)
    {
        bodyPartBehaviour.bossBehaviour = gigaSegmentedBehaviour;
        //
        if(posteriorSegmentBehaviour != null)
        {
            posteriorSegmentBehaviour.ReassignHead(gigaSegmentedBehaviour);
        }
    }

    public override void LoseWeakPoint(string tag = "")
    {
        Debug.Log("Destuction tag: " + tag);
        //
        if (tag.Equals("Connection"))
        {
            posteriorSegmentBehaviour.LoseConnectionWithPrev();
            // TODO: Manejar aqui reaisgnacion en body part
            HeadBehaviour.StartSprint();
        }
        else if (tag.Equals("Generator"))
        {
            StartSprint();
        }
    }

    public override void RespondToDamagedWeakPoint(string tag = "") { }

    public override void ImpactWithTerrain(bool hardEnough) { }

    #endregion
}
