using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Level", menuName = "LevelData", order = 1)]
public class LevelData : ScriptableObject
{
    //
    public LevelInfo levelInfo;

    ///////// implement form here /////////
    /*
     TERRAIN
     - max terrain pieces in level
     - terrain pieces to use in the level (types)
        - minimum piees (type) used in level
        - ratio of each terrain piece

     ENEMIES
     - enemy types used in level
        - max number of enemy type in level
        - spamed enemy type per wave
        - time between waves of enemy type
    */
}

[System.Serializable]
public class LevelInfo
{
    //
    public string inGameName;
    public string description;
    public GameMode gameMode;
    //
    public VictoryCondition victoryCondition;
    public int enemiesToDefeat;
    public float expectedDuration;
    //public string enemyIdentifier;
    // TODO: Quitaremos esto
    public int attackActionsAvailable;
    public int defenseActionsAvailable;
    public int jumpActionsAvailable;
    public int sprintActionsAvailable;
    //
    //public GameObject[] enemyGroups;
    //public int[] enemiesToSpawn;
    //public int[] enemySpawnIncrement;
    //public float[] timeBetweenSpawns;
    //public int[] maxActiveEnemies;
    public EnemySpawnSettings[] enemiesSpawnSettings;
    //
    //public GameObject[] terrainPrefabs;
    //public int[] terrainRatio;
    //public int[] terrainMax;
    //public int[] terrainMin;
    public TerrainElementSettings[] terrainElementsSettings;
}

[System.Serializable]
public class EnemySpawnSettings
{
    public GameObject enemyPrefab;
    public FormationData formationData;
    public int enemiesToSpawn;
    public int enemySpawnIncrement;
    public float timeBetweenSpawns;
    public int maxActiveEnemies;
}

[System.Serializable]
public class TerrainElementSettings
{
    public GameObject terrainPrefab;
    public int terrainRatio;
    public int terrainMax;
    public int terrainMin;
}