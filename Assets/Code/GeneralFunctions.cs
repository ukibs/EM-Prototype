﻿using System.Collections;
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
    public static Vector3 AnticipateObjectivePositionForAiming(Vector3 selfPosition, Vector3 objectivePosition, 
    Vector3 objectiveVelocity, float referenceWeaponMuzzleSpeed, float dt)
    {
        Vector3 playerFutureEstimatedPosition = new Vector3();

        // Determinamos la distancia para ver cuanto anticipar en función de nuestra muzzle speed
        float distanceToPlayer = (objectivePosition - selfPosition).magnitude;
        // Let's check the calculations
        float timeWithoutDrag = referenceWeaponMuzzleSpeed / distanceToPlayer * dt;
        Vector3 objectivePositionWithEstimation = objectivePosition + (objectiveVelocity * timeWithoutDrag);
        // TODO: Sacar posición del objetivo con ese tiempo
        float timeWithDrag = EstimateFlyingTimeWithDrag(selfPosition, objectivePosition, referenceWeaponMuzzleSpeed, 0.1f);
        // Debug.Log("Time without drag: " + timeWithoutDrag + ", with drag: " + timeWithDrag);
        float timeForBulletToReachPlayer = timeWithDrag * 1;

        //playerFutureEstimatedPosition = objectivePosition + (objectiveVelocity * timeForBulletToReachPlayer);
        playerFutureEstimatedPosition = objectivePositionWithEstimation + (objectiveVelocity * timeForBulletToReachPlayer);

        return playerFutureEstimatedPosition;
    }

    /// <summary>
    /// Gives the height the rpoyectile will lose due to gravity before reaching its objective
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="objectivePoint"></param>
    /// <param name="muzzleSpeed"></param>
    /// <returns></returns>
    public static float GetProyectileFallToObjective(Vector3 startPoint, Vector3 objectivePoint, float muzzleSpeed)
    {
        // TODO: Revisar que haga falta (o no) la velocidad inicial (en Y)
        // La podemos sacar con la muzzleSpeed y la dirección hacia el objetivo
        Vector3 distanceToObjective = objectivePoint - startPoint;

        // Con esto podemos sacar la velocidad en y para estimar
        Vector3 directionToObjective = distanceToObjective.normalized;
        Vector3 proyectile3dSpeed = directionToObjective * muzzleSpeed;

        float secondsToObjective = distanceToObjective.magnitude / muzzleSpeed;
        //float fallInThatTime = (proyectile3dSpeed.y * secondsToObjective) + 
        //    -9.81f * Mathf.Pow(secondsToObjective,2) / 2;
        float fallInThatTime = -9.81f * Mathf.Pow(secondsToObjective, 2) / 2;

        return fallInThatTime;
    }

    // El drag seguramente funcione como el dump
    // TODO: Averiguarlo
    public static float EstimateFlyingTimeWithDrag(Vector3 startPoint, Vector3 objectivePoint, float muzzleSPeed, float proyectileDrag)
    {
        // TODO: Pillar a Anontio por banda
        // t = ln(1-(d*k/v0))/-k
        Vector3 distance = objectivePoint - startPoint;
        float travelTime = Mathf.Log(1 - (distance.magnitude * proyectileDrag / muzzleSPeed)) / -proyectileDrag;
        return travelTime;
    }

    // TODO: Recordar lo que quería ahcer con esta función
    public static float GetDeviationAngle(Vector3 origin, Vector3 objective)
    {
        // De momento solo en el eje x
        

        return 0;
    }

    // TODO: hacerlo aqui
    public static Quaternion ConstrainRotation(Quaternion currentRotation, Quaternion originalRotation, Vector2 maxRotationOffset)
    {


        return currentRotation;
    }

    /// <summary>
    /// 1940 Navy's formula for armor penetration
    /// No es perfecta pero nos vale de momento
    /// OJO: No se tiene en cuenta el angulo
    /// Cuando lo hagamos habrá que hacerlo aparte
    /// </summary>
    /// <param name="tShellWeight"></param>
    /// <param name="mmShellDiameter"></param>
    /// <param name="msShellVelocity"></param>
    /// <returns></returns>
    public static float Navy1940PenetrationCalc(float tShellWeight, float mmShellDiameter, float msShellVelocity)
    {
        //
        float poundToKg = 2.2f;
        float inchToMm = 0.04f;
        float feetToM = 3.28f;
        float tToKg = 1000;
        //
        float functionConstant = 0.000469f;
        float weightPow = 0.556f;
        float diameterPow = -0.6521f;
        float velocityPow = 1.1001f;

        // = $H$24*POW(B6*$F$21/1000;$H$21) * POW(D6*$F$22;$H$22) * POW(F6*$F$23;$H$23)/$F$22

        return functionConstant * Mathf.Pow(tShellWeight * poundToKg * tToKg, weightPow) 
            * Mathf.Pow(mmShellDiameter * inchToMm, diameterPow)
            * Mathf.Pow(msShellVelocity * feetToM, velocityPow)
            / inchToMm;
    }
}
