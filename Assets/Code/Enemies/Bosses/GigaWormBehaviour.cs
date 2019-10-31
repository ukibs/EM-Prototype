using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Comportamiento del Giga Gusano (Tuto)
/// Haremos uno más complejo más adelante
/// </summary>
public class GigaWormBehaviour : BossBaseBehaviour
{
    #region Status Enums

    public enum WormStatus
    {
        Invalid = -1,

        Wandering,
        Chasing,
        Stunned,
        Recovering,
        Dead,

        Count
    }

    private enum SubmersionStatus
    {
        Invalid = -1,

        Up,
        GoingDown,
        Down,
        GoingUp,

        Count
    }

    private enum MawStatus
    {
        Invalid = -1,

        Closed,
        Opening,
        Opened,
        Closing,

        Count
    }

    #endregion

    #region Public Attributes

    public float wanderingMovementSpeed = 10;
    public float chasingMovementSpeed = 10;
    public float rotationSpeed = 30;

    public float timeUnderground = 10;
    public float overGroundHeight = 0;
    public float underGroundHeight = -20;
    public float heightChangeSpeed = 5;

    public Transform head;
    public Transform[] maws;
    public float maxMawOpeningAngle = 45;
    public float mawSpeed = 180;

    //
    public int exteriorWeakPoints;
    public int interiorWeakPoints;

    //
    public float timeUntilCompleteStun = 2;
    public float completeStunRotation = 30;
    public float maxStunDuration = 20;

    //
    public Transform interiorEntrance;
    public Transform exitPoint;
    //
    public WeakPoint wormCore;

    //
    public AudioClip exteriorWeakPointDestroyedClip;
    public AudioClip allExteriorWeakPointDestroyedClip;
    public AudioClip stunnedClip;
    public AudioClip coreDamagedClip;
    public AudioClip deathClip;

    #endregion

    #region Private Attributes

    private WormStatus currentState = WormStatus.Wandering;
    //private RobotControl player;
    //private Rigidbody rb;
    //private ProvLevelManager levelManager;
    //private CarolBaseHelp carolHelp;
    //private CameraReference cameraReference;

    //
    private float currentTimeUnderground;
    private bool goesUnderground = false;

    //
    private float currentMawOpeningStatus;
    private MawStatus mawStatus = MawStatus.Closed;
    private bool firstTimeMawOpened = false;
    private bool firstBulletAttempAtMouth = false;
    private bool firstStun = false;
    private bool firstEntranceInMouth = false;

    // Usaremos esta varaible para ver si dos o más mandíbulas están tocando al player cuando cierra la boca
    private int mawsCollidingPlayer = 0;

    //
    private float currentStunDuration = 0;

    //
    private CrushingEsophagus[] crushingEsophaguses;
    private GigaWormInsides gigaWormInsides;

    #endregion

    #region Properties

    // Propiedades para la parte interna
    public Transform ExitPoint { get { return exitPoint; } }
    public bool Active { set { active = value; } }
    public WormStatus CurrentState { get { return currentState; } }

    #endregion

    // TODO: En colisiones entre cuerpos
    // Tratar la inclinación de los cuerpos
    // No es lo mismo un choque perpedicular que uno más directo
    // Seno?

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        //player = FindObjectOfType<RobotControl>();
        //Debug.Log("Player found? " + player);
        //rb = GetComponent<Rigidbody>();
        currentSpeed = wanderingMovementSpeed;
        //audioSource = GetComponent<AudioSource>();
        //levelManager = FindObjectOfType<ProvLevelManager>();
        //carolHelp = FindObjectOfType<CarolBaseHelp>();
        //cameraReference = FindObjectOfType<CameraReference>();
        //
        crushingEsophaguses = GetComponentsInChildren<CrushingEsophagus>();
        gigaWormInsides = FindObjectOfType<GigaWormInsides>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        //
        if (player == null)
            return;
        //
        base.Update();
        //
        float dt = Time.deltaTime;
        //
        Vector3 playerDirection = player.transform.position - transform.position;
        //
        UpdateBehaviour(playerDirection, dt);
        // Lo reseteamos a cada step
        mawsCollidingPlayer = 0;
        //
        //if((currentState == WormStatus.Stunned && !playerOut) ||
        //    (currentState == WormStatus.Recovering && !playerOut))
        //{
        //    InsidesDamageToPlayer(dt);
        //}

