using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Level", menuName = "LevelData", order = 1)]
public class LevelData : ScriptableObject
{
    //
    public LevelInfo levelInfo;
    //
    //public int levelNumber;
    //public VictoryCondition victoryCondition = VictoryCondition.DefeatAnyEnemy;
    //public int numberToKill = 10;
    ////
    //public int unlockedAttackAbilities = 0;
    //public int unlockedDefenseAbilities = 0;
    //public int unlockedJumpAbilities = 0;
    //public int unlockedSprintAbilities = 0;
    ////.
    //public GameObject[] enemiesToSpawn;
    //public int[] amountToSpawn;
    //public float[] timeBetweenSpawns;
    //public int[] maxActiveEnemies;
    ////
    //public GameObject[] terrainPrefabs;
    //public int[] terrainRation;
    //public int[] terrainMax;
    //public int[] terrainMin;


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
    public VictoryCondition victoryCondition;
    public int enemiesToDefeat;
    //public string enemyIdentifier;
    //
    public int attackActionsAvailable;
    public int defenseActionsAvailable;
    public int jumpActionsAvailable;
    public int sprintActionsAvailable;
    //
    public GameObject[] enemyGroups;
    public int[] enemiesToSpawn;
    public int[] enemySpawnIncrement;
    public float[] timeBetweenSpawns;
    public int[] maxActiveEnemies;
    //
    public GameObject[] terrainPrefabs;
    public int[] terrainRatio;
    public int[] terrainMax;
    public int[] terrainMin;
}