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
    private int[] activeEnemiesAmount;
    private Transform playerTransform;
    private EnemySpawnPoint[] enemySpawnPoints;
    private CarolBaseHelp carolHelp;

    // Vamos a manejarlo aqui de moemnto para no saturar el audio
    //private List<AudioClip> activeFiringClips;
    //private List<float> afcTimeActive;

    // Trabajaremos estos dos array de listas para el POOL
    private List<GameObject>[] activeEnemies;
    private List<GameObject>[] reserveEnemies;


    public int[] ActiveEnemiesAmount { get { return activeEnemiesAmount; } }

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
        //
        InitiateEnemies();
        
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
                if (activeEnemiesAmount[i] < maxEnemiesInAction[i])
                    //SpawnEnemies(i);
                    ActivateEnemies(i);
                
            }
        }
    }

    #region New Spawn Methods

    // Los iniciamos desactivados
    void InitiateEnemies()
    {
        //
        activeEnemies = new List<GameObject>[enemyPrefabsToUse.Length];
        reserveEnemies = new List<GameObject>[enemyPrefabsToUse.Length];
        // Pasamos por cada uno de los prefabs preparados
        for (int i = 0; i < enemyPrefabsToUse.Length; i++)
        {
            //
            activeEnemies[i] = new List<GameObject>(maxEnemiesInAction[i]);
            reserveEnemies[i] = new List<GameObject>(maxEnemiesInAction[i]);
            // Instanciamos y guardamos inactivos el máximo posible
            for (int j = 0; j < maxEnemiesInAction[i]; j++)
            {
                GameObject newEnemy = Instantiate(enemyPrefabsToUse[i], Vector3.zero, Quaternion.identity);
                //Debug.Log(newEnemy.name);
                newEnemy.SetActive(false);
                reserveEnemies[i].Add(newEnemy);
            }
        }
        //
        Debug.Log("Enemies initiated");
    }

    //
    void ActivateEnemies(int i)
    {
        //Metodo con spawn points
        EnemyType typeToSpawn = enemyPrefabsToUse[i].GetComponent<EnemyIdentifier>().enemyType;
        Vector3 pointForGroupSpawn = GetRandomSpawmPointNearerThanX(typeToSpawn, 500).position;

        // NOTA: Control de error
        // De primeras no debería haber tamaño de spawn 0
        if (groupsToSpawnSizes[i] > 0)
        {
            // Si no hay enemigos activos de ese tipo, aviso de Carol
            if (activeEnemies[i].Count == 0)
                carolHelp.TriggerGeneralAdvice("EnemiesIncoming");
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

                // TODO: Chequear cuando esté vacía
                if (reserveEnemies[i].Count == 0)
                {
                    Debug.Log("No more enemies in reserve");
                    continue;
                }
                GameObject nextEnemy = reserveEnemies[i][0];
                reserveEnemies[i].Remove(nextEnemy);
                // TODO: Revisar que falla aquí
                if(nextEnemy.gameObject == null)
                {
                    Debug.Log(nextEnemy);
                    continue;
                }
                //
                nextEnemy.SetActive(true);
                nextEnemy.transform.position = positionToSpawn;                
                EnemyConsistency enemyConsistency = nextEnemy.GetComponent<EnemyConsistency>();
                //
                if (enemyConsistency == null)
                    enemyConsistency = nextEnemy.GetComponentInChildren<EnemyConsistency>();
                //
                enemyConsistency.ManagerIndex = i;
                enemyConsistency.ResetHealth();

                // Y lo añadimos a enemigos activos
                activeEnemies[i].Add(nextEnemy);

                //GameObject nextEnemy = Instantiate(enemyPrefabsToUse[i], pointForGroupSpawn, Quaternion.identity);
            }
            
        }
    }

    //
    public void SendToReserve(int index, GameObject downedEnemy)
    {
        //
        activeEnemies[index].Remove(downedEnemy);
        reserveEnemies[index].Add(downedEnemy);
        downedEnemy.SetActive(false);
    }

    #endregion

    #region Old Spawn Methods

    //
    //void SpawnEnemies(int i)
    //{
    //    //for(int i = 0; i < enemyPrefabsToUse.Length; i++)
    //    //{
    //        // Metodo viejo de spammeo
    //        //float spawnAngle = UnityEngine.Random.Range(0, 360);
    //        //float spawnRadius = UnityEngine.Random.Range(minSpawnDistance, maxSpawnDistance);
    //        //Vector2 groupSpawnPositionXY = new Vector2(Mathf.Cos(spawnAngle) * spawnRadius, Mathf.Sin(spawnAngle) * spawnRadius);
    //        //Vector3 pointForGroupSpawn = new Vector3(groupSpawnPositionXY.x + playerTransform.position.x, 1,
    //        //                                            groupSpawnPositionXY.y + playerTransform.position.z);

    //        //Metodo con spawn points
    //        EnemyType typeToSpawn = enemyPrefabsToUse[i].GetComponent<EnemyIdentifier>().enemyType;
    //        //Debug.Log(typeToSpawn);
    //        //Vector3 pointForGroupSpawn = GetNearestSpawnPointToPlayer(typeToSpawn).position;
    //        Vector3 pointForGroupSpawn = GetRandomSpawmPointNearerThanX(typeToSpawn, 500).position;
            
    //        //
    //        if (groupsToSpawnSizes[i] > 0)
    //        {
    //            //
    //            float memberSpawnAngle = 360 / groupsToSpawnSizes[i];
    //            float meberSpawnRadius = 10;
    //            //
    //            for (int j = 0; j < groupsToSpawnSizes[i]; j++)
    //            {
    //                // 
    //                float memberSpawnCoordinates = memberSpawnAngle * j;
    //                Vector2 memberSpawnPositionXY = new Vector2(Mathf.Cos(memberSpawnCoordinates) * meberSpawnRadius,
    //                                                            Mathf.Sin(memberSpawnCoordinates) * meberSpawnRadius);

    //                //Vector3 positionToSpawn = new Vector3(Random.Range(-groupToSpawnSize[i], groupToSpawnSize[i]) + pointForGroupSpawn.x, 1,
    //                //                                        Random.Range(-groupToSpawnSize[i], groupToSpawnSize[i]) + pointForGroupSpawn.z);
    //                Vector3 positionToSpawn = new Vector3(pointForGroupSpawn.x + memberSpawnPositionXY.x,
    //                    pointForGroupSpawn.y, pointForGroupSpawn.z + memberSpawnPositionXY.y);
    //                //
    //                //
    //                //float spawnAngle = Random.Range(0, 360);
    //                //float spawnRadius = Random.Range(minSpawnDistance, maxSpawnDistance);
    //                //Vector2 groupSpawnPositionXY = new Vector2(Mathf.Cos(spawnAngle) * spawnRadius, Mathf.Sin(spawnAngle) * spawnRadius);
    //                //Vector3 pointForGroupSpawn = new Vector3(groupSpawnPositionXY.x + playerTransform.position.x, 2,
    //                //                                            groupSpawnPositionXY.y + playerTransform.position.z);
    //                //
    //                GameObject nextEnemy = Instantiate(enemyPrefabsToUse[i], positionToSpawn, Quaternion.identity);
    //                EnemyConsistency enemyConsistency = nextEnemy.GetComponent<EnemyConsistency>();
    //                //
    //                if (enemyConsistency == null)
    //                    enemyConsistency = nextEnemy.GetComponentInChildren<EnemyConsistency>();
    //                //
    //                enemyConsistency.ManagerIndex = i;
                
    //                    //GameObject nextEnemy = Instantiate(enemyPrefabsToUse[i], pointForGroupSpawn, Quaternion.identity);
    //            }
    //            //
    //            if (activeEnemiesAmount[i] == 0)
    //                carolHelp.TriggerGeneralAdvice("EnemiesIncoming");
    //            //
    //            activeEnemiesAmount[i] += groupsToSpawnSizes[i];
    //            //
    //            groupsToSpawnSizes[i] += enemySpawnIncrement[i];
    //        }
    //        else
    //        {
    //            Debug.Log("Group depleted");
    //        }
    //    //}
    //}

    //
    //public void SubtractOne(int enemyIndex)
    //{
    //    //NOTA: Esto 
    //    //string[] prefabName = gameObject.name.Split('(');
    //    //
    //    //for(int i = 0; i < enemyPrefabsToUse.Length; i++)
    //    //{
    //    //
    //    //Debug.Log("Checking enemy to subtract: " + prefabName[0] + ", " + enemyPrefabsToUse[i].name);
    //    //
    //    //if (prefabName[0].Equals(enemyPrefabsToUse[i].name))
    //    //{
    //    //
    //    activeEnemiesAmount[enemyIndex]--;
    //    //
    //    //Debug.Log("Subtracting " + enemyPrefabsToUse[enemyIndex].name);
    //    //
    //    //return;
    //    //    }

    //    //}
    //    //
    //    //Debug.Log("Subtracting: Name not matched, " + prefabName[0]);
    //}

    #endregion

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
        activeEnemiesAmount = new int[enemyPrefabsToUse.Length];
        timeFromLastSpawn = new float[enemyPrefabsToUse.Length];
        // Si se raya aqui lo mandamos al Initiate Manager
        enemySpawnPoints = FindObjectsOfType<EnemySpawnPoint>();
        //Debug.Log(" Spawn points: " + enemySpawnPoints.Length);
        // Vigilar que no salte dos veces
        //SpawnEnemies(groupsToSpawnSizes);
    }

    #region Pathfinding Methods

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

    #endregion

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

