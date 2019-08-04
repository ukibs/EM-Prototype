using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    //
    public float maxDistanceToNeighbour = 200;
    public float fCost = 0;
    public float hCost = 0;
    public Waypoint pathParent;
    //
    private TerrainManager terrainManager;
    private List<Waypoint> currentNeighbors;
    private List<float> distancesToNeighbors;

    //
    public List<Waypoint> CurrentNeighbors { get { return currentNeighbors; } }
    public List<float> DistancesToNeighbors { get { return distancesToNeighbors; } }

    public float GCost { get { return fCost + hCost; } }

    // Start is called before the first frame update
    void Start()
    {
        terrainManager = FindObjectOfType<TerrainManager>();
        //currentNeighbors = new List<Waypoint>(10);
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void OnDrawGizmos()
    {
        //
        //ShowPathsToNeighbors();
    }

    //
    void ShowPathsToNeighbors()
    {
        if (currentNeighbors != null)
        {
            for (int i = 0; i < currentNeighbors.Count; i++)
            {
                //Debug.DrawLine(transform.position, currentNeighbors[i].transform.position, Color.green, 100, false);
                //Gizmos.DrawLine(transform.position, currentNeighbors[i].transform.position);
                Vector3 neighbourDirection = currentNeighbors[i].transform.position - transform.position;
                Debug.DrawRay(transform.position, neighbourDirection, Color.blue);
            }
        }
    }

    //
    public void GetNeighbours()
    {
        //
        if (currentNeighbors == null)
        {
            currentNeighbors = new List<Waypoint>(10);
            distancesToNeighbors = new List<float>(10);
        }

        // Primero limpiamos los viejos
        else
        {
            currentNeighbors.Clear();
            distancesToNeighbors.Clear();
        }
        // TODO: Manejor esto en el terrain manager
        if (terrainManager == null)
            terrainManager = FindObjectOfType<TerrainManager>();
        //
        Waypoint[] allWaypoints = terrainManager.AllWaypoints;
        //
        for(int i = 0; i < allWaypoints.Length; i++)
        {
            //Que no sea él mismo
            if (this != allWaypoints[i])
            {
                // Primero distancia
                Vector3 distanceAndDirection = allWaypoints[i].transform.position - transform.position;
                if (distanceAndDirection.magnitude <= maxDistanceToNeighbour)
                {
                    // Y después que esté a la vista
                    if (!Physics.Raycast(transform.position, distanceAndDirection.normalized, distanceAndDirection.magnitude))
                    {
                        currentNeighbors.Add(allWaypoints[i]);
                        distancesToNeighbors.Add(distanceAndDirection.magnitude);
                    }
                }
            }
        }
        //
        //Debug.Log("Neighbors found: " + currentNeighbors.Count);
    }

    //
    void PurgeExcedentNeighbors()
    {

    }
}
