using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsInstantiator : MonoBehaviour {

    public GameObject[] prefabsToSpawn;
    public int[] amountsToSpawn;
    public Vector3 areaOfSpawn;
    public Vector3 areofSpawnOffset;

    // Use this for initialization
    void Start () {
        //
        for (int i = 0; i < prefabsToSpawn.Length; i++)
        {
            //
            for (int j = 0; j < amountsToSpawn[i]; j++)
            {
                //Vector3 newPosition = new Vector3(Random.Range(-amountsToSpawn[i], amountsToSpawn[i]), 5.0f, Random.Range(-amountsToSpawn[i], amountsToSpawn[i]));
                Vector3 newPosition = new Vector3(Random.Range(-areaOfSpawn.x, areaOfSpawn.x) + areofSpawnOffset.x,
                    Random.Range(-areaOfSpawn.y, areaOfSpawn.y) + areofSpawnOffset.y,
                    Random.Range(-areaOfSpawn.z, areaOfSpawn.z) + areofSpawnOffset.z);
                Instantiate(prefabsToSpawn[i], newPosition, Quaternion.identity);
            }
        }
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
