using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MekanoidBodyBehaviour : EnemyBaseBodyBehaviour
{
    // TODO: Hacer una forma que podamos controlar la velocidad de los vehículos
    public float motorForce = 200.0f;
    public EnemyTurret[] turrets;   // TODO: QUe las busque él
    public EnemyWeapon[] weapons;   // TODO: Que la busque él

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        // TODO: Revisar porque no las pilla
        //turrets = GetComponentsInChildren<EnemyTurret>();
        //weapons = GetComponentsInChildren<EnemyWeapon>();

        //
        PutIgnoreCollisionInTurrets();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected void PutIgnoreCollisionInTurrets()
    {
        // Vamos a hacer que se ignoren las colisiones entre el vehículo y su torreta
        // TODO: Ver como hacerlo con las torretas que tienen coliders como hijos
        for (int i = 0; i < turrets.Length; i++)
        {
            // Torretas con un collider (en la propia torreta)
            Collider turretCollider = GetComponent<Collider>();
            if (turretCollider != null)
                Physics.IgnoreCollision(turretCollider, turrets[i].GetComponent<Collider>());
            // Torreteas con varios colliders (en los hijos)
            // TODO: Hacerlo
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected bool HasRemainingTurrets()
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
    /// Decide que acción del comportamiento utilizar
    /// </summary>
    protected override void DecideActionToDo()
    {
        //
        for (int i = 0; i < behaviour.Length; i++)
        {
            switch (behaviour[i])
            {
                case Actions.EncirclingPlayer:
                    Vector3 playerDistance = player.transform.position - transform.position;
                    if (HasRemainingTurrets() && playerDistance.magnitude < MainWeaponsMinRange())
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
    protected override void CheckActionToDo()
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
    protected float MainWeaponsMinRange()
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
    protected virtual void Move()
    {
        GiveItGas();
    }
}
