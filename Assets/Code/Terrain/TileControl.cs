using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileControl : MonoBehaviour
{

    private DestructibleTerrain[] destructibleTerrains;
    private DestroyedPiece[] destroyedPieces;

    // Start is called before the first frame update
    void Start()
    {
        destructibleTerrains = GetComponentsInChildren<DestructibleTerrain>();
        destroyedPieces = GetComponentsInChildren<DestroyedPiece>();
    }

    public void RestoreElements()
    {
        //
        for(int i = 0; i < destructibleTerrains.Length; i++)
        {
            destructibleTerrains[i].Restore();
        }
        //
        for (int i = 0; i < destroyedPieces.Length; i++)
        {
            destroyedPieces[i].Restore();
        }
    }
}
