﻿using System.Collections;
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

public class EnemyBodyBehaviour : MonoBehaviour
{

    public float timeBetweenActionChecking = 1.0f;
    // TODO: Hacer una forma que podamos controlar la velocidad de los vehículos
    public float motorForce = 200.0f;
    [Tooltip("Rotatin in degrees per second.")]
    public float rotationSpeed = 90;
    public EnemyTurret[] turrets;   // TODO: QUe las busque él
    public EnemyWeapon[] weapons;   // TODO: Que la busque él
    public Actions[] behaviour;     // Luego trabajaremos bien esto

    protected RobotControl player;
    protected Rigidbody rb;

    protected Actions currentAction = Actions.GoingToPlayer;
    protected float timeFromLastCheck = 0;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        player = FindObjectOfType<RobotControl>();
        rb = GetComponent<Rigidbody>();
        // TODO: Revisar porque no las pilla
        //turrets = GetComponentsInChildren<EnemyTurret>();
        //weapons = GetComponentsInChildren<EnemyWeapon>();

        // Vamos a hacer que se ignoren las colisiones entre el vehículo y su torreta
        // TODO: Ver como hacerlo con las torretas que tienen coliders como hijos
        for (int i = 0; i < turrets.Length; i++)
        {
            // Torretas con un collider (en la propia torreta)
            Collider turretCollider = GetComponent<Collider>();
            if(turretCollider != null)
                Physics.IgnoreCollision(turretCollider, turrets[i].GetComponent<Collider>());
            // Torreteas con varios colliders (en los hijos)
            // TODO: Hacerlo
        }

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //
        float dt = Time.deltaTime;

        // Primero que el player siga vivo, si no mal
        if (player != null)
        {
            //
            timeFromLastCheck += dt;
            if (timeFromLastCheck > timeBetweenActionChecking)
            {
                CheckActionToDo();
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

        //
    protected virtual void GiveItGas()
    {
        
    }

    /// <summary>
    /// Decide que acción del comportamiento utilizar
    /// </summary>
    void DecideActionToDo()
    {
        //
        for (int i = 0; i < behaviour.Length; i++)
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

        if (turrets.Length > 0)
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
}
