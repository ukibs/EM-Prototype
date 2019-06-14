using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileControl : MonoBehaviour
{

    private DestructibleTerrain[] destructibleTerrains;

    // Start is called before the first frame update
    void Start()
    {
        destructibleTerrains = GetComponentsInChildren<DestructibleTerrain>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RestoreElements()
    {
        for(int i = 0; i < destructibleTerrains.Length; i++)
        {
            destructibleTerrains[i].Restore();
        }
    }
}
