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
// 
public enum GameMode
{
    Invalid = -1,

    Arcade,
    Bosses,

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
    public GameMode gameMode = GameMode.Bosses;

    //
    public WorkingWith workingWith = WorkingWith.Bugs;

    // Acciones desbloqueadas
    public int unlockedJumpActions = 0;
    public int unlockedSprintActions = 0;
    public int unlockedAttackActions = 0;
    public int unlockedDefenseActions = 0;
    //
    public float maxCharge = 1;
    // Armas
    // Puslo
    public float pulseForce = 10;
    // C´ñon
    public float canonBaseMuzzleSpeed = 1000;
    public float canonMinCharge = 0.5f;
    // Fuego rapido
    public float rapidFireRate = 1;
    public float rapidFireMuzzleSpeed = 1000;
    public float maxOverheat = 1;   // NOTA: De momento va ligado al tiempo
    // Perforante
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
    //private LevelInfo[] arcadeLevelsInfo;
    //private LevelInfo[] bossLevelsInfo;

    #endregion

    #region Properties

    public int CurrentExperience { get { return currentExperience; } }
    public int GameProgression { get { return gameProgression; } }
    //public int CurrentLevel { set { currentLevel = value; } }
    public int TotalLevels { get { return levelsInfo.Length; } }
    public LevelInfo[] LevelsInfo { get { return levelsInfo; } }

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
            GetAllLevelInfoFromLevelData();
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
    
    //
    public void GetLevelInfoFromLevelDataDependingOnMode()
    {
        //
        string modeFolder = "";
        switch (gameMode)
        {
            case GameMode.Arcade:
                modeFolder = "Arcade";
                break;
            case GameMode.Bosses:
                modeFolder = "BossMode";
                break;
        }

        // Cargamos los scriptable objects que hemos creado
        LevelData[] levelData = Resources.LoadAll<LevelData>("LevelData/" + modeFolder);
        // Preparamos el levels info
        levelsInfo = new LevelInfo[levelData.Length];
        // Y vamos cargando los datos
        for (int i = 0; i < levelData.Length; i++)
        {
            levelsInfo[i] = levelData[i].levelInfo;
        }
    }

    //
    public void GetAllLevelInfoFromLevelData()
    {
        // Cargamos los scriptable objects que hemos creado
        LevelData[] arcadeLevelData = Resources.LoadAll<LevelData>("LevelData/Arcade");
        LevelData[] bossLevelData = Resources.LoadAll<LevelData>("LevelData/BossMode");
        // Preparamos el levels info
        int totalLength = arcadeLevelData.Length + bossLevelData.Length;
        levelsInfo = new LevelInfo[totalLength];
        // Y vamos cargando los datos
        // Arcade
        int i;
        for (i = 0; i < arcadeLevelData.Length; i++)
        {
            levelsInfo[i] = arcadeLevelData[i].levelInfo;
        }
        // Bosses
        for (; i < totalLength; i++)
        {
            levelsInfo[i] = bossLevelData[i - arcadeLevelData.Length].levelInfo;
        }
    }

    // Llamada desde el level manager
    public LevelInfo GetCurrentLevelInfo()
    {
        if (currentLevel > -1)
        {
            //
            if (levelsInfo == null)
                GetAllLevelInfoFromLevelData();
            //
            return levelsInfo[currentLevel];
        }            
        else
            return null;
    }

    #endregion
}

