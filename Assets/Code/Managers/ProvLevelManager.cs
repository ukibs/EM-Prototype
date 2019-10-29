using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum VictoryCondition
{
    Invalid = -1,

    DefeatAllEnemies,   // Para esta deberán dejar de spammear en cierto punto
    DefeatAnyEnemy,
    DefeatCertainEnemy,
    FindSomething,
    SlayTheBeast,       // Aquí se controlará el cierre de otrq forma

    Count
}

public class ProvLevelManager : MonoBehaviour
{
    //
    public GUISkin guiSkin;

    //
    private int enemiesToDestroy;
    private int enemiesDestroyed = 0;

    private RobotControl robotControl;
    private VictoryCondition victoryCondition;
    private GameManager gameManager;
    private EnemyManager enemyManager;
    private TerrainManager terrainManager;
    private Fade fade;
    private InputManager inputManager;

    private bool victory = false;
    private bool finished = false;

    // De momento solo para el modo matar cierto enemigo
    private LevelInfo levelInfo;
    private string enemyToKillName;

    public bool Finished { get { return finished; } }
    public bool Victory {  get { return victory; } }

    public VictoryCondition LevelVictoryCondition { get { return victoryCondition; } }

    // Start is called before the first frame update
    void Start()
    {
        //
        robotControl = FindObjectOfType<RobotControl>();
        gameManager = FindObjectOfType<GameManager>();
        enemyManager = GetComponent<EnemyManager>();
        terrainManager = GetComponent<TerrainManager>();
        fade = FindObjectOfType<Fade>();
        inputManager = FindObjectOfType<InputManager>();

        // In arcade mode we get the data from level info list
        //if(gameManager.gameMode == GameMode.Arcade)
        LoadLevelDataArcade();
        // In boss mode...
        /*else
        {

        }*/


        //
        //switch (victoryCondition)
        //{
        //    case VictoryCondition.DefeatAllEnemies:
        //        EnemyConsistency[] enemiesInLevel = FindObjectsOfType<EnemyConsistency>();
        //        enemiesToDestroy = enemiesInLevel.Length;
        //        break;
        //}

        //
        if (victoryCondition == VictoryCondition.DefeatCertainEnemy)
        {
            EnemyConsistency enemyConsistency = levelInfo.enemiesSpawnSettings[0].enemyPrefab.GetComponent<EnemyConsistency>();

            if(enemyConsistency == null)
                enemyConsistency = levelInfo.enemiesSpawnSettings[0].enemyPrefab.GetComponentInChildren<EnemyConsistency>();

            enemyToKillName = enemyConsistency.inGameName;
        }

        // Escodemos el cursor para jugar birn
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Cuqluier botón estando muerto para volver al menu
        //if(Input.anyKey && !PlayerReference.isAlive)
        //{
            // TODO: Meter fade
            //SceneManager.LoadScene("Menu");
            //EndLevel();
        //}
        //
        CheckVictory();
        //
        if(finished && fade.alpha >= 1)
        {
            //SceneManager.LoadScene("Map");
            SceneManager.LoadScene("Main Menu");
        }
        //
        CheckControls();
    }

