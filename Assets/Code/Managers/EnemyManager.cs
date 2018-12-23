using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject[] enemyPrefabsToUse;
    public int[] groupToSpawnSize;
    public float timeBetweenSpawns = 10;
    public float minSpawnDistance = 100;
    public float maxSpawnDistance = 200;

    private float timeFromLastSpawn = 0;
    private Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = FindObjectOfType<RobotControl>().transform;
        //
        SpawnEnemies();
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        timeFromLastSpawn += dt;
        //
        if(timeFromLastSpawn >= timeBetweenSpawns)
        {
            SpawnEnemies();
            timeFromLastSpawn -= timeBetweenSpawns;
        }
    }

    void SpawnEnemies()
    {
        for(int i = 0; i < enemyPrefabsToUse.Length; i++)
        {
            //
            float spawnAngle = Random.Range(0, 360);
            float spawnRadius = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector2 groupSpawnPositionXY = new Vector2(Mathf.Cos(spawnAngle) * spawnRadius, Mathf.Sin(spawnAngle) * spawnRadius);
            Vector3 pointForGroupSpawn = new Vector3(groupSpawnPositionXY.x + playerTransform.position.x, 1, 
                                                        groupSpawnPositionXY.y + playerTransform.position.z);
            //
            for (int j = 0; j < groupToSpawnSize[i]; j++)
            {
                Vector3 positionToSpawn = new Vector3(Random.Range(-groupToSpawnSize[i], groupToSpawnSize[i]) + pointForGroupSpawn.x, 1,
                                                        Random.Range(-groupToSpawnSize[i], groupToSpawnSize[i]) + pointForGroupSpawn.z);
                GameObject nextEnemy = Instantiate(enemyPrefabsToUse[i], positionToSpawn, Quaternion.identity);
            }
        }
    }
}
