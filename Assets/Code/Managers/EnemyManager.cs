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
    private ProvLevelManager levelManager;

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
    private List<GameObject> loneWolfs; // TODO: Trabajar esto con enemigos sin formación

    // Variable para manejar bosses aparte en el índice de spameo
    // NOTA: Igual no lo usamos al final
    private int enemyStartIndex = 0;

    #region Properties

    public int[] ActiveEnemiesAmount { get { return activeEnemiesAmount; } }
    public Vector3 EpicenterPoint { get { return epicenterPoint; } }
    public EpicenterMode CurrentEpicenterMode { get { return epicenterMode; } }

    public List<GameObject> AllActiveEnemies
    {
        get
        {
            List<GameObject> allActiveEnemies = new List<GameObject>();
            for(int i = 0; i < activeEnemies.Length; i++)
            {
                allActiveEnemies.AddRange(activeEnemies[i]);
            }
            return allActiveEnemies;
        }
    }

    public List<EnemyFormation> EnemyFormations { get { return enemyFormations; } }

    public List<GameObject> FormationLeaders
    {
        get
        {
            List<GameObject> formationLeaders = new List<GameObject>(enemyFormations.Count);
            for (int i = 0; i < enemyFormations.Count; i++)
            {
                formationLeaders.Add(enemyFormations[i].FormationLeader.gameObject);
            }
            return formationLeaders;
        }
    }

    #endregion

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
        levelManager = FindObjectOfType<ProvLevelManager>();

        // En caso de boss lo activamos desde el principio
        if (levelManager.LevelVictoryCondition == VictoryCondition.SlayTheBeast)
        {
            ActivateEnemies(0);
            enemyStartIndex = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        // En el boss no actualizamos el spameo de enemigos
        // Lo gestionará el propio boss
        if(levelManager.LevelVictoryCondition != VictoryCondition.SlayTheBeast)
            UpdateEnemySpawnCounts(dt);
    }

    #region New Spawn Methods

    void UpdateEnemySpawnCounts(float dt)
    {
        //
        for (int i = enemyStartIndex; i < enemiesSpawnSettings.Length; i++)
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
    /// Activa los enemigos y los coloca donde corresponde
    /// Puede ser llamado por el boss
    /// </summary>
    /// <param name="i"></param>
    public void ActivateEnemies(int i, Vector3 spawnPoint = new Vector3())
    {
        //Metodo con spawn points
        EnemyIdentifier enemyIdentifier = enemiesSpawnSettings[i].enemyPrefab.GetComponent<EnemyIdentifier>();
        Transform groupSpawn;
        Vector3 pointForGroupSpawn;
        //
        if (spawnPoint != Vector3.zero)
        {
            pointForGroupSpawn = spawnPoint;
        }
        else if (enemyIdentifier)
        {
            EnemyType typeToSpawn = enemyIdentifier.enemyType;
            groupSpawn = GetRandomSpawmPointNearerThanX(typeToSpawn, farestSpawnDistanceToEpicenter);
            // Control de errores
            if (groupSpawn == null)
                return;
            pointForGroupSpawn = groupSpawn.position;
        }
        else
        {
            // TODO: Desarrorlo
            pointForGroupSpawn = epicenterPoint;
        }

        // NOTA: Control de error
        // De primeras no debería haber tamaño de spawn 0
        // Aparte, ahora sale todo el grupo o no sale
        if (enemiesSpawnSettings[i].enemiesToSpawn > 0
            && enemiesSpawnSettings[i].enemiesToSpawn <= enemiesSpawnSettings[i].maxActiveEnemies - activeEnemies[i].Count)
        {
            // Si no hay enemigos activos de ese tipo, aviso de Carol
            if (activeEnemies[i].Count == 0)
                carolHelp.TriggerGeneralAdvice("EnemiesIncoming");
            // Primero iniciamos la formación
            EnemyFormation newEnemyFormation = null;
            if (enemiesSpawnSettings[i].formationData != null) {
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
                if (nextEnemy.gameObject == null)
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
                if(enemyConsistency != null)
                {
                    enemyConsistency.ManagerIndex = i;
                    enemyConsistency.ResetStatus();
                }                

                // Y lo añadimos a enemigos activos
                activeEnemies[i].Add(nextEnemy);

                // Si he formación lo metemos a ella
                if (newEnemyFormation != null)
                {
                    //TODO: Meterlo en la formación
                    EnemyBaseBodyBehaviour behaviour = nextEnemy.GetComponent<EnemyBaseBodyBehaviour>();
                    // TODO: Arregalro para que no haga falta hacer esto
                    if (behaviour == null) behaviour = nextEnemy.GetComponentInChildren<EnemyBaseBodyBehaviour>();
                    //
                    newEnemyFormation.formationMembers.Add(behaviour);
                    behaviour.enemyFormation = newEnemyFormation;
                }

                //GameObject nextEnemy = Instantiate(enemyPrefabsToUse[i], pointForGroupSpawn, Quaternion.identity);
            }

        }
        else
            Debug.Log("Zero enemies to spawn or no more room for enemies");
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

    #region Formation Methods

    public void CheckFusionableFormations(EnemyFormation ownFormation)
    {
        //
        for(int i = 0; i < enemyFormations.Count; i++)
        {
            if(enemyFormations[i] != ownFormation &&
                ownFormation.FormationStrength + enemyFormations[i].FormationStrength <= 1)
            {
                FuseFormations(ownFormation, enemyFormations[i]);
            }
        }
    }

    public void FuseFormations(EnemyFormation formationToStrengthen, EnemyFormation formationToDissolve)
    {
        //Debug.Log("Fusing formations " + formationToStrengthen + " and " + formationToDissolve);
        formationToStrengthen.formationMembers.AddRange(formationToDissolve.formationMembers);
        enemyFormations.Remove(formationToDissolve);
        carolHelp.TriggerGeneralAdvice("SwarmsReforming");
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

