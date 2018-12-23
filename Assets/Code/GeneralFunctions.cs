using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneralFunctions
{
    //
    // Nota: Antes o después trabajaremos con una pool
    public static void ShootProjectile(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 direction, float forceToApply, 
        float dt, ShootCalculation shootCalculation = ShootCalculation.Force)
    {
        
        Rigidbody prefabRb = prefab.GetComponent<Rigidbody>();
        // Get the muzzle speed in meters/second (rememeber 1 is a Ton)
        float bulletMuzzleSpeed;
        if (shootCalculation == ShootCalculation.Force)
            bulletMuzzleSpeed = prefabRb.mass * 1000 / forceToApply;
        // TODO: Esto es una ñapa, hay que arreglarlo
        else
            bulletMuzzleSpeed = forceToApply;
        //Debug.Log("Bullet muzzle speed: " + bulletMuzzleSpeed);
        // Si está lo bastante cerca que el disparo sea hitscan
        //if (direction.magnitude < bulletMuzzleSpeed * dt)
        //{

        //}
        //// Si no bala con objeto
        //else
        //{
            //
            GameObject newBullet = GameObject.Instantiate(prefab, position, rotation);
            Rigidbody newBulletRB = newBullet.GetComponent<Rigidbody>();
            Vector3 directionWithForce = direction.normalized * forceToApply;
        if (shootCalculation == ShootCalculation.Force)
            newBulletRB.AddForce(directionWithForce, ForceMode.Impulse);
        else
            newBulletRB.velocity = direction * bulletMuzzleSpeed;
            // Debug.Log(prefab.name + " shot with: direction, " + direction + " force, " + forceToApply + "total," + directionWithForce);
        //}
    }

    //
    public static Quaternion UpdateRotation(Transform self, Vector3 objective, float rotationSpeed, float dt, Vector3 axis = new Vector3())
    {
        // First to know the direction
        Vector3 forward = self.forward.normalized;
        Vector3 pointDirection = (objective - self.position).normalized;
        // Para poder decidir el eje
        if (axis == Vector3.zero) axis = Vector3.up;
        // Decide the axis to use
        Vector2 selfPlaneToUse = new Vector2();
        Vector2 destinationPlaneToUse = new Vector2();
        //
        if (axis == Vector3.up) { selfPlaneToUse = new Vector2(forward.z, forward.x); destinationPlaneToUse = new Vector2(pointDirection.z, pointDirection.x); }
        if (axis == Vector3.right) { selfPlaneToUse = new Vector2(forward.y, forward.z); destinationPlaneToUse = new Vector2(pointDirection.y, pointDirection.z); }
        if (axis == Vector3.forward) { selfPlaneToUse = new Vector2(forward.y, forward.x); destinationPlaneToUse = new Vector2(pointDirection.y, pointDirection.x); }
        //
        float forwardAngle = Mathf.Atan2(selfPlaneToUse.x, selfPlaneToUse.y);
        float pDAnlge = Mathf.Atan2(destinationPlaneToUse.x, destinationPlaneToUse.y);
        float offset = (pDAnlge - forwardAngle) * Mathf.Rad2Deg;
        
        //A fix for when the number overflows the half circle
        if (Mathf.Abs(offset) > 180.0f)
        {
            offset -= 360.0f * Mathf.Sign(offset);
        }

        //And apply turning or check to move
        Quaternion rotationToReturn = self.transform.rotation;
        //Quaternion targetRotation = Quaternion.LookRotation(pointDirection);
        //rotationToReturn = Quaternion.Lerp(rotationToReturn, targetRotation, dt * rotationSpeed);
        
        if (Mathf.Abs(offset) < rotationSpeed * dt)
        {
            //self.Rotate(0.0f, -offset, 0.0f);
            rotationToReturn *= Quaternion.AngleAxis(-offset, axis);
        }
        else
        {
            //self.Rotate(0.0f, rotationSpeed * Mathf.Sign(-offset) * dt, 0.0f);
            rotationToReturn *= Quaternion.AngleAxis(rotationSpeed * Mathf.Sign(-offset) * dt, axis);
        }
        
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
}
