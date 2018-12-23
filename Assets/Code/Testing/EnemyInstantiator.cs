using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInstantiator : MonoBehaviour
{
    public GameObject[] prefabsToSpawn;
    public int[] amountsToSpawn;
    //public Vector3 areaOfSpawn;

    // Use this for initialization
    void Start()
    {
        //
        for (int i = 0; i < prefabsToSpawn.Length; i++)
        {
            //
            for (int j = 0; j < amountsToSpawn[i]; j++)
            {
                Vector3 newPosition = new Vector3(j * 5 - amountsToSpawn.Length * 5/2, 2.0f, i * 5 + 100);
                Instantiate(prefabsToSpawn[i], newPosition, Quaternion.identity);
            }
        }

    }
}