    // TODO: Dibujar esto en el HUD
    private void OnGUI()
    {
        // TODO: Mover todo esto al HUD
        //if(gameManager.gameMode == GameMode.Arcade)
        if(victoryCondition != VictoryCondition.SlayTheBeast)
        {
            // Ya lo haremos en el hud mas adelante
            GUI.Label(new Rect(Screen.width * 55 / 1000, Screen.height * 20 / 100, 300, 30), "Enemies destroyed", guiSkin.label);
            GUI.Label(new Rect(Screen.width * 55 / 1000, Screen.height * 23 / 100, 300, 30), enemiesDestroyed + "/" + enemiesToDestroy, guiSkin.label);
            //

            // intento de que salgan los enemigos actuales por pantalla
            //int totalActiveEnemies = 0;
            //for (int i = 0; i < enemyManager.ActiveEnemies.Length; i++)
            //    totalActiveEnemies += enemyManager.ActiveEnemies[i];
            int totalActiveEnemies = FindObjectsOfType<EnemyConsistency>().Length;
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height * 75 / 100 + 50, 300, 50),
               "Active Enemies: " + totalActiveEnemies, guiSkin.label);
        }
        else
        {
            // Esto de momento lo mostramos en CarolHelp
        }
        
        // Y pausa
        if (GameControl.paused)
        {
            //
            GUI.Label(new Rect(Screen.width/2 - 150, Screen.height * 4/5, 300, 50), "A/Space to quit", guiSkin.customStyles[3]);
            // Metemos aqui sensibilidad
            string formatedSensivity = inputManager.RightAxisSensivity.ToString("0.00");
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height * 4 / 5 + 100, 300, 50), 
                "Camera sensivity: " + formatedSensivity, guiSkin.label);
           

        }
    }

    #region Methods

    /// <summary>
    /// Load the data from the correspondant level info
    /// </summary>
    void LoadLevelDataArcade()
    {
        // TODO: Revisar como gestionamos esto
        //levelInfo = gameManager.GetCurrentLevelInfo();
        levelInfo = gameManager.CurrentLevelInfo;
        victoryCondition = levelInfo.victoryCondition;
        enemiesToDestroy = levelInfo.enemiesToDefeat;
        // Esto habrá que manejarlo de otro modo
        // Forzamos provisonalmente que se equipen las asignadas
        // Attack
        gameManager.playerAttributes.unlockedAttackActions = levelInfo.attackActionsAvailable;
        // Control chorras para la Build
        if (PlayerReference.playerControl == null)
            PlayerReference.Initiate(FindObjectOfType<RobotControl>().gameObject);
        // Control de habilidades del prota para el formato de niveekes de ka demo
        // Attack
        //if (gameManager.unlockedAttackActions > 1)
        //    PlayerReference.playerControl.ActiveAttackMode = (AttackMode)(gameManager.unlockedAttackActions - 1);
        //// Defense
        //gameManager.unlockedDefenseActions = levelInfo.defenseActionsAvailable;
        //if (gameManager.unlockedDefenseActions > 1)
        //    PlayerReference.playerControl.ActiveDefenseMode = (DefenseMode)(gameManager.unlockedDefenseActions - 1);
        //// Jump
        //gameManager.unlockedJumpActions = levelInfo.jumpActionsAvailable;
        //if (gameManager.unlockedJumpActions > 1)
        //    PlayerReference.playerControl.ActiveJumpMode = (JumpMode)(gameManager.unlockedJumpActions - 1);
        //// Sprint
        //gameManager.unlockedSprintActions = levelInfo.sprintActionsAvailable;
        //if (gameManager.unlockedSprintActions > 1)
        //    PlayerReference.playerControl.ActiveSprintMode = (SprintMode)(gameManager.unlockedSprintActions - 1);
        //
        terrainManager.InitiateManager(levelInfo);
        //
        if (enemyManager != null)
            enemyManager.InitiateManager(levelInfo);
    }

    //
    void LoadLevelDataBossMode()
    {

    }

    //
    void CheckControls()
    {
        // TODO: Controlarlo mejor
        if (inputManager.PauseButton)
        {
            //Debug.Log("Triynig to pause");
            if (!GameControl.paused)
            {
                Time.timeScale = 0;
                GameControl.paused = true;
                // Escodemos el cursor para jugar birn
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1;
                GameControl.paused = false;
                // Escodemos el cursor para jugar birn
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        // Para volver al menu
        if(inputManager.JumpButton && GameControl.paused)
        {
            // Unpause
            GameControl.paused = false;
            Time.timeScale = 1;
            //
            if(EnemyAnalyzer.isActive)
                EnemyAnalyzer.Release();
            //
            SceneManager.LoadScene("Main Menu");
        }
        // 
        if (GameControl.paused)
        {
            inputManager.RightAxisSensivity += inputManager.StickAxis.y * 0.01f;
        }
        //
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (GameControl.bulletTime)
            {
                GameControl.bulletTime = false;
                Time.timeScale = 1;
            }
            else
            {
                GameControl.bulletTime = true;
                Time.timeScale = 0.1f;
            }
        }
    }

    void CheckVictory()
    {
        if(victory == false)
        {
            switch (victoryCondition)
            {
                //case VictoryCondition.DefeatAllEnemies:
                //    // TODO: Este lo cambiaremos
                //    if (enemiesDestroyed >= enemiesToDestroy)
                //    {
                //        victory = true;
                //        gameManager.ProgressInGame();
                //        EndLevel();
                //    }
                //    break;
                // Estas dos se chequean igual aqui
                case VictoryCondition.DefeatCertainEnemy:
                case VictoryCondition.DefeatAnyEnemy:
                    if (enemiesDestroyed >= enemiesToDestroy)
                    {
                        victory = true;
                        gameManager.ProgressInGame();
                        EndLevel();
                    }
                    break;
            }
        }
        
    }

    public void EndLevel()
    {
        //
        if(victory == true)
        {
            // De momento esto
            gameManager.ReceiveExperience(enemiesDestroyed);
        }
        //
        finished = true;
        robotControl.InPlay = false;
        //
        fade.direction = 1;
        fade.speed = 0.5f;
    }

    /// <summary>
    /// 
    /// </summary>
    public void AnnotateKill(string enemyToAnotateName)
    {
        // TODO: Impelemntar filtro de enemigos
        switch (victoryCondition)
        {
            case VictoryCondition.DefeatAllEnemies:
            case VictoryCondition.DefeatAnyEnemy:
                enemiesDestroyed++;
                break;
            // Nota: En estas el objetivo a batir será el primero de la lista
            case VictoryCondition.DefeatCertainEnemy:
                if (enemyToAnotateName == enemyToKillName)
                    enemiesDestroyed++;
                break;
        }
    }

    /// <summary>
    /// Orden de victoria enviada desde un actor de la escena
    /// </summary>
    public void ConfirmVictory()
    {
        victory = true;
        EndLevel();
    }

    #endregion
}
