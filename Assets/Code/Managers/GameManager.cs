using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// De momento lo manejamos con esto
public enum GameProgression
{
    Invalid = -1,

    FirstIntro,
    FirstFight,
    DiscoveringMap,

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
    public int rapidFireMuzzleSpeed = 3000;
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

    private LevelStatus[] levelsStatus;
    private int currentExperience = 0;
    // TODO: Decidir como manejar esto
    private int gameProgression = 0;

    #endregion

    #region Properties

    public int CurrentExperience { get { return currentExperience; } }
    public int GameProgression { get { return gameProgression; } }

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
    }

    private void Start()
    {
        // De momento dos niveles
        levelsStatus = new LevelStatus[2];
        //

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
        gameProgression++;
    }

    #endregion
}
