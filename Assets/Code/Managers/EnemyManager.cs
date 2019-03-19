using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject[] enemyPrefabsToUse;
    public int[] initialGroupToSpawnSize;
    public int[] continuousGroupToSpawnSize;
    public int maxEnemiesInAction;
    public float timeBetweenSpawns = 10;
    public float minSpawnDistance = 100;
    public float maxSpawnDistance = 200;
    public int spawnLimit = -1;

    private float timeFromLastSpawn = 0;
    private Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = FindObjectOfType<RobotControl>().transform;
        //
        SpawnEnemies(initialGroupToSpawnSize);
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        timeFromLastSpawn += dt;
        //
        if(spawnLimit > -1)
        {

        }
        //
        if(timeFromLastSpawn >= timeBetweenSpawns)
        {
            //
            int activeEnemies = FindObjectsOfType<EnemyConsistency>().Length;
            //
            if(activeEnemies < maxEnemiesInAction)
                SpawnEnemies(continuousGroupToSpawnSize);
            // Si no que vuelva a empezar a contrar y ya
            timeFromLastSpawn -= timeBetweenSpawns;
        }
    }

    void SpawnEnemies(int[] groupToSpawnSize)
    {
        for(int i = 0; i < enemyPrefabsToUse.Length; i++)
        {
            //
            //float spawnAngle = Random.Range(0, 360);
            //float spawnRadius = Random.Range(minSpawnDistance, maxSpawnDistance);
            //Vector2 groupSpawnPositionXY = new Vector2(Mathf.Cos(spawnAngle) * spawnRadius, Mathf.Sin(spawnAngle) * spawnRadius);
            //Vector3 pointForGroupSpawn = new Vector3(groupSpawnPositionXY.x + playerTransform.position.x, 1, 
            //                                            groupSpawnPositionXY.y + playerTransform.position.z);
            //
            for (int j = 0; j < groupToSpawnSize[i]; j++)
            {
                //Vector3 positionToSpawn = new Vector3(Random.Range(-groupToSpawnSize[i], groupToSpawnSize[i]) + pointForGroupSpawn.x, 1,
                //                                        Random.Range(-groupToSpawnSize[i], groupToSpawnSize[i]) + pointForGroupSpawn.z);
                //
                //
                float spawnAngle = Random.Range(0, 360);
                float spawnRadius = Random.Range(minSpawnDistance, maxSpawnDistance);
                Vector2 groupSpawnPositionXY = new Vector2(Mathf.Cos(spawnAngle) * spawnRadius, Mathf.Sin(spawnAngle) * spawnRadius);
                Vector3 pointForGroupSpawn = new Vector3(groupSpawnPositionXY.x + playerTransform.position.x, 2,
                                                            groupSpawnPositionXY.y + playerTransform.position.z);
                //
                //GameObject nextEnemy = Instantiate(enemyPrefabsToUse[i], positionToSpawn, Quaternion.identity);
                GameObject nextEnemy = Instantiate(enemyPrefabsToUse[i], pointForGroupSpawn, Quaternion.identity);
            }
        }
    }

    public void InitiateManager(GameObject[] enemiesPrefabs, int[] enemiesToSpawn)
    {
        //
        enemyPrefabsToUse = enemiesPrefabs;
        initialGroupToSpawnSize = enemiesToSpawn;
        continuousGroupToSpawnSize = enemiesToSpawn;
        // Vigilar que no salte dos veces
        SpawnEnemies(initialGroupToSpawnSize);
    }
}
