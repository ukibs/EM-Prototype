using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleTerrainElement : MonoBehaviour
{
    //
    private DestructibleTerrain parentTerrain;

    private void Start()
    {
        //brokenVersion = transform.Find
        parentTerrain = GetComponentInParent<DestructibleTerrain>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(parentTerrain != null)
            parentTerrain.CheckAndDestroy(collision);        
    }
    
}
