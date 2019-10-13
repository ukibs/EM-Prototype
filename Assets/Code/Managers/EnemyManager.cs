using System;
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
    public EnemySpawnSettings[] enemiesSpawnSettings;
    //public GameObject[] enemyPrefabsToUse;
    ////public int[] initialGroupToSpawnSize;
    //public int[] groupsToSpawnSizes;
    //public int[] enemySpawnIncrement;
    //public int[] maxEnemiesInAction;
    //public float[] timeBetweenSpawns;
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

    // Con esto gestionaremos las formaciones enemigas
    private List<EnemyFormation> enemyFormations;


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
        //
        enemyFormations = new List<EnemyFormation>();
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        for (int i = 0; i < enemiesSpawnSettings.Length; i++)
        {
            //
            timeFromLastSpawn[i] += dt;
            //
            if (timeFromLastSpawn[i] >= enemiesSpawnSettings[i].timeBetweenSpawns)
            {
                // Si no que vuelva a empezar a contrar y ya
                timeFromLastSpawn[i] -= enemiesSpawnSettings[i].timeBetweenSpawns;
                //
                //int activeEnemies = FindObjectsOfType<EnemyConsistency>().Length;
                // TODO: Sacar solo el grupo determinado por el index
                // Así estamos sacando a todos
                if (activeEnemiesAmount[i] < enemiesSpawnSettings[i].maxActiveEnemies)
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
        //float angle = UnityEngine.Random.Range(0, 360);
        ////
        //float pointX = Mathf.Cos(angle) * distanceToPlayer;
        //float pointY = Mathf.Sin(angle) * distanceToPlayer;
        ////
        //epicenterPoint = new Vector3(pointX, 0, pointY);
        //
        epicenterPoint = Vector3.forward * distanceToPlayer;
    }

    // Los iniciamos desactivados
    void InitiateEnemies()
    {
        //
        activeEnemies = new List<GameObject>[enemiesSpawnSettings.Length];
        reserveEnemies = new List<GameObject>[enemiesSpawnSettings.Length];
        // Pasamos por cada uno de los prefabs preparados
        for (int i = 0; i < enemiesSpawnSettings.Length; i++)
        {
            //
            activeEnemies[i] = new List<GameObject>(enemiesSpawnSettings[i].maxActiveEnemies);
            reserveEnemies[i] = new List<GameObject>(enemiesSpawnSettings[i].maxActiveEnemies);
            // Instanciamos y guardamos inactivos el máximo posible
            for (int j = 0; j < enemiesSpawnSettings[i].maxActiveEnemies; j++)
            {
                GameObject newEnemy = Instantiate(enemiesSpawnSettings[i].enemyPrefab, Vector3.zero, Quaternion.identity);
                //Debug.Log(newEnemy.name);
                newEnemy.SetActive(false);
                reserveEnemies[i].Add(newEnemy);
            }
        }
        //
        // Debug.Log("Enemies initiated");
    }

    /// <summary>
    /// TODO: Que spamee todo el grupo o no spamee
    /// </summary>
    /// <param name="i"></param>
    void ActivateEnemies(int i)
    {
        //Metodo con spawn points
        EnemyType typeToSpawn = enemiesSpawnSettings[i].enemyPrefab.GetComponent<EnemyIdentifier>().enemyType;
        Transform groupSpawn = GetRandomSpawmPointNearerThanX(typeToSpawn, farestSpawnDistanceToEpicenter);
        // Control de errores
        if (groupSpawn == null)
            return;
        //
        Vector3 pointForGroupSpawn = groupSpawn.position;

        // NOTA: Control de error
        // De primeras no debería haber tamaño de spawn 0
        // Aparte, ahora sale todo el grupo o no sale
        if (enemiesSpawnSettings[i].enemiesToSpawn > 0
            && enemiesSpawnSettings[i].enemiesToSpawn < enemiesSpawnSettings[i].maxActiveEnemies - activeEnemies[i].Count)
        {
            // Si no hay enemigos activos de ese tipo, aviso de Carol
            if (activeEnemies[i].Count == 0)
                carolHelp.TriggerGeneralAdvice("EnemiesIncoming");
            // Primero iniciamos la formación
            EnemyFormation newEnemyFormation = null;
            if(enemiesSpawnSettings[i].formationData != null) {
                //
                //newEnemyFormation = new EnemyFormation(enemiesSpawnSettings[i].enemiesToSpawn, 
                //    enemiesSpawnSettings[i].formationData.formationInfo.formationType, 
                //    enemiesSpawnSettings[i].formationData.formationInfo.distanceBetweenMembers);
                newEnemyFormation = new EnemyFormation(enemiesSpawnSettings[i].formationData.formationInfo, 
                    enemiesSpawnSettings[i].enemiesToSpawn);
                //
                enemyFormations.Add(newEnemyFormation);
            }
            //
            float memberSpawnAngle = 360 / enemiesSpawnSettings[i].enemiesToSpawn;
            float meberSpawnRadius = 10;
            // Sacamos a los enemigos
            for (int j = 0; j < enemiesSpawnSettings[i].enemiesToSpawn; j++)
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
                // TODO: Revisar lo de la posición al activarlos
                nextEnemy.SetActive(true);
                nextEnemy.transform.position = positionToSpawn;                
                EnemyConsistency enemyConsistency = nextEnemy.GetComponent<EnemyConsistency>();
                //
                if (enemyConsistency == null)
                    enemyConsistency = nextEnemy.GetComponentInChildren<EnemyConsistency>();
                //
                enemyConsistency.ManagerIndex = i;
                enemyConsistency.ResetStatus();

                // Y lo añadimos a enemigos activos
                activeEnemies[i].Add(nextEnemy);

                // Si he formación lo metemos a ella
                if(newEnemyFormation != null)
                {
                    //TODO: Meterlo en la formación
                    EnemyBaseBodyBehaviour behaviour = nextEnemy.GetComponent<EnemyBaseBodyBehaviour>();
                    // TODO: Arregalro para que no haga falta hacer esto
                    if(behaviour == null) behaviour = nextEnemy.GetComponentInChildren<EnemyBaseBodyBehaviour>();
                    //
                    newEnemyFormation.formationMembers.Add(behaviour);
                    behaviour.enemyFormation = newEnemyFormation;
                }

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
        enemiesSpawnSettings = levelInfo.enemiesSpawnSettings;
        //
        //enemyPrefabsToUse = levelInfo.enemyGroups;
        ////groupsToSpawnSizes = levelInfo.enemiesToSpawn.CopyTo(groupsToSpawnSizes);
        //groupsToSpawnSizes = new int[levelInfo.enemiesToSpawn.Length];
        //levelInfo.enemiesToSpawn.CopyTo(groupsToSpawnSizes, 0);
        //enemySpawnIncrement = levelInfo.enemySpawnIncrement;
        //maxEnemiesInAction = levelInfo.maxActiveEnemies;
        //this.timeBetweenSpawns = levelInfo.timeBetweenSpawns;
        //
        activeEnemiesAmount = new int[enemiesSpawnSettings.Length];
        timeFromLastSpawn = new float[enemiesSpawnSettings.Length];
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