        // Aquí se recupera
        if(currentState == WormStatus.Stunned)
        {
            currentStunDuration += dt;
            if(currentStunDuration >= maxStunDuration)
            {
                currentState = WormStatus.Recovering;
                currentStunDuration = 0;
                gigaWormInsides.ChangeShowersEmission();
            }
        }
    }

    // Mandibulas
    private void OnCollisionStay(Collision collision)
    {
        // TODO: Esto ahora queda redundante
        // Quitarlo
        if(collision.GetContact(0).thisCollider.tag == "Maw")
            CheckMawsCollision(collision);
        else if(collision.GetContact(0).thisCollider.tag == "Esophagus")
        {
            // Un poco guarro
            for (int i = 0; i < crushingEsophaguses.Length; i++)
                crushingEsophaguses[i].CheckWallsCollision(collision);
        }

    }

    // Entrada de la boca
    private void OnTriggerEnter(Collider other)
    {
        // TODO: Chequear balas para primer entento
        Bullet bullet = other.GetComponent<Bullet>();
        if(bullet != null && currentState == WormStatus.Chasing && !firstBulletAttempAtMouth)
        {
            // Aquí sacaremos un mensaje de carol
            // TODO: Organizar mejor estas funciones
            //Debug.Log("Bullet shoot to mouth");
            firstBulletAttempAtMouth = true;
            carolHelp.TriggerIt(7, "Bullet shoot to mouth");
        }

        // Chequeamos que sea el player lo que colisiona
        PlayerIntegrity playerIntegrity = other.GetComponent<PlayerIntegrity>();
        //
        if(playerIntegrity != null)
        {
            //
            if (currentState == WormStatus.Stunned)
            {
                // Mandamos al player al interior del gusano
                MovePlayerToInterior();
                gigaWormInsides.PlayerOut = false;
                active = false;
                //
                if (!firstEntranceInMouth)
                {
                    firstEntranceInMouth = true;
                    carolHelp.TriggerIt(10, "Player entering mouth");
                }
            }
        }
        
    }

    protected void OnDrawGizmos()
    {
        //
        if (player != null)
        {
            //
            //Vector3 playerDirection = player.transform.position - transform.position;
            //
            Gizmos.DrawLine(transform.position, player.transform.position);
        }
    }

    protected void UpdateBehaviour(Vector3 playerDirection, float dt)
    {
        //
        switch (currentState)
        {
            // Cuando está vagando por ahí
            case WormStatus.Wandering:

                float playerDistance = (transform.position - player.transform.position).magnitude;

                // Que rote alrededor del player si se aleja demasiado
                // Así no se va a cuenca
                if (playerDistance > 700)
                {
                    // Sacar la cruz
                    Vector3 playerCross = Vector3.Cross(playerDirection, Vector3.up);
                    //transform.rotation = Quaternion.LookRotation(playerCross);
                    head.rotation = GeneralFunctions.UpdateRotationInOneAxis(head, playerCross, rotationSpeed, dt);
                }
                else
                {
                    head.Rotate(Vector3.up * dt);
                }

                // Velocity no sirve con kinematicos
                //rb.velocity = transform.forward * 100;
                //
                head.Translate(Vector3.forward * wanderingMovementSpeed * dt);
                //Debug.Log("I'm wandering");

                //
                if (goesUnderground)
                {
                    //
                    if (head.position.y > underGroundHeight)
                        head.Translate(Vector3.up * heightChangeSpeed * dt * -1);
                    //
                    currentTimeUnderground += dt;
                    if (currentTimeUnderground >= timeUnderground)
                    {
                        goesUnderground = false;

                    }

                }
                else if (!goesUnderground && head.position.y < overGroundHeight)
                {
                    head.Translate(Vector3.up * heightChangeSpeed * dt);
                }
                else if (exteriorWeakPoints == 0)
                {
                    // Aquí pasa a la fase de perseguir
                    currentState = WormStatus.Chasing;
                    currentSpeed = chasingMovementSpeed;
                    active = true;
                    rotationSpeed *= 3;
                    //Debug.Log("Start chasing");
                    // Por aqui em principio solo pasa una vez
                    carolHelp.TriggerIt(5, "Start chasing");
                }
                else
                {
                    // Del targeteable, hacer variable más clara
                    active = true;
                }
                break;
            case WormStatus.Chasing:
                // Que persiga al player
                head.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
                head.Translate(Vector3.forward * currentSpeed * dt);
                // Que abra la boca cuando lo tenga cerca
                UpdateMaw(playerDirection, dt);
                // Que intente atraparlo de un mordisco (muerte mortísima)
                // (En el script de collision del terreno de momento de momento)
                //      Que pase a estado stun
                break;
            case WormStatus.Stunned:
                // Vamos a hacer que vaya perdiendo velcodidad en un tiempo
                if(currentSpeed > 0)
                {
                    //
                    currentSpeed -= (chasingMovementSpeed / timeUntilCompleteStun) * dt;
                    currentSpeed = Mathf.Max(currentSpeed, 0);
                    //
                    head.Rotate(Vector3.forward * completeStunRotation / timeUntilCompleteStun * dt);
                    //
                    head.Translate(Vector3.forward * currentSpeed * dt);
                }
                break;
            case WormStatus.Recovering:

                // Basicamente invertiremos el stun
                if (currentSpeed < chasingMovementSpeed)
                {
                    // Le metemos un multiplicador al 
                    float recoveryMultiplier = 0.5f;
                    //
                    currentSpeed += (chasingMovementSpeed / timeUntilCompleteStun) * dt * recoveryMultiplier;
                    currentSpeed = Mathf.Min(currentSpeed, chasingMovementSpeed);
                    //
                    head.Rotate(-Vector3.forward * completeStunRotation / timeUntilCompleteStun * dt * recoveryMultiplier);
                }

                //
                head.Translate(Vector3.forward * currentSpeed * dt);
                // 
                if (currentSpeed == chasingMovementSpeed && gigaWormInsides.PlayerOut)
                {
                    currentState = WormStatus.Chasing;
                    //
                    //Vector3 currentHeadEulers = head.localEulerAngles;
                    //currentHeadEulers.z = 0;
                    //head.localEulerAngles = currentHeadEulers;
                }
                    
                break;
        }
    }

    // TODO: Funciona, pero hay que revisar el modelado de la mandíbula
    private void CheckMawsCollision(Collision collision)
    {
        //
        if (mawStatus != MawStatus.Closing)
            return;
        //
        PlayerIntegrity robotControl = collision.collider.GetComponent<PlayerIntegrity>();
        if(collision.GetContact(0).thisCollider.tag == "Maw" && robotControl != null)
        {
            mawsCollidingPlayer++;
        }
        //
        if (mawsCollidingPlayer >= 2)
            robotControl.Die();
    }

    //
    private void UpdateMaw(Vector3 playerDirection, float dt)
    {
        //
        float playerDistance = playerDirection.magnitude;
        // NOTA: Hardoceado. Sería bueno ponerlo parametrizable
        if(playerDistance > 130 && playerDistance < 250)
        {
            //
            switch (mawStatus)
            {
                case MawStatus.Closing:
                case MawStatus.Closed:
                    mawStatus = MawStatus.Opening;
                    // Chequeamos que lo haga por primera vez para la ayuda de carol
                    if (!firstTimeMawOpened)
                    {
                        carolHelp.TriggerIt(6, "First time maw opened");
                        firstTimeMawOpened = true;
                    }
                    break;
            }
        }
        //
        else if (playerDistance <= 130 || playerDistance >= 250)
        {
            //
            switch (mawStatus)
            {
                case MawStatus.Opening:
                case MawStatus.Opened:
                    mawStatus = MawStatus.Closing;
                    break;
            }
        }
        //
        //int rotationDirection = 0;
        switch (mawStatus)
        {
            case MawStatus.Opening:
                currentMawOpeningStatus -= mawSpeed * dt;
                currentMawOpeningStatus = Mathf.Max(currentMawOpeningStatus, -maxMawOpeningAngle);
                if (currentMawOpeningStatus <= -maxMawOpeningAngle)
                    mawStatus = MawStatus.Opened;
                //rotationDirection = -1;
                break;
            case MawStatus.Closing:
                currentMawOpeningStatus += mawSpeed * dt;
                currentMawOpeningStatus = Mathf.Min(currentMawOpeningStatus, 0);
                if (currentMawOpeningStatus >= maxMawOpeningAngle)
                    mawStatus = MawStatus.Closed;
                //rotationDirection = 1;
                break;
        }
        // Vamos a trabajar esto con euler ngles
        for(int i = 0; i < maws.Length; i++)
        {
            Vector3 mawRotation = maws[i].localEulerAngles;
            // Ñapaaaa
            // Mandibula vieja
            //switch (i)
            //{
            //    case 0: mawRotation.x = currentMawOpeningStatus; break;
            //    case 1: mawRotation.y = currentMawOpeningStatus; break;
            //    case 2: mawRotation.x = -currentMawOpeningStatus; break;
            //    case 3: mawRotation.y = -currentMawOpeningStatus; break;
            //}
            // Mandibula nueva
            switch (i)
            {
                case 0: mawRotation.x = currentMawOpeningStatus; break;
                case 1:
                    mawRotation.y = currentMawOpeningStatus;
                    break;
                case 2:
                    mawRotation.y = -currentMawOpeningStatus;
                    break;
            }

            maws[i].localEulerAngles = mawRotation;

            //maws[i].Rotate(transform.right * rotationDirection * mawSpeed * dt);
        }
    }

    // Llamada desde los weakpoints cuando son destruidos
    public override void LoseWeakPoint(string tag = "")
    {
        switch (currentState)
        {
            // Para la etapa inicial
            case WormStatus.Wandering:
                goesUnderground = true;
                currentTimeUnderground = 0;
                // Del targeteable, hacer variable más clara
                active = false;
                //
                exteriorWeakPoints--;
                if(interiorWeakPoints == 0)
                {
                    // Luego veremos si hacemos aqui el cambio al siguiente paso
                    //currentState
                    GeneralFunctions.PlaySoundEffect(audioSource, exteriorWeakPointDestroyedClip);
                }
                else
                {
                    GeneralFunctions.PlaySoundEffect(audioSource, allExteriorWeakPointDestroyedClip);
                }
                // Activamos los enemigos del grupo 1 (el 0 es el porpio boss)
                enemyManager.ActivateEnemies(1, transform.position + (Vector3.up * 100));
                break;
            // Para la final
            case WormStatus.Stunned:
            case WormStatus.Recovering:
                //
                currentState = WormStatus.Dead;
                GeneralFunctions.PlaySoundEffect(audioSource, deathClip);
                // TODO: Hacer la muerte y victoria
                levelManager.ConfirmVictory();
                break;
        }
    }

    // Para casos en que el weakpoint active algo al ser dañado en vez de destruido
    public override void RespondToDamagedWeakPoint(string tag = "")
    {
        if(currentState == WormStatus.Stunned)
        {
            currentState = WormStatus.Recovering;
            // Luego emtemos un par mas de cosas
            gigaWormInsides.ChangeShowersEmission();
        }
    }

    //
    public override void ImpactWithTerrain(bool hardEnough)
    {
        //
        Debug.Log("Maws impacting with terrain: " + mawStatus + ", " + hardEnough);
        //
        if(mawStatus != MawStatus.Closed && hardEnough)
        {
            currentState = WormStatus.Stunned;
            GeneralFunctions.PlaySoundEffect(audioSource, stunnedClip);
            gigaWormInsides.ChangeShowersEmission();
            //
            if (!firstStun)
            {
                firstStun = true;
                carolHelp.TriggerIt(9, "First stun");
            }
        }
    }

    //
    private void MovePlayerToInterior()
    {
        player.transform.position = interiorEntrance.position;
        player.transform.rotation = Quaternion.LookRotation(Vector3.back);
        cameraReference.transform.rotation = Quaternion.LookRotation(Vector3.back);
    }
    
}
