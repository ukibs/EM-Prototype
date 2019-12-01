using System;
using System.Collections;
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

    private Rigidbody rb;

    //
    private AttackState currentAttackState = AttackState.Cooldown;
    private List<LiftedObject> liftedObjects;
    private float accumulatedLiftMass;
    private float currentAttackCooldown;
    private float currentLiftDuration;
    private List<GameObject> liftedObjectsTrails;

    private float currentLiftForce;

    private bool isAlive = true;

    private List<Vector3> preLauchPositions;

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
            float accumulatedLiftForce = currentLiftForce;
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
        //
        currentLiftForce = maxLiftForcePerSegment;
        //
        rb = GetComponent<Rigidbody>();
        //
        InitializePreLaunchPositionsListMatrix();
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
        //if (IsActiveHead && currentAttackState == AttackState.Lifting)
        //{
        //    //
        //    for(int i = 0; i < liftedObjects.Count; i++)
        //    {
        //        Gizmos.color = Color.blue;
        //        Gizmos.DrawLine(liftedObjects[i].initialPosition, liftedObjects[i].finalPosition);
        //    }
        //}

        //
        for(int i = 0; i < preLauchPositions.Count - 1; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(preLauchPositions[i], preLauchPositions[i+1]);
        }
    }

    #endregion

    #region Methods

    private void GetPreviousAndPosteriorSegements()
    {
        int index = transform.GetSiblingIndex();
        // Debug.Log("Sibling index: " + index);

        // Chequeo de parte previa
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
            
        // Chequeo de parte posterior
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
        //
        if (!isAlive) return;
        //
        if (IsActiveHead)
        {
            //
            Vector3 playerOffset = player.transform.position - transform.position;
            //
            float heightOffset = currentDesiredHeight - transform.position.y;
            Vector3 verticalMovement = Vector3.up * Mathf.Sign(heightOffset) * verticaSpeed;
            //
            if (Mathf.Abs(heightOffset) < 5) DecideNewHeight();
            // Manejamos el vertical movement un poco aparte por si le ponemos una velocidad propia
            transform.Translate(((Vector3.forward * currentSpeed) + (verticalMovement)) * dt);

            // Según la distancia al player que lo rodee o vaya hacie él
            // TODO: Deshardcodearlo
            if (playerOffset.magnitude < 500)
                transform.rotation = GeneralFunctions.UpdateRotationOnCross(transform, player.transform.position, rotationSpeed, dt);
            else
                transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);

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
        currentDesiredHeight = UnityEngine.Random.Range(minHeight, maxHeight);
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

    private void CheckDeath()
    {
        //
        Debug.Log("Checking death -> Current lift force: " + currentLiftForce + ", Previous Segment Behaviour: " + previousSegmentBehaviour +
            ", Posterior Segment Behaviour: " + posteriorSegmentBehaviour);
        //
        if(currentLiftForce <= 50 && previousSegmentBehaviour == null && posteriorSegmentBehaviour == null)
        {
            // Aquí muere el segmento

            // Primero rigidbody de cadaver
            rb.isKinematic = false;
            rb.useGravity = true;

            // Gestión de layer
            //int newLayer = 
            //gameObject.layer = 

            //
            isAlive = false;
            Debug.Log("Segment dead");

            // Y finalmente gestionamos la condición de victoria

        }
    }
    
    #endregion

    #region Attack Methods

    private void InitializePreLaunchPositionsListSpiral()
    {
        //
        preLauchPositions = new List<Vector3>();
        //
        float a = 1.1f;    // Constante
        //float e;    // Vamos a decir que es i
        float k = 3;    // Constante
        float fi = 30;   // Angulo
        // r = a * Mathf.Pow(e, k * fi);
        //
        for (int i = 0; i < 300; i++)
        {
            //
            float angle = i * fi;
            float radius = a * Mathf.Pow(i, k * fi * Mathf.Deg2Rad);
            //
            float xPosition = radius * Mathf.Cos(angle);
            float yPosition = radius * Mathf.Sin(angle);
            //
            //preLauchPositions[i] = new Vector3(xPosition, yPosition, 0);
            preLauchPositions.Add(new Vector3(xPosition, yPosition, 0));
        }
    }

    private void InitializePreLaunchPositionsListMatrix()
    {
        //
        preLauchPositions = new List<Vector3>();
        //
        int rowLength = 2;
        int columnLength = 2;
        float spaceBetweenPositions = 2;
        int rowIndex = 0;
        int columnIndex = 0;
        //
        for (int i = 0; i < 300; i++)
        {
            //

            //
            float xPosition = columnIndex * spaceBetweenPositions;
            float yPosition = rowIndex * spaceBetweenPositions;
            //
            if (columnIndex == rowIndex) columnIndex++;
            else rowIndex++;
            //
            if (columnIndex == columnLength - 1 && rowIndex == rowLength - 1)
            {
                columnLength++; rowLength++;
                columnIndex++;
                rowIndex = 0;
            }
            else if(columnIndex == columnLength - 2)
            {
                if(rowIndex < rowLength - 1)
                {
                    rowIndex++;
                    columnIndex--;
                }
                else
                {
                    columnIndex = 0;
                    columnLength++;
                }
                
            }
            //
            //preLauchPositions[i] = new Vector3(xPosition, yPosition, 0);
            preLauchPositions.Add(new Vector3(xPosition, yPosition, 0));
        }
    }

    

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
                    LiftedObject newLiftedObject = new LiftedObject(possibleRb, possibleRb.position, possibleRb.position + (Vector3.up * 500));
                    //
                    liftedObjects.Add(newLiftedObject);
                    //
                    if (accumulatedLiftMass >= TotalLiftForce)
                    {
                        //
                        //Debug.Log("Lifting " + liftedObjects.Count + " objects with a total accumulated mass of" + accumulatedLiftMass);
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
            // Ñapa para cuando se salen del array de posiciones
            Vector3 finalPosition;
            if (i < preLauchPositions.Count) finalPosition = liftedObjects[0].finalPosition + preLauchPositions[i];
            else finalPosition = liftedObjects[i].finalPosition;
            //
            liftedObjects[i].liftedRb.position =
                Vector3.Lerp(liftedObjects[i].initialPosition,
                //liftedObjects[0].liftedRb.transform.TransformPoint(preLauchPositions[i]), 
                liftedObjects[0].finalPosition + preLauchPositions[i],
                Mathf.Sqrt(currentLiftDuration / liftDuration));
            //
            liftedObjects[i].liftedRb.transform.LookAt(player.transform.position);
            // Ñapa para las "costillas"
            if(liftedObjects[i].liftedRb.mass == 1)
            {
                liftedObjects[i].liftedRb.transform.rotation *= Quaternion.Euler(Vector3.up * 90);
            }

        }
    }

    private void ThrowBodies(float dt)
    {
        //
        float desiredProyectileSpeed = 500;
        //
        //
        //Vector3 playerStimatedPosition = GeneralFunctions.AnticipateObjectivePositionForAiming(
        //    liftedObjects[0].liftedRb.position,
        //    player.transform.position,
        //    PlayerReference.playerRb.velocity, desiredProyectileSpeed, dt);
        //playerStimatedPosition.y += GeneralFunctions.GetProyectileFallToObjective(liftedObjects[0].liftedRb.position,
        //    player.transform.position, desiredProyectileSpeed);
        //
        //Vector3 playerStimatedDirection = player.transform.position - liftedObjects[0].liftedRb.position;
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

        //
        StartCoroutine(CountdownToHideTrails());
    }

    IEnumerator CountdownToHideTrails()
    {
        yield return new WaitForSeconds(2);
        HideLineRenderers();
    }

    /// <summary>
    /// 
    /// </summary>
    void HideLineRenderers()
    {
        //
        for(int i = 0; i < liftedObjectsTrails.Count; i++)
        {
            //
            LineRenderer lineRenderer = liftedObjectsTrails[i].GetComponent<LineRenderer>();
            //
            if (lineRenderer.positionCount > 0)
            {
                lineRenderer.positionCount = 0;
            }
            else
            {
                return;
            }
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
            posteriorSegmentBehaviour = null;
            posteriorSegment = null;
            //
            CheckDeath();
            // TODO: Manejar aqui reaisgnacion en body part
            HeadBehaviour.StartSprint();
            //
            enemyManager.ActivateEnemies(2, transform.position);
            
        }
        else if (tag.Equals("Generator"))
        {
            // De momento trabajamos con este valor
            currentLiftForce -= 25;
            //
            CheckDeath();
            //
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