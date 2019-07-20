using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject[] enemyPrefabsToUse;
    //public int[] initialGroupToSpawnSize;
    public int[] groupsToSpawnSizes;
    public int[] enemySpawnIncrement;
    public int[] maxEnemiesInAction;
    public float[] timeBetweenSpawns;
    //public float minSpawnDistance = 100;
    //public float maxSpawnDistance = 200;
    public int spawnLimit = -1;

    private float[] timeFromLastSpawn;
    private int[] activeEnemies;
    private Transform playerTransform;
    private EnemySpawnPoint[] enemySpawnPoints;
    private CarolBaseHelp carolHelp;

    // Vamos a manejarlo aqui de moemnto para no saturar el audio
    //private List<AudioClip> activeFiringClips;
    //private List<float> afcTimeActive;

    public int[] ActiveEnemies { get { return activeEnemies; } }

    // Start is called before the first frame update
    void Start()
    {
        //
        playerTransform = FindObjectOfType<RobotControl>().transform;
        //
        //SpawnEnemies(initialGroupToSpawnSize);
        //
        //activeFiringClips = new List<AudioClip>(10);
        //afcTimeActive = new List<float>(10);
        //
        carolHelp = FindObjectOfType<CarolBaseHelp>();
        
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
                // Si no que vuelva a empezar a contrar y ya
                timeFromLastSpawn[i] -= timeBetweenSpawns[i];
                //
                //int activeEnemies = FindObjectsOfType<EnemyConsistency>().Length;
                // TODO: Sacar solo el grupo determinado por el index
                // Así estamos sacando a todos
                if (activeEnemies[i] < maxEnemiesInAction[i])
                    SpawnEnemies(i);
                
            }
        }
    }

    //
    void SpawnEnemies(int i)
    {
        //for(int i = 0; i < enemyPrefabsToUse.Length; i++)
        //{
            // Metodo viejo de spammeo
            //float spawnAngle = UnityEngine.Random.Range(0, 360);
            //float spawnRadius = UnityEngine.Random.Range(minSpawnDistance, maxSpawnDistance);
            //Vector2 groupSpawnPositionXY = new Vector2(Mathf.Cos(spawnAngle) * spawnRadius, Mathf.Sin(spawnAngle) * spawnRadius);
            //Vector3 pointForGroupSpawn = new Vector3(groupSpawnPositionXY.x + playerTransform.position.x, 1,
            //                                            groupSpawnPositionXY.y + playerTransform.position.z);

            //Metodo con spawn points
            EnemyType typeToSpawn = enemyPrefabsToUse[i].GetComponent<EnemyIdentifier>().enemyType;
            //Debug.Log(typeToSpawn);
            //Vector3 pointForGroupSpawn = GetNearestSpawnPointToPlayer(typeToSpawn).position;
            Vector3 pointForGroupSpawn = GetRandomSpawmPointNearerThanX(typeToSpawn, 500).position;
            
            //
            if (groupsToSpawnSizes[i] > 0)
            {
                //
                float memberSpawnAngle = 360 / groupsToSpawnSizes[i];
                float meberSpawnRadius = 10;
                //
                for (int j = 0; j < groupsToSpawnSizes[i]; j++)
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
                    GameObject nextEnemy = Instantiate(enemyPrefabsToUse[i], positionToSpawn, Quaternion.identity);
                    EnemyConsistency enemyConsistency = nextEnemy.GetComponent<EnemyConsistency>();
                    //
                    if (enemyConsistency == null)
                        enemyConsistency = nextEnemy.GetComponentInChildren<EnemyConsistency>();
                    //
                    enemyConsistency.ManagerIndex = i;
                
                        //GameObject nextEnemy = Instantiate(enemyPrefabsToUse[i], pointForGroupSpawn, Quaternion.identity);
                }
                //
                if (activeEnemies[i] == 0)
                    carolHelp.TriggerGeneralAdvice("EnemiesIncoming");
                //
                activeEnemies[i] += groupsToSpawnSizes[i];
                //
                groupsToSpawnSizes[i] += enemySpawnIncrement[i];
            }
            else
            {
                Debug.Log("Group depleted");
            }
        //}
    }

    //
    public void InitiateManager(LevelInfo levelInfo)
    {
        //
        enemyPrefabsToUse = levelInfo.enemyGroups;
        //groupsToSpawnSizes = levelInfo.enemiesToSpawn.CopyTo(groupsToSpawnSizes);
        groupsToSpawnSizes = new int[levelInfo.enemiesToSpawn.Length];
        levelInfo.enemiesToSpawn.CopyTo(groupsToSpawnSizes, 0);
        enemySpawnIncrement = levelInfo.enemySpawnIncrement;
        maxEnemiesInAction = levelInfo.maxActiveEnemies;
        this.timeBetweenSpawns = levelInfo.timeBetweenSpawns;
        //
        activeEnemies = new int[enemyPrefabsToUse.Length];
        timeFromLastSpawn = new float[enemyPrefabsToUse.Length];
        // Si se raya aqui lo mandamos al Initiate Manager
        enemySpawnPoints = FindObjectsOfType<EnemySpawnPoint>();
        //Debug.Log(" Spawn points: " + enemySpawnPoints.Length);
        // Vigilar que no salte dos veces
        //SpawnEnemies(groupsToSpawnSizes);
    }

    //
    public void SubtractOne(int enemyIndex)
    {
        //NOTA: Esto 
        string[] prefabName = gameObject.name.Split('(');
        //
        //for(int i = 0; i < enemyPrefabsToUse.Length; i++)
        //{
            //
            //Debug.Log("Checking enemy to subtract: " + prefabName[0] + ", " + enemyPrefabsToUse[i].name);
            //
            //if (prefabName[0].Equals(enemyPrefabsToUse[i].name))
            //{
                //
                activeEnemies[enemyIndex]--;
                //
                Debug.Log("Subtracting " + enemyPrefabsToUse[enemyIndex].name);
                //
                //return;
        //    }
                
        //}
        //
        //Debug.Log("Subtracting: Name not matched, " + prefabName[0]);
    }

    //
    Transform GetNearestSpawnPointToPlayer(EnemyType enemyTypeToSpawn)
    {
        Transform nearestSpawnPoint = null;
        float nearestDistance = Mathf.Infinity;
        //
        for(int i = 0; i < enemySpawnPoints.Length; i++)
        {
            if(enemySpawnPoints[i].enemyType == enemyTypeToSpawn)
            {
                Vector3 distanceToPlayer = playerTransform.position - enemySpawnPoints[i].transform.position;
                if(distanceToPlayer.magnitude < nearestDistance)
                {
                    nearestSpawnPoint = enemySpawnPoints[i].transform;
                    nearestDistance = distanceToPlayer.magnitude;
                }
            }
        }
        //
        return nearestSpawnPoint;
    }

    //
    Transform GetRandomSpawmPointNearerThanX(EnemyType enemyTypeToSpawn, float maxDistance)
    {
        List<Transform> candidateSpawns = new List<Transform>(10);

        for (int i = 0; i < enemySpawnPoints.Length; i++)
        {
            if (enemySpawnPoints[i].enemyType == enemyTypeToSpawn)
            {
                Vector3 distanceToPlayer = playerTransform.position - enemySpawnPoints[i].transform.position;
                if (distanceToPlayer.magnitude < maxDistance)
                {
                    candidateSpawns.Add(enemySpawnPoints[i].transform);
                }
            }
        }
        //
        int selectedSpawn = UnityEngine.Random.Range(0, candidateSpawns.Count);

        return candidateSpawns[selectedSpawn];
    }
}

