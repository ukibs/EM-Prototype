using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Level", menuName = "LevelData", order = 1)]
public class LevelData : ScriptableObject
{
    //
    public int levelNumber;
    public VictoryCondition victoryCondition = VictoryCondition.DefeatAnyEnemy;
    public int numberToKill = 10;
    //
    public int unlockedAttackAbilities = 0;
    public int unlockedDefenseAbilities = 0;
    public int unlockedJumpAbilities = 0;
    public int unlockedSprintAbilities = 0;
    //.
    public GameObject[] enemiesToSpawn;
    public int[] amountToSpawn;
    public float timeBetweenSpawns;
    //
    public int maxActiveEnemies = 10;
}
