using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject[] enemyPrefabsToUse;
    public int[] initialGroupToSpawnSize;
    public int[] continuousGroupToSpawnSize;
    public int[] enemySpawnIncrement;
    public int[] maxEnemiesInAction;
    public float[] timeBetweenSpawns;
    public float minSpawnDistance = 100;
    public float maxSpawnDistance = 200;
    public int spawnLimit = -1;

    private float[] timeFromLastSpawn;
    private int[] activeEnemies;
    private Transform playerTransform;

    // Vamos a manejarlo aqui de moemnto para no saturar el audio
    private List<AudioClip> activeFiringClips;
    private List<float> afcTimeActive;

    // Start is called before the first frame update
    void Start()
    {
        //
        playerTransform = FindObjectOfType<RobotControl>().transform;
        //
        SpawnEnemies(initialGroupToSpawnSize);
        //
        activeFiringClips = new List<AudioClip>(10);
        afcTimeActive = new List<float>(10);
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        for (int i = 0; i < enemyPrefabsToUse.Length; i++)
        {
            //
            timeFromLastSpawn[i] += dt;
            //
            if (timeFromLastSpawn[i] >= timeBetweenSpawns[i])
            {
                //
                //int activeEnemies = FindObjectsOfType<EnemyConsistency>().Length;
                //
                if (activeEnemies[i] < maxEnemiesInAction[i])
                    SpawnEnemies(continuousGroupToSpawnSize);
                // Si no que vuelva a empezar a contrar y ya
                timeFromLastSpawn[i] -= timeBetweenSpawns[i];
            }
        }
        // Manejamos aqui los clips
        for(int i = 0; i < activeFiringClips.Count; i++)
        {
            //
            afcTimeActive[i] += dt;
            if(afcTimeActive[i] >= activeFiringClips[i].length)
            {
                activeFiringClips.RemoveAt(i);
                afcTimeActive.RemoveAt(i);
            }
        }
    }

    //
    void SpawnEnemies(int[] groupToSpawnSize)
    {
        for(int i = 0; i < enemyPrefabsToUse.Length; i++)
        {
            //
            float spawnAngle = UnityEngine.Random.Range(0, 360);
            float spawnRadius = UnityEngine.Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector2 groupSpawnPositionXY = new Vector2(Mathf.Cos(spawnAngle) * spawnRadius, Mathf.Sin(spawnAngle) * spawnRadius);
            Vector3 pointForGroupSpawn = new Vector3(groupSpawnPositionXY.x + playerTransform.position.x, 1,
                                                        groupSpawnPositionXY.y + playerTransform.position.z);
            //
            float memberSpawnAngle = 360 / groupToSpawnSize[i];
            float meberSpawnRadius = 10;
            //
            for (int j = 0; j < groupToSpawnSize[i]; j++)
            {
                // 
                float memberSpawnCoordinates = memberSpawnAngle * j;
                Vector2 memberSpawnPositionXY = new Vector2(Mathf.Cos(memberSpawnCoordinates) * meberSpawnRadius, 
                                                            Mathf.Sin(memberSpawnCoordinates) * meberSpawnRadius);

                //Vector3 positionToSpawn = new Vector3(Random.Range(-groupToSpawnSize[i], groupToSpawnSize[i]) + pointForGroupSpawn.x, 1,
                //                                        Random.Range(-groupToSpawnSize[i], groupToSpawnSize[i]) + pointForGroupSpawn.z);
                Vector3 positionToSpawn = new Vector3(pointForGroupSpawn.x + memberSpawnPositionXY.x, 
                    pointForGroupSpawn.y, pointForGroupSpawn.z + memberSpawnPositionXY.y);
                //
                //
                //float spawnAngle = Random.Range(0, 360);
                //float spawnRadius = Random.Range(minSpawnDistance, maxSpawnDistance);
                //Vector2 groupSpawnPositionXY = new Vector2(Mathf.Cos(spawnAngle) * spawnRadius, Mathf.Sin(spawnAngle) * spawnRadius);
                //Vector3 pointForGroupSpawn = new Vector3(groupSpawnPositionXY.x + playerTransform.position.x, 2,
                //                                            groupSpawnPositionXY.y + playerTransform.position.z);
                //
                //GameObject nextEnemy = 
                    Instantiate(enemyPrefabsToUse[i], positionToSpawn, Quaternion.identity);
                //GameObject nextEnemy = Instantiate(enemyPrefabsToUse[i], pointForGroupSpawn, Quaternion.identity);
            }
            //
            activeEnemies[i] += groupToSpawnSize[i];
            //
            groupToSpawnSize[i] += enemySpawnIncrement[i];
        }
    }

    //
    public void InitiateManager(LevelInfo levelInfo)
    {
        //
        enemyPrefabsToUse = levelInfo.enemiesToUse;
        initialGroupToSpawnSize = levelInfo.enemiesToSpawn;
        continuousGroupToSpawnSize = levelInfo.enemiesToSpawn;
        enemySpawnIncrement = levelInfo.enemySpawnIncrement;
        maxEnemiesInAction = levelInfo.maxActiveEnemies;
        this.timeBetweenSpawns = levelInfo.timeBetweenSpawns;
        //
        activeEnemies = new int[enemyPrefabsToUse.Length];
        timeFromLastSpawn = new float[enemyPrefabsToUse.Length];
        // Vigilar que no salte dos veces
        SpawnEnemies(initialGroupToSpawnSize);
    }

    //
    public void AddClip(AudioClip firingClip)
    {
        activeFiringClips.Add(firingClip);
        afcTimeActive.Add(0);
    }

    //
    public bool IsFiringClipActive(AudioClip firingClip)
    {
        return activeFiringClips.Contains(firingClip);
    }

    //
    public void SubtractOne(GameObject gameObject)
    {
        //NOTA: Esto 
        string[] prefabName = gameObject.name.Split('(');
        //
        for(int i = 0; i < enemyPrefabsToUse.Length; i++)
        {
            if (prefabName[0].Equals(enemyPrefabsToUse[i].name))
                activeEnemies[i]--;
        }
    }
}

