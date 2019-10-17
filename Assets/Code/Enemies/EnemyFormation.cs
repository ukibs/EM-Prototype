using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFormation
{
    //public EnemyBaseBodyBehaviour formationLeader;
    public Vector3[] positions;
    //public FormationType formationType;
    //public float distanceBetweenMembers;
    //public int maxMembersPerRow = 9;
    public List<EnemyBaseBodyBehaviour> formationMembers;

    public FormationInfo formationInfo;

    public EnemyBaseBodyBehaviour FormationLeader { get { return formationMembers[0]; } }

    // Constructor
    public EnemyFormation(FormationInfo formationInfo, int formationSize)
    {
        //

        this.formationInfo = formationInfo;

        //this.formationType = formationInfo.formationType;
        positions = new Vector3[formationSize];
        //this.distanceBetweenMembers = formationInfo.distanceBetweenMembers;
        formationMembers = new List<EnemyBaseBodyBehaviour>(formationSize);
        //this.maxMembersPerRow = formationInfo.maxMembersPerRow; 
        StablishPositions();

        //
        if (formationInfo.weaponData)
        {
            Bullet bulletInfo = formationInfo.weaponData.weapon.proyectilePrefab.GetComponent<Bullet>();
            float bulletLifeTime = bulletInfo.lifeTime;
            BulletPool.instance.RegisterBullets(formationInfo.weaponData.weapon.proyectilePrefab,
                formationInfo.weaponData.weapon.rateOfFire, bulletLifeTime);
        }
    }

    //
    public int GetIndexInFormation(EnemyBaseBodyBehaviour behaviour)
    {
        return formationMembers.IndexOf(behaviour);
    }

    //
    public Vector3 GetFormationPlaceInWorld(EnemyBaseBodyBehaviour behaviour)
    {
        //
        //Debug.Log("Debugging leader: " + FormationLeader);
        // Ñapa
        if (!FormationLeader) return Vector3.zero;
        //
        int formationIndex = formationMembers.IndexOf(behaviour);
        Vector3 placeInWorld = FormationLeader.transform.TransformPoint(positions[formationIndex]);
        return placeInWorld;
    }

    //
    void StablishPositions()
    {
        switch (formationInfo.formationType)
        {
            case FormationType.Arrow:
                
                int numberOfRows = (int)(positions.Length / formationInfo.maxMembersPerRow);
                //
                for(int i = 0; i < numberOfRows; i++)
                {
                    //
                    positions[i * formationInfo.maxMembersPerRow] = new Vector3(0, i * formationInfo.distanceBetweenMembers, 0);
                    //Row
                    int iterations = formationInfo.maxMembersPerRow / 2;
                    //Debug.Log("Row iterations: " + iterations);
                    for (int j = 0; j < iterations; j++)
                    {
                        positions[(i * formationInfo.maxMembersPerRow) + (j * 2) + 1] = 
                            new Vector3(-j * formationInfo.distanceBetweenMembers, 
                                        i * formationInfo.distanceBetweenMembers, 
                                        -j * formationInfo.distanceBetweenMembers);
                        // TODO: Chequear pares para salir
                        //if (maxMembersPerRow <= j * 2 + 2) break;
                        positions[(i * formationInfo.maxMembersPerRow) + (j * 2) + 2] = 
                            new Vector3(j * formationInfo.distanceBetweenMembers, 
                                        i * formationInfo.distanceBetweenMembers, 
                                        -j * formationInfo.distanceBetweenMembers);
                    }                    
                }
                break;
        }
    }

    //
    public void LeaveFormation(EnemyBaseBodyBehaviour behaviour)
    {
        formationMembers.Remove(behaviour);
    }
}
