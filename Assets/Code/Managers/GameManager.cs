using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

// De momento lo manejamos con esto
//public enum GameProgression
//{
//    Invalid = -1,

//    FirstIntro,
//    FirstFight,
//    DiscoveringMap,

//    Count
//}

// Selector provisional para cambiar entre bichos y mekanioides
public enum WorkingWith
{
    Invalid = -1,

    Mekanoids,
    Bugs,

    Count
}

/// <summary>
/// Manager del juego en general
/// Ya lo iremos ampliando
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Public Attributes

    //
    public static GameManager instance = null;

    //
    public WorkingWith workingWith = WorkingWith.Bugs;

    // Acciones desbloqueadas
    public int unlockedJumpActions = 0;
    public int unlockedSprintActions = 0;
    public int unlockedAttackActions = 0;
    public int unlockedDefenseActions = 0;
    //
    public float maxCharge = 1;
    //
    public float pulseForce = 10;
    public float canonBaseMuzzleSpeed = 1000;
    public float canonBaseProyectileMass = 0.00001f;
    public float rapidFireRate = 1;
    public float rapidFireMuzzleSpeed = 1000;
    public float piercingBaseMuzzleSpeed = 2000;
    public float piercingBaseProyectileMass = 0.00001f;
    //
    public float movementForce = 5;
    public float jumpForce = 10;
    public float sprintForce = 1;
    //
    public float sphericShieldStrength = 100;
    //
    public float maxShield = 10000;
    public float maxHealth = 1000;
    public float shieldAbsortion = 100;
    public float armor = 2000;

    #endregion

    #region Private Attributes

    //private LevelStatus[] levelsStatus;
    private int currentExperience = 0;
    // TODO: Decidir como manejar esto
    private int gameProgression = 0;
    // Usaremos -1 para diferenciar el infinito del resto
    private int currentLevel = 0;
    //
    private LevelInfo[] levelsInfo;

    #endregion

    #region Properties

    public int CurrentExperience { get { return currentExperience; } }
    public int GameProgression { get { return gameProgression; } }
    //public int CurrentLevel { set { currentLevel = value; } }

    #endregion

    // TODO: Añadir marcador de enemigos detectados cuando empecemos a trabajar largas distancias

    #region Unity Methods
    //
    void Awake()
    {
        //Check if there is already an instance of SoundManager
        if (instance == null)
            //if not, set it to this.
            instance = this;
        //If instance already exists:
        else if (instance != this)
            //Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
            Destroy(gameObject);

        //Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
        DontDestroyOnLoad(gameObject);
        //
        gameProgression = PlayerPrefs.GetInt("Game Progression", 0);
        //
        if (levelsInfo == null)
        {
            //
            // GetLevelsInfoFromXML();
            //
            GetLevelInfoFromLevelData();
        }
            
    }

    private void Start()
    {
        // De momento dos niveles
        //levelsStatus = new LevelStatus[2];
        // TODO: Que lea un XML (o documento de otro tipo)
        // para cargar la info de los niveles
        //if(levelsInfo == null)
        //    GetLevelsInfoFromXML();
    }

    #endregion

    #region Methods

    public void ReceiveExperience(int experienceAmount)
    {

    }

    public void ActivateNextWeapon()
    {
        unlockedAttackActions++;
    }

    public void ProgressInGame()
    {
        if(currentLevel == gameProgression && gameProgression < levelsInfo.Length - 1)
        {
            gameProgression++;
            //
            PlayerPrefs.SetInt("Game Progression", gameProgression);
            //
            Debug.Log("Progressing: Current game progression: " + gameProgression);
        }
    }

    public void SelectLevel(int levelNumber)
    {
        currentLevel = levelNumber;
    }

    #endregion

    #region XML Functions

    public void GetLevelsInfoFromXML()
    {
        //
        //Debug.Log("Getting levels info: Current game progression: " + gameProgression);
        //
        XmlDocument xml_d;
        XmlNodeList xmlLevelsInfo;
        TextAsset textasset = (TextAsset)Resources.Load("LevelInfo", typeof(TextAsset));
        xml_d = new XmlDocument();
        xml_d.LoadXml(textasset.text);
        xmlLevelsInfo = xml_d.GetElementsByTagName("LEVEL");
        // Preparamos el levels info
        levelsInfo = new LevelInfo[xmlLevelsInfo.Count];
        // Y vamos cargando los datos
        for(int i = 0; i < xmlLevelsInfo.Count; i++)
        {
            XmlNode node = xmlLevelsInfo.Item(i);
            XmlNodeList levelData = node.ChildNodes;
            levelsInfo[i] = new LevelInfo();
            // Victory condition
            string victoryConditionString = levelData.Item(0).InnerText;
            switch (victoryConditionString)
            {
                case "Kill Any": levelsInfo[i].victoryCondition = VictoryCondition.DefeatAnyEnemy; break;
            }
            //
            levelsInfo[i].enemiesToDefeat = int.Parse(levelData.Item(1).InnerText);
            //
            levelsInfo[i].attackActionsAvailable = int.Parse(levelData.Item(2).InnerText);
            levelsInfo[i].defenseActionsAvailable = int.Parse(levelData.Item(3).InnerText);
            levelsInfo[i].sprintActionsAvailable = int.Parse(levelData.Item(4).InnerText);
            levelsInfo[i].jumpActionsAvailable = int.Parse(levelData.Item(5).InnerText);
            //
            XmlNodeList xmlEnemies = levelData.Item(6).ChildNodes;
            string[] enemiesNames = new string[xmlEnemies.Count];
            XmlNodeList xmlAmounts = levelData.Item(7).ChildNodes;
            int[] enemiesAmounts = new int[xmlEnemies.Count];
            levelsInfo[i].enemiesToUse = new GameObject[xmlEnemies.Count];
            levelsInfo[i].enemiesToSpawn = new int[xmlEnemies.Count];
            //
            string currentEnemyFolder = (workingWith == WorkingWith.Bugs) ? "Bugs" : "Mekanoids";
            //
            for (int j = 0; j < xmlEnemies.Count; j++)
            {
                enemiesNames[j] = xmlEnemies.Item(j).InnerText;
                // Parece que da problemas leyendo esta parte en la versión compilada
                // TODO: Averiguar que pasa
                levelsInfo[i].enemiesToUse[j] = Resources.Load("Prefabs/Enemies/" + currentEnemyFolder + "/" + enemiesNames[j]) as GameObject;
                //Debug.Log("Level " + i + ": " + levelsInfo[i].enemiesToUse[j]);
                enemiesAmounts[j] = int.Parse(xmlAmounts.Item(j).InnerText);
                levelsInfo[i].enemiesToSpawn[j] = enemiesAmounts[j];
            }
            //
            levelsInfo[i].maxActiveEnemies = int.Parse(levelData.Item(8).InnerText);
            // And done

        }
    }

    //
    public void GetLevelInfoFromLevelData()
    {
        // Cargamos los scriptable objects que hemos creado
        LevelData[] levelData = Resources.LoadAll<LevelData>("LevelData");
        // Preparamos el levels info
        levelsInfo = new LevelInfo[levelData.Length];
        // Y vamos cargando los datos
        for (int i = 0; i < levelData.Length; i++)
        {
            levelsInfo[i] = new LevelInfo();
            // Victory condition
            levelsInfo[i].victoryCondition = levelData[i].victoryCondition;
            //
            levelsInfo[i].enemiesToDefeat = levelData[i].numberToKill;
            //
            levelsInfo[i].attackActionsAvailable = levelData[i].unlockedAttackAbilities;
            levelsInfo[i].defenseActionsAvailable = levelData[i].unlockedDefenseAbilities;
            levelsInfo[i].sprintActionsAvailable = levelData[i].unlockedSprintAbilities;
            levelsInfo[i].jumpActionsAvailable = levelData[i].unlockedJumpAbilities;
            //
            levelsInfo[i].enemiesToUse = levelData[i].enemiesToSpawn;
            levelsInfo[i].enemiesToSpawn = levelData[i].amountToSpawn;
            levelsInfo[i].timeBetweenSpawns = levelData[i].timeBetweenSpawns;
            //
            levelsInfo[i].maxActiveEnemies = levelData[i].maxActiveEnemies;
            // And done

        }
    }

    // Llamada desde el level manager
    public LevelInfo GetCurrentLevelInfo()
    {
        if (currentLevel > -1)
        {
            //
            if (levelsInfo == null)
                GetLevelsInfoFromXML();
            //
            return levelsInfo[currentLevel];
        }            
        else
            return null;
    }

    #endregion
}

public class LevelInfo
{
    //
    public VictoryCondition victoryCondition;
    public int enemiesToDefeat;
    public string enemyIdentifier;
    //
    public int attackActionsAvailable;
    public int defenseActionsAvailable;
    public int jumpActionsAvailable;
    public int sprintActionsAvailable;
    //
    public GameObject[] enemiesToUse;
    public int[] enemiesToSpawn;
    public float timeBetweenSpawns;
    //
    public int maxActiveEnemies;
}