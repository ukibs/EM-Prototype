﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    public enum EpicenterMode
    {
        Invalid = -1,

        Player,
        FixedPoint,

        Count
    }

    public EpicenterMode epicenterMode;
    public GameObject[] enemyPrefabsToUse;
    //public int[] initialGroupToSpawnSize;
    public int[] groupsToSpawnSizes;
    public int[] enemySpawnIncrement;
    public int[] maxEnemiesInAction;
    public float[] timeBetweenSpawns;
    //public float minSpawnDistance = 100;
    //public float maxSpawnDistance = 200;
    public int spawnLimit = -1;
    public float farestSpawnDistanceToEpicenter = 1000;

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

    // Para cuando el epicentro sea un punto determinado
    private Vector3 epicenterPoint;


    public int[] ActiveEnemiesAmount { get { return activeEnemiesAmount; } }
    public Vector3 EpicenterPoint { get { return epicenterPoint; } }
    public EpicenterMode CurrentEpicenterMode { get { return epicenterMode; } }

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
        //
        DetermineEpicenterPoint();
        
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

    void DetermineEpicenterPoint()
    {
        //
        float distanceToPlayer = 1500;
        float angle = UnityEngine.Random.Range(0, 360);
        //
        float pointX = Mathf.Cos(angle) * distanceToPlayer;
        float pointY = Mathf.Sin(angle) * distanceToPlayer;
        //
        epicenterPoint = new Vector3(pointX, 0, pointY);
    }

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
        Transform groupSpawn = GetRandomSpawmPointNearerThanX(typeToSpawn, farestSpawnDistanceToEpicenter);
        //
        if (groupSpawn == null)
            return;
        //
        Vector3 pointForGroupSpawn = groupSpawn.position;

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
        // Chequeo extra para solventar problemas con la jerarquía
        EnemyConsistency enemyConsistency = downedEnemy.GetComponent<EnemyConsistency>();
        if (enemyConsistency == null)
            downedEnemy.transform.GetChild(0).localPosition = Vector3.zero;
        //
        activeEnemies[index].Remove(downedEnemy);
        reserveEnemies[index].Add(downedEnemy);
        downedEnemy.SetActive(false);
        
    }

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

    // TODO: Revisar aqui algunas cosas
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

    /// <summary>
    /// Get a random spawn point not farer than maxDistance from player
    /// </summary>
    /// <param name="enemyTypeToSpawn"></param>
    /// <param name="maxDistance"></param>
    /// <returns></returns>
    Transform GetRandomSpawmPointNearerThanX(EnemyType enemyTypeToSpawn, float maxDistance)
    {
        //
        Vector3 epicenter = Vector3.zero;
        switch (epicenterMode)
        {
            case EpicenterMode.Player: epicenter = playerTransform.position; break;
            case EpicenterMode.FixedPoint: epicenter = epicenterPoint; break;
        }
        //
        List<Transform> candidateSpawns = new List<Transform>(10);

        for (int i = 0; i < enemySpawnPoints.Length; i++)
        {
            if (enemySpawnPoints[i].enemyType == enemyTypeToSpawn)
            {
                Vector3 distanceToPlayer = epicenter - enemySpawnPoints[i].transform.position;
                if (distanceToPlayer.magnitude < maxDistance)
                {
                    candidateSpawns.Add(enemySpawnPoints[i].transform);
                }
            }
        }
        // Chequeamos que haya al menos uno
        if (candidateSpawns.Count == 0)
            return null;
        //
        int selectedSpawn = UnityEngine.Random.Range(0, candidateSpawns.Count);

        return candidateSpawns[selectedSpawn];
    }
}

