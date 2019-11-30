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

    public enum AttackState
    {
        Invalid = -1,

        Lifting,
        Cooldown,

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

    // Attack variables
    public float maxLiftForcePerSegment = 100;
    public float liftDuration = 5;
    public float attackCooldown = 5;

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

    //
    private AttackState currentAttackState = AttackState.Cooldown;
    private List<LiftedObject> liftedObjects;
    private float accumulatedLiftMass;
    private float currentAttackCooldown;
    private float currentLiftDuration;
    private List<GameObject> liftedObjectsTrails;

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
                //Debug.Log(previousSegmentBehaviour.gameObject);
                return previousSegmentBehaviour.HeadBehaviour;
            }
        }
        
    }

    public float TotalLiftForce
    {
        get
        {
            //
            float accumulatedLiftForce = maxLiftForcePerSegment;
            //
            if (posteriorSegmentBehaviour != null)
                accumulatedLiftForce += posteriorSegmentBehaviour.TotalLiftForce;
            //
            return accumulatedLiftForce;
        }
    }

    #endregion

    #region Unity Methods

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
        //
        liftedObjects = new List<LiftedObject>(10);
        InitiateLiftedObjectsTrails();
    }

    // Update is called once per frame
    protected override void Update()
    {
        // Vamos a hacer que la coja aqui una vez al empezar. Ya que parece que no está todavía bien asignado en el start
        if (!headAssigned)
        {
            headAssigned = true;
            if (!IsActiveHead)
                bodyPartBehaviour.bossBehaviour = HeadBehaviour;
        }

            
        //
        base.Update();
        //
        float dt = Time.deltaTime;
        //
        UpdateHeadBehaviour(dt);
        
    }

    private void OnDrawGizmos()
    {
        //
        //if (previousSegment)
        //{
        //    Vector3 previousDirection = previousSegment.transform.position - transform.position;
        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawLine(transform.position, previousSegment.transform.position);
        //    Gizmos.DrawLine(transform.position, transform.position + (Vector3.up * 100));
        //}
        //
        if (IsActiveHead && currentAttackState == AttackState.Lifting)
        {
            //
            for(int i = 0; i < liftedObjects.Count; i++)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(liftedObjects[i].initialPosition, liftedObjects[i].finalPosition);
            }
        }
    }

    #endregion

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
            // Actualizamos behaviour de ataque
            // TODO: Poner algo aqui para que empiece ignorando a EM
            if (true)
            {
                switch (currentAttackState)
                {
                    case AttackState.Cooldown:
                        currentAttackCooldown += dt;
                        if(currentAttackCooldown >= attackCooldown)
                        {
                            currentAttackCooldown = 0;
                            GetRigidbodiesToLaunch();
                            currentAttackState = AttackState.Lifting;
                            //
                            //Debug.Log("Start lifting");
                        }
                        break;

                    case AttackState.Lifting:
                        currentLiftDuration += dt;
                        
                        if(currentLiftDuration >= liftDuration)
                        {
                            currentLiftDuration = 0;
                            // fsvfsdsd
                            currentAttackState = AttackState.Cooldown;
                            //
                            ThrowBodies(dt);
                            //
                            //Debug.Log("End lifting");
                        }
                        else
                        {
                            UpdateLifting();
                        }
                        break;
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
            //Debug.Log("Starting sprint");
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
        //Debug.Log("Finishing sprint");
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
    
    #endregion

    #region Attack Methods

    private void InitiateLiftedObjectsTrails()
    {
        //
        int provisionalSize = 100;
        //
        liftedObjectsTrails = new List<GameObject>(provisionalSize);
        //
        for (int i = 0; i < liftedObjectsTrails.Capacity; i++)
        {
            GameObject newLiftedObjectTrail = Instantiate(carolHelp.dangerousProyetilesTrailPrefab);
            liftedObjectsTrails.Add(newLiftedObjectTrail);
        }
    }

    private void GetRigidbodiesToLaunch()
    {
        // Recordar que la lista de objetos enganchados tendrá que estar limpia
        liftedObjects.Clear();
        // TODO: Mover al radio a variable pública
        Collider[] possibleBodies = Physics.OverlapSphere(transform.position, 750);
        //Debug.Log("" + possibleBodies.Length + " possible bodies");
        //
        for (int i = 0; i < possibleBodies.Length; i++)
        {
            // Primero que tenga esta tag
            if (possibleBodies[i].tag == "Hard Terrain")
            {
                // Luego que tenga rigidbody
                Rigidbody possibleRb = possibleBodies[i].GetComponent<Rigidbody>();
                if (possibleRb != null)
                {
                    // 
                    accumulatedLiftMass += possibleRb.mass;
                    //
                    LiftedObject newLiftedObject = new LiftedObject(possibleRb, possibleRb.position, possibleRb.position + (Vector3.up * 200));
                    //
                    liftedObjects.Add(newLiftedObject);
                    //
                    if (accumulatedLiftMass >= TotalLiftForce)
                    {
                        //
                        Debug.Log("Lifting " + liftedObjects.Count + " objects with a total accumulated mass of" + accumulatedLiftMass);
                        accumulatedLiftMass = 0;
                        return;
                    }
                }
            }

        }
        
    }

    private void UpdateLifting()
    {
        for (int i = 0; i < liftedObjects.Count; i++)
        {
            liftedObjects[i].liftedRb.position = 
                Vector3.Lerp(liftedObjects[i].initialPosition, liftedObjects[i].finalPosition, currentLiftDuration / liftDuration);
        }
    }

    private void ThrowBodies(float dt)
    {
        //
        float desiredProyectileSpeed = 500;
        // Inital approach
        for (int i = 0; i < liftedObjects.Count; i++)
        {
            //
            Vector3 playerStimatedPosition = GeneralFunctions.AnticipateObjectivePositionForAiming(
                liftedObjects[i].liftedRb.position, 
                player.transform.position, 
                PlayerReference.playerRb.velocity, desiredProyectileSpeed, dt);
            playerStimatedPosition.y += GeneralFunctions.GetProyectileFallToObjective(liftedObjects[i].liftedRb.position,
                player.transform.position, desiredProyectileSpeed);
            //
            Vector3 playerStimatedDirection = player.transform.position - liftedObjects[i].liftedRb.position;

            // TODO: HAcer que Carol o el bullet pool pinte las trayectorias
            if (i < liftedObjectsTrails.Count)
                AllocateTrayectoryLineRenderer(liftedObjects[i].liftedRb, liftedObjectsTrails[i],
                    playerStimatedDirection.normalized * desiredProyectileSpeed);
            else
                Debug.Log("More bodies than expected");
            // De momento le asignamos la velicdad a palo seco
            liftedObjects[i].liftedRb.AddForce(playerStimatedDirection.normalized * desiredProyectileSpeed, ForceMode.VelocityChange);
            //
            carolHelp.TriggerGeneralAdvice("GreatDangerIncoming");
        }
    }

    // Versión del line renderer de trayectoria para los fragemtos que arroja
    public void AllocateTrayectoryLineRenderer(Rigidbody liftedBody, GameObject lineClone, Vector3 bodySpeed)
    {
        //
        float hardCodedFlyTime = 10;
        float hardCodedDrag = 0.1f;
        //
        float timePerTic = 0.5f;
        int stepsToCheck = (int)(hardCodedFlyTime / timePerTic);
        Vector3[] positions = new Vector3[stepsToCheck];
        //
        float anticipatedDragEffect = 1 - (hardCodedDrag * timePerTic);
        float speedInStep = bodySpeed.magnitude;
        //
        positions[0] = liftedBody.position;

        //
        for (int i = 1; i < stepsToCheck; i++)
        {
            // TODO: Habrá que tener en cuenta el drag
            // Creo
            //GeneralFunctions.AnticipateObjectivePositionForAiming();
            //GeneralFunctions.GetVelocityWithDistanceAndDrag(rb.velocity.magnitude, , rb.drag, rb.mass);
            float fallInThatTime = Physics.gravity.y * Mathf.Pow(timePerTic * i, 2) / 2;
            //
            speedInStep = speedInStep * anticipatedDragEffect;
            //
            positions[i] = liftedBody.position + (bodySpeed.normalized * speedInStep * timePerTic * i) + new Vector3(0, fallInThatTime, 0);
            //positions[i] = positions[i-1] + (rb.velocity.normalized * speedInStep * timePerTic) + new Vector3(0, fallInThatTime, 0);
        }
        //
        if (lineClone)
        {
            //
            LineRenderer lineRenderer = lineClone.GetComponent<LineRenderer>();
            //
            lineRenderer.positionCount = stepsToCheck;
            lineRenderer.SetPositions(positions);
        }
        else
        {
            Debug.Log("Trying to allocate non-existant line renderer");
        }

    }

    #endregion

    #region BaseBossBehaviour

    public override void LoseWeakPoint(string tag = "")
    {
        Debug.Log("Destuction tag: " + tag);
        //
        if (tag.Equals("Connection"))
        {
            posteriorSegmentBehaviour.LoseConnectionWithPrev();
            // TODO: Manejar aqui reaisgnacion en body part
            HeadBehaviour.StartSprint();
            //
            enemyManager.ActivateEnemies(2, transform.position);
        }
        else if (tag.Equals("Generator"))
        {
            StartSprint();
            //
            enemyManager.ActivateEnemies(1, transform.position);
        }
    }

    public override void RespondToDamagedWeakPoint(string tag = "") { }

    public override void ImpactWithTerrain(bool hardEnough) { }

    #endregion
}

public class LiftedObject
{
    public Rigidbody liftedRb;
    public Vector3 initialPosition;
    public Vector3 finalPosition;

    public LiftedObject(Rigidbody liftedRb, Vector3 initialPosition, Vector3 finalPosition)
    {
        this.liftedRb = liftedRb;
        this.initialPosition = initialPosition;
        this.finalPosition = finalPosition;
    }
}