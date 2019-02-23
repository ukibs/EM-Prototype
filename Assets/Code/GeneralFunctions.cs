using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneralFunctions
{
    //
    // Nota: Antes o después trabajaremos con una pool
    public static void ShootProjectile(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 direction, float forceToApply, 
        float dt, ShootCalculation shootCalculation = ShootCalculation.Force, float proyectileMass = -1)
    {
        
        Rigidbody prefabRb = prefab.GetComponent<Rigidbody>();
        // Get the muzzle speed in meters/second (rememeber 1 is a Ton)
        float bulletMuzzleSpeed;
        if (shootCalculation == ShootCalculation.Force)
            bulletMuzzleSpeed = prefabRb.mass * 1000 / forceToApply;
        // TODO: Esto es una ñapa, hay que arreglarlo
        else
            bulletMuzzleSpeed = forceToApply;
        //
        GameObject newBullet = GameObject.Instantiate(prefab, position, rotation);
        Rigidbody newBulletRB = newBullet.GetComponent<Rigidbody>();
        Vector3 directionWithForce = direction.normalized * forceToApply;
        //
        if(proyectileMass != -1)
        {
            newBulletRB.mass = proyectileMass;
        }
        //
        if (shootCalculation == ShootCalculation.Force)
            newBulletRB.AddForce(directionWithForce, ForceMode.Impulse);
        else
            newBulletRB.velocity = direction * bulletMuzzleSpeed;
            // Debug.Log(prefab.name + " shot with: direction, " + direction + " force, " + forceToApply + "total," + directionWithForce);
        //}
    }

    /// <summary>
    /// TODO: Ojo que ahora trabajamos con una única velocidad de rotación
    /// </summary>
    /// <param name="self"></param>
    /// <param name="objective"></param>
    /// <param name="rotationSpeed"></param>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static Quaternion UpdateRotation(Transform self, Vector3 objective, float rotationSpeed, float dt)
    {

        Vector3 directionToDestination = objective - self.position;

        Vector3 directionCurrent = self.forward;

        Quaternion rotationToDestination = Quaternion.FromToRotation(directionCurrent, directionToDestination);

        Vector3 rotationToDestinationAxis;
        float rotationToDestinationAngle;
        rotationToDestination.ToAngleAxis( out rotationToDestinationAngle, out rotationToDestinationAxis );

        // TODO: Trabajar con el offset para que no se pase
        Quaternion rotationToApply;
        if (rotationToDestinationAngle < rotationSpeed * dt)
        {
            rotationToApply = Quaternion.AngleAxis(rotationToDestinationAngle, rotationToDestinationAxis);
        }
        else
        {
            rotationToApply = Quaternion.AngleAxis(rotationSpeed * dt, rotationToDestinationAxis);
        }
        

        self.rotation = rotationToApply * self.rotation;

        return self.rotation;
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="self"></param>
    /// <param name="objective"></param>
    /// <param name="rotationSpeed"></param>
    /// <param name="dt"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    public static Quaternion UpdateRotationInOneAxis(Transform self, Vector3 objective, float rotationSpeed, float dt,
       Vector3 axis = new Vector3())
    {
        //
        Quaternion rotationToReturn = self.transform.rotation;
        // Para poder decidir el eje
        if (axis == Vector3.zero) axis = Vector3.up;
        // Seleccionamos el plano según el eje que queramos utilizar
        if (axis == Vector3.up)
            objective.y = self.position.y;
        if (axis == Vector3.right)
            objective.x = self.position.x;
        if (axis == Vector3.forward)
            objective.z = self.position.z;
        //
        Quaternion objectiveDirection = Quaternion.LookRotation(objective - self.position);
        rotationToReturn = Quaternion.RotateTowards(rotationToReturn, objectiveDirection, rotationSpeed * dt);
        //
        return rotationToReturn;
    }

    /// <summary>
    /// Anticipate the objective position for autoaiming
    /// </summary>
    /// <param name="selfPosition"></param>
    /// <param name="objectivePosition"></param>
    /// <param name="objectiveVelocity"></param>
    /// <param name="referenceWeaponMuzzleSpeed"></param>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static Vector3 AnticipatePlayerPositionForAiming(Vector3 selfPosition, Vector3 objectivePosition, 
    Vector3 objectiveVelocity, float referenceWeaponMuzzleSpeed, float dt)
    {
        Vector3 playerFutureEstimatedPosition = new Vector3();

        // Determinamos la distancia para ver cuanto anticipar en función de nuestra muzzle speed
        float distanceToPlayer = (objectivePosition - selfPosition).magnitude;
        float timeForBulletToReachPlayer = referenceWeaponMuzzleSpeed / distanceToPlayer * dt;

        playerFutureEstimatedPosition = objectivePosition + (objectiveVelocity * timeForBulletToReachPlayer);

        return playerFutureEstimatedPosition;
    }

    public static Quaternion ConstrainRotation(Quaternion currentRotation, Quaternion originalRotation, Vector2 maxRotationOffset)
    {


        return currentRotation;
    }
}
