﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class GeneralFunctions
{
    //
    // Nota: Antes o después trabajaremos con una pool
    public static GameObject ShootProjectile(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 direction, float forceToApply, 
        float dt, ShootCalculation shootCalculation = ShootCalculation.Force)
    {
        
        //Rigidbody prefabRb = prefab.GetComponent<Rigidbody>();
        // Get the muzzle speed in meters/second (rememeber 1 is a Ton)
        // TODO: Lo acabaremos quitando
        float bulletMuzzleSpeed = 0;
        if (shootCalculation == ShootCalculation.MuzzleSpeed)
            bulletMuzzleSpeed = forceToApply;
        //
        GameObject newBullet = GameObject.Instantiate(prefab, position, rotation);
        Rigidbody newBulletRB = newBullet.GetComponent<Rigidbody>();
        Vector3 directionWithForce = direction.normalized * forceToApply;
        // Recodar que la relación es gramos - toneladas, no gramos - kilos
        if (shootCalculation == ShootCalculation.Force)
            newBulletRB.AddForce(directionWithForce / 1000, ForceMode.Impulse);
        else
            newBulletRB.velocity = direction * bulletMuzzleSpeed;
        // Debug.Log(prefab.name + " shot with: direction, " + direction + " force, " + forceToApply + "total," + directionWithForce);
        //}

        //
        return newBullet;
    }

    //
    public static GameObject ShootProjectileFromPool(GameObject bulletToUse, Vector3 position, Quaternion rotation, Vector3 direction, 
        float forceToApply, float dt, ShootCalculation shootCalculation = ShootCalculation.Force)
    {

        //Rigidbody prefabRb = prefab.GetComponent<Rigidbody>();
        // Get the muzzle speed in meters/second (rememeber 1 is a Ton)
        // TODO: Lo acabaremos quitando
        float bulletMuzzleSpeed = 0;
        if (shootCalculation == ShootCalculation.MuzzleSpeed)
            bulletMuzzleSpeed = forceToApply;
        //
        //GameObject newBullet = GameObject.Instantiate(prefab, position, rotation);
        bulletToUse.transform.position = position;
        bulletToUse.transform.rotation = rotation;
        //
        Rigidbody newBulletRB = bulletToUse.GetComponent<Rigidbody>();
        Vector3 directionWithForce = direction.normalized * forceToApply;
        // Recodar que la relación es gramos - toneladas, no gramos - kilos
        if (shootCalculation == ShootCalculation.Force)
            newBulletRB.AddForce(directionWithForce / 1000, ForceMode.Impulse);
        else
            newBulletRB.velocity = direction * bulletMuzzleSpeed;
        // Debug.Log(prefab.name + " shot with: direction, " + direction + " force, " + forceToApply + "total," + directionWithForce);
        //}

        //
        return bulletToUse;
    }

    /// <summary>
    /// Actualiza rotación sin tener en cuenta eje
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
        //Debug.Log("ffds " + axis);
        // Seleccionamos el plano según el eje que queramos utilizar
        if (axis == Vector3.up)
            objective.y = self.position.y;
        if (axis == Vector3.right)
            objective.x = self.position.x;
        if (axis == Vector3.forward)
            objective.z = self.position.z;
        //Debug.Log("Axis used: " + axis);
        //
        Quaternion objectiveDirection = Quaternion.LookRotation(objective - self.position);
        rotationToReturn = Quaternion.RotateTowards(rotationToReturn, objectiveDirection, rotationSpeed * dt);
        //
        return rotationToReturn;
    }

    /// <summary>
    /// Función para rotar hacia la cruz entre el sujeto y un objetivo
    /// </summary>
    /// <param name="self"></param>
    /// <param name="objective"></param>
    /// <param name="rotationSpeed"></param>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static Quaternion UpdateRotationOnCross(Transform self, Vector3 objective, float rotationSpeed, float dt)
    {
        //
        Quaternion rotationToReturn = self.transform.rotation;
        //
        objective.y = self.transform.position.y;

        //
        Vector3 objectiveDirection = objective - self.position;
        Vector3 crossDirection = Vector3.Cross(self.up, objectiveDirection);
        
        //
        Quaternion idealRotation = Quaternion.LookRotation(/*self.position + */crossDirection);
        rotationToReturn = Quaternion.RotateTowards(rotationToReturn, idealRotation, rotationSpeed * dt);

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
    Vector3 objectiveVelocity, float referenceWeaponMuzzleSpeed, float dt, float proyectileDrag = 0.1f)
    {
        Vector3 playerFutureEstimatedPosition = new Vector3();
        float distanceToPlayer = (objectivePosition - selfPosition).magnitude;
        float timeWithoutDrag = distanceToPlayer / referenceWeaponMuzzleSpeed;
        Vector3 objectivePositionWithEstimation = objectivePosition + (objectiveVelocity * timeWithoutDrag);
        float timeWithDrag = EstimateFlyingTimeWithDrag(selfPosition, objectivePositionWithEstimation, 
                                                        referenceWeaponMuzzleSpeed, proyectileDrag);        
        playerFutureEstimatedPosition = objectivePosition + (objectiveVelocity * timeWithDrag);
        return playerFutureEstimatedPosition;
    }

    // TODO: Función aparte para sacar cálculo con drag
    //public static float GetFlyingTimeWithDrag()
    //{

    //}

    /// <summary>
    /// Gives the height the rpoyectile will lose due to gravity before reaching its objective
    /// Proyectile drag has a default value of 0.1, the value for spheric objects
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="objectivePoint"></param>
    /// <param name="muzzleSpeed"></param>
    /// <returns></returns>
    public static float GetProyectileFallToObjective(Vector3 startPoint, Vector3 objectivePoint, float muzzleSpeed, 
        float proyectileDrag = 0.1f)
    {
        float secondsToObjectiveWithDrag = EstimateFlyingTimeWithDrag(startPoint, objectivePoint, muzzleSpeed, proyectileDrag);
        float fallInThatTime = Physics.gravity.y * Mathf.Pow(secondsToObjectiveWithDrag, 2) / 2;
        return fallInThatTime;
    }

    /// <summary>
    /// Estimates the time the proyectile will need to reach its objectives having account on the drag
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="objectivePoint"></param>
    /// <param name="muzzleSPeed"></param>
    /// <param name="proyectileDrag"></param>
    /// <returns></returns>
    public static float EstimateFlyingTimeWithDrag(Vector3 startPoint, Vector3 objectivePoint, float muzzleSPeed, float proyectileDrag)
    {
        // t = ln(1-(d*k/v0))/-k
        Vector3 distance = objectivePoint - startPoint;
        float travelTime = Mathf.Log(1 - (distance.magnitude * proyectileDrag / muzzleSPeed)) / -proyectileDrag;
        return travelTime;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="initialVelocity"></param>
    /// <param name="distance"></param>
    /// <param name="drag"></param>
    /// <param name="proyectileMass"></param>
    /// <returns></returns>
    public static float GetVelocityWithDistanceAndDrag(float initialVelocity, float distance, float drag, float proyectileMass)
    {
        float handheldArea = 0.0001f;
        float exponentialPart = -1 * drag * handheldArea * distance * proyectileMass;
        double velocityAtPoint = (initialVelocity * Math.Exp(exponentialPart));
        return Convert.ToSingle(velocityAtPoint);
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

        float result = functionConstant * Mathf.Pow(tShellWeight * poundToKg * tToKg, weightPow)
            * Mathf.Pow(mmShellDiameter * inchToMm, diameterPow)
            * Mathf.Pow(msShellVelocity * feetToM, velocityPow)
            / inchToMm;

        // AJuste manual
        result *= 0.75f;

        return result;
    }

    /// <summary>
    /// Thompson's penetration formula of 1930
    /// It works with armor thickness, proyectile diameter and impact angle
    /// It returns the required kinetic energy to penetrate a determine thicness of armor
    /// Remember: It already has de impact angle in count. Not more calculations with it are required
    /// </summary>
    /// <param name="armorThickness"></param>
    /// <param name="proyectileDiameter"></param>
    /// <param name="impactAngle"></param>
    /// <returns></returns>
    public static float Thompson1930PenetrationCalc(float armorThickness, float proyectileDiameter, float impactAngle)
    {
        float fCoefficient = 1.8288f * (armorThickness/proyectileDiameter - 0.45f) * ((float)Math.Pow(impactAngle, 2) + 2000) + 12192;
        float requiredKE = 8.025f * (armorThickness * (float)Math.Pow(proyectileDiameter, 2) * (float)Math.Pow(fCoefficient, 2) /
                            (float)Math.Pow(Mathf.Cos(impactAngle * Mathf.Deg2Rad), 2)); ;

        return requiredKE;
    }

    /// <summary>
    /// Get the collision force between two bodies
    /// Remember, bullets go another way
    /// </summary>
    /// <param name="selfRb"></param>
    /// <param name="otherRb"></param>
    /// <returns></returns>
    public static float GetCollisionForce(Rigidbody selfRb, Rigidbody otherRb)
    {
        // Ponemos esto para evitar errores
        // Pero esto no dbería ocurrir
        if (selfRb == null)
            return 0;

        //
        RigidbodyExtraInfo selfRbExtraInfo = selfRb.GetComponent<RigidbodyExtraInfo>();

        // El otherRb puede ser nulo
        float otherImpactForce = 0;
        if (otherRb != null)
        {
            RigidbodyExtraInfo otherRbExtraInfo = otherRb.GetComponent<RigidbodyExtraInfo>();

            //otherImpactForce = otherRb.velocity.magnitude * otherRb.mass;
            otherImpactForce = GeneralFunctions.GetBodyKineticEnergy(otherRb, otherRbExtraInfo);
        }


        // El propio no puede serlo
        //float selfImpactForce = selfRb.velocity.magnitude * selfRb.mass;
        float selfImpactForce = GeneralFunctions.GetBodyKineticEnergy(selfRb, selfRbExtraInfo);

        //
        float impactForce = otherImpactForce + selfImpactForce;
        return impactForce;
    }

    /// <summary>
    /// Error proof sound function
    /// Rememeber, it will avoid exceptions
    /// Not guarantee to play
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public static void PlaySoundEffect(AudioSource audioSource, AudioClip audioClip, float pitchVariation = 0)
    {
        if(audioSource != null && audioClip != null)
        {
            audioSource.clip = audioClip;
            if (pitchVariation != 0.0f)
            {
                audioSource.pitch = UnityEngine.Random.Range(1.0f - pitchVariation, 1.0f + pitchVariation);
            }
            audioSource.Play();
        }
        else
        {
            Debug.Log("Tried to play not assigned clip");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public static void PlaySoundEffectWithoutOverlaping(AudioSource audioSource, AudioClip audioClip, float pitchVariation = 0)
    {
        if (audioSource != null && audioClip != null)
        {
            if(!audioSource.isPlaying || audioClip != audioSource.clip)
            {
                audioSource.clip = audioClip;

                if (pitchVariation != 0.0f)
                {
                    audioSource.pitch = UnityEngine.Random.Range(1.0f - pitchVariation, 1.0f + pitchVariation);
                }
                audioSource.Play();
            }            
        }
    }

    /// <summary>
    /// Gives the kinetic energy of a rigidbody
    /// </summary>
    /// <param name="rb"></param>
    /// <returns></returns>
    public static float GetBodyKineticEnergy(Rigidbody rb, RigidbodyExtraInfo rbExtraInfo)
    {
        //
        if(rbExtraInfo == null)
        {
            Debug.Log("Asking kinetic energy of not prepared object: " + rb.name);
            return 0;
        }
        //
        float bodySpeedVariation = Math.Abs((rb.velocity - rbExtraInfo.PreviousVelocity).magnitude);
        float bodyMass = rb.mass;

        float bodyKE = bodyMass * Mathf.Pow(bodySpeedVariation, 2) / 2;

        Debug.Log("Impact of " + rb.name + ", speed variation of " + bodySpeedVariation + ", impacting with a force of " + bodyKE + " Joules");

        return bodyKE;
    }

    /// <summary>
    /// Gives the kinetic energy of a bullet, assuming it compelty stops on impact
    /// </summary>
    /// <param name="rb"></param>
    /// <returns></returns>
    public static float GetBulletKineticEnergy(Rigidbody rb)
    {
        float bodySpeed = rb.velocity.magnitude;
        float bodyMass = rb.mass;

        float bodyKE = bodyMass * Mathf.Pow(bodySpeed, 2) / 2;

        return bodyKE;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="bodyMass"></param>
    /// <returns></returns>
    public static float GetBodyKineticEnergy(Vector3 vectorialVelocity, float bodyMass)
    {
        float bodyKE = bodyMass * Mathf.Pow(vectorialVelocity.magnitude, 2) / 2;

        return bodyKE;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="bodyMass"></param>
    /// <returns></returns>
    public static float GetBodyKineticEnergy(float velocity, float bodyMass)
    {
        float bodyKE = bodyMass * Mathf.Pow(velocity, 2) / 2;

        return bodyKE;
    }

    /// <summary>
    /// Gives the momentum of a rigidbody
    /// TODO: Decidir si devolverlo como vector
    /// </summary>
    /// <param name="rb"></param>
    /// <returns></returns>
    public static float GetBodyMomentum(Rigidbody rb)
    {
        float bodySpeed = rb.velocity.magnitude;
        float bodyMass = rb.mass;

        float bodyMomentum = bodyMass * bodySpeed;

        return bodyMomentum;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="proyectileLenght"></param>
    /// <param name="proyectileDensity"></param>
    /// <param name="armorDensity"></param>
    /// <returns></returns>
    public static float GetNewtonPenetration(float proyectileLenght, float proyectileDensity, float armorDensity)
    {
        return proyectileLenght * (proyectileDensity / armorDensity);
    }

    public static float GetArmorThiccnessWithAngle(float armorThickness, float incidenceAngle)
    {
        return armorThickness / Mathf.Cos(incidenceAngle);
    }

    public static float GetForceTransmitedWithAngle(float kineticEnergy, float incidenceAngle)
    {
        return kineticEnergy * (1 - Mathf.Pow(Mathf.Cos(incidenceAngle), 2));
    }

    // TODO: Creo que esto ya no se usa
    public static T DeepCopy<T>(T obj)
    {

        if (!typeof(T).IsSerializable)
        {
            throw new Exception("The source object must be serializable");

        }

        if (System.Object.ReferenceEquals(obj, null))
        {
            throw new Exception("The source object must not be null");
        }

        T result = default(T);

        using (var memoryStream = new MemoryStream())
        {
            var formatter = new BinaryFormatter();

            formatter.Serialize(memoryStream, obj);

            memoryStream.Seek(0, SeekOrigin.Begin);

            result = (T)formatter.Deserialize(memoryStream);

            memoryStream.Close();
        }

        return result;

    }
}
