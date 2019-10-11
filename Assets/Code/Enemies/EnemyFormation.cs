using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFormation
{
    //public EnemyBaseBodyBehaviour formationLeader;
    public Vector3[] positions;
    public FormationType formationType;
    public float distanceBetweenMembers;
    public int maxMembersPerRow = 9;
    public List<EnemyBaseBodyBehaviour> formationMembers;

    public EnemyBaseBodyBehaviour FormationLeader { get { return formationMembers[0]; } }

    // Constructor
    public EnemyFormation(int size, FormationType formationType, float distanceBetweenMembers)
    {
        this.formationType = formationType;
        positions = new Vector3[size];
        this.distanceBetweenMembers = distanceBetweenMembers;
        formationMembers = new List<EnemyBaseBodyBehaviour>(size);
        StablishPositions();
    }

    //
    //public void AsignLeader(EnemyBaseBodyBehaviour newFormationLeader)
    //{
    //    this.formationLeader = newFormationLeader;
    //}

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
        //
        int formationIndex = formationMembers.IndexOf(behaviour);
        Vector3 placeInWorld = FormationLeader.transform.TransformPoint(positions[formationIndex]);
        return placeInWorld;
    }

    //
    void StablishPositions()
    {
        switch (formationType)
        {
            case FormationType.Arrow:
                
                int numberOfRows = (int)(positions.Length / maxMembersPerRow);
                //
                for(int i = 0; i < numberOfRows; i++)
                {
                    //
                    positions[i * maxMembersPerRow] = new Vector3(0, i * distanceBetweenMembers, 0);
                    //Row
                    int iterations = maxMembersPerRow / 2;
                    Debug.Log("Row iterations: " + iterations);
                    for (int j = 0; j < iterations; j++)
                    {
                        positions[(i * maxMembersPerRow) + (j * 2) + 1] = new Vector3(-j * distanceBetweenMembers, i * distanceBetweenMembers, -j * distanceBetweenMembers);
                        // TODO: Chequear pares para salir
                        //if (maxMembersPerRow <= j * 2 + 2) break;
                        positions[(i * maxMembersPerRow) + (j * 2) + 2] = new Vector3(j * distanceBetweenMembers, i * distanceBetweenMembers, -j * distanceBetweenMembers);
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
