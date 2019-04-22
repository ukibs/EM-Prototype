using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Invalid = -1,

    Worm,
    Grunt,
    Screecher,

    Count
}

public class EnemySpawnPoint : MonoBehaviour
{
    public EnemyType enemyType = EnemyType.Worm;
    
}
