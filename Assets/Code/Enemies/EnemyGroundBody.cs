using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Actions
{
    Invalid = -1,

    GoingToPlayer,
    EncirclingPlayer,
    FacingPlayer,

    Count
}

public class EnemyGroundBody : MonoBehaviour
{
    public float timeBetweenActionChecking = 1.0f;
    // TODO: Hacer una forma que podamos controlar la velocidad de los vehículos
    public float motorForce = 200.0f;
    [Tooltip("Rotatin in degrees per second.")]
    public float rotationSpeed = 90;
    public EnemyTurret[] turrets;   // TODO: QUe las busque él
    public EnemyWeapon[] weapons;   // TODO: Que la busque él
    public Actions[] behaviour;     // Luego trabajaremos bien esto

    private RobotControl player;
    private Rigidbody rb;

    private Actions currentAction = Actions.GoingToPlayer;
    private float timeFromLastCheck = 0;
    private bool touchingSomething;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<RobotControl>();
        rb = GetComponent<Rigidbody>();
        //
        turrets = GetComponentsInChildren<EnemyTurret>();
        weapons = GetComponentsInChildren<EnemyWeapon>();

        // Vamos a hacer que se ignoren las colisiones entre el vehículo y su torreta
        for(int i = 0; i < turrets.Length; i++)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), turrets[i].GetComponent<Collider>());
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        
        // Primero que el player siga vivo, si no mal
        if(player != null)
        {
            //
            timeFromLastCheck += dt;
            if(timeFromLastCheck > timeBetweenActionChecking)
            {
                //CheckActionToDo();
                DecideActionToDo();
                timeFromLastCheck -= timeBetweenActionChecking;
            }
            
            //
            Vector3 playerDirection = player.transform.position - transform.position;
            playerDirection.y = 0.0f;
            //
            switch (currentAction)
            {
                case Actions.FacingPlayer:
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
                    break;
                case Actions.GoingToPlayer:
                    //transform.rotation = Quaternion.LookRotation(playerDirection);
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, player.transform.position, rotationSpeed, dt);
                    GiveItGas();
                    break;
                case Actions.EncirclingPlayer:
                    Vector3 playerCross = Vector3.Cross(playerDirection, Vector3.up);
                    //transform.rotation = Quaternion.LookRotation(playerCross);
                    transform.rotation = GeneralFunctions.UpdateRotationInOneAxis(transform, playerCross, rotationSpeed, dt);
                    GiveItGas();
                    break;
            }
            
            // Damp para que no se desmadren
            //float dampForce = 10.0f;
            //rb.velocity = rb.velocity * ( 1 - dampForce * dt);
        }
    }

    //private void OnDrawGizmos()
    //{
    //    //
    //    if (player != null)
    //    {
    //        Debug.DrawRay(transform.position, transform.forward * 5, Color.blue);
    //        Vector3 playerDirection = player.transform.position - transform.position;
    //        Debug.DrawRay(transform.position, playerDirection, Color.red);
    //    }

    //}

    void GiveItGas()
    {
        // Y movemos con el rigidvody
        if (touchingSomething && HasGroundUnderneath())
            rb.AddForce(transform.forward * motorForce, ForceMode.Impulse);
        // Lo negamos hasta el próximo chqueo de colisón
        touchingSomething = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        touchingSomething = true;
    }

    #region Methods

    bool HasGroundUnderneath()
    {
        //
        if(Physics.Raycast(transform.position, -transform.up, 2f))
        {
            return true;
        }
        //
        return false;
    }

    /// <summary>
    /// Decide que acción del comportamiento utilizar
    /// </summary>
    void DecideActionToDo()
    {
        //
        for(int i = 0; i < behaviour.Length; i++)
        {
            switch (behaviour[i])
            {
                case Actions.EncirclingPlayer:
                    if (HasRemainingTurrets())
                    {
                        // Añadiremos también que le queden armas
                        currentAction = behaviour[i];
                        return;
                    }
                    break;
                case Actions.FacingPlayer:
                    if (HasRemainingTurrets())
                    {
                        // Añadiremos también que le queden armas
                        currentAction = behaviour[i];
                        return;
                    }
                    break;
                case Actions.GoingToPlayer:
                    // Esta no lleva condición
                    currentAction = behaviour[i];
                    return;
            }
        }
    }

    /// <summary>
    /// Ejecutar la acción elegida
    /// </summary>
    void CheckActionToDo()
    {
        // De momento aqui snecillo, luego nos curramos más la IA
        float distanceToPlayer = (transform.position - player.transform.position).magnitude; // Ya veremos si hacemos sqrt magnitude para ahorrar
        
        // Si está lo bastante cerca que corra a su alrededor
        // TODO: Que funcione con un parámetro
        if (HasRemainingTurrets() && distanceToPlayer < MainWeaponsMinRange())
        {
            currentAction = Actions.EncirclingPlayer;
        }
        // Si no que vaya hacia él
        else
        {
            currentAction = Actions.GoingToPlayer;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool HasRemainingTurrets()
    {
        bool hasReaminingTurrets = false;

        if(turrets.Length > 0)
        {
            for (int i = 0; i < turrets.Length; i++)
            {
                if (turrets[i] != null)
                {
                    hasReaminingTurrets = true;
                }
            }
        }
        
        return hasReaminingTurrets;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    float MainWeaponsMinRange()
    {
        float minRange = Mathf.Infinity;

        for (int i = 0; i < turrets.Length; i++)
        {
            if (turrets[i].MainWeaponsMinRange() < minRange)
            {
                minRange = turrets[i].MainWeaponsMinRange();
            }
        }

        return minRange;
    }

    //
    //void UpdateRotation(float dt)
    //{
    //    //First to know the direction
    //    Vector3 forward = transform.forward.normalized;
    //    Vector3 pointDirection = (player.transform.position - transform.position).normalized;
    //    float forwardAngle = Mathf.Atan2(forward.z, forward.x);
    //    float pDAnlge = Mathf.Atan2(pointDirection.z, pointDirection.x);
    //    float offset = (pDAnlge - forwardAngle) * Mathf.Rad2Deg;
        

    //    //A fix for when the number overflows the half circle
    //    if (Mathf.Abs(offset) > 180.0f)
    //    {
    //        offset -= 360.0f * Mathf.Sign(offset);
    //    }

    //    //And apply turning or check to move
    //    if (Mathf.Abs(offset) < rotationSpeed * dt)
    //    {
    //        transform.Rotate(0.0f, -offset, 0.0f);
    //    }
    //    else
    //    {
    //        transform.Rotate(0.0f, rotationSpeed * Mathf.Sign(-offset) * dt, 0.0f);
    //    }
    //}

    #endregion
}
