using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum VictoryCondition
{
    Invalid = -1,

    DefeatAllEnemies,
    DefeatAnyEnemy,
    DefeatCertainEnemy,

    Count
}

public class ProvLevelManager : MonoBehaviour
{
    private int enemiesToDestroy;
    private int enemiesDestroyed = 0;

    private RobotControl robotControl;
    private VictoryCondition victoryCondition;
    private GameManager gameManager;
    private EnemyManager enemyManager;
    private Fade fade;

    private bool victory = false;
    private bool finished = false;

    // Start is called before the first frame update
    void Start()
    {
        //
        robotControl = FindObjectOfType<RobotControl>();
        gameManager = FindObjectOfType<GameManager>();
        enemyManager = GetComponent<EnemyManager>();
        fade = FindObjectOfType<Fade>();
        // TODO: Pedir al game manager la info del nivel que toca
        LevelInfo levelInfo = gameManager.GetCurrentLevelInfo();
        victoryCondition = levelInfo.victoryCondition;
        enemiesToDestroy = levelInfo.enemiesToDefeat;
        // Esto habrá que manejarlo de otro modo
        // Forzamos provisonalmente que se equipen las asignadas
        gameManager.unlockedAttackActions = levelInfo.attackActionsAvailable;
        gameManager.unlockedDefenseActions = levelInfo.defenseActionsAvailable;
        gameManager.unlockedJumpActions = levelInfo.jumpActionsAvailable;
        if (gameManager.unlockedJumpActions > 1)
            PlayerReference.playerControl.ActiveJumpMode = (JumpMode)(gameManager.unlockedJumpActions-1);
        gameManager.unlockedSprintActions = levelInfo.sprintActionsAvailable;
        //
        enemyManager.InitiateManager(levelInfo.enemiesToUse, levelInfo.enemiesToSpawn);
        //
        //switch (victoryCondition)
        //{
        //    case VictoryCondition.DefeatAllEnemies:
        //        EnemyConsistency[] enemiesInLevel = FindObjectsOfType<EnemyConsistency>();
        //        enemiesToDestroy = enemiesInLevel.Length;
        //        break;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        //
        if(Input.anyKey && robotControl == null)
        {
            SceneManager.LoadScene("Menu");
        }
        //
        CheckVictory();
        //
        if(finished && fade.alpha >= 1)
        {
            //SceneManager.LoadScene("Map"); mongolin
            SceneManager.LoadScene("Menu");
        }
    }

    private void OnGUI()
    {
        // Ya lo haremos en el hud mas adelante
        GUI.Label(new Rect(30, 100, 200, 20), "Enemies destroyed: " + enemiesDestroyed);

    }

    void CheckVictory()
    {
        if(victory == false)
        {
            switch (victoryCondition)
            {
                case VictoryCondition.DefeatAllEnemies:
                    // TODO: Este lo cambiaremos
                    if (enemiesDestroyed >= enemiesToDestroy)
                    {
                        victory = true;
                        //EndLevel();
                    }
                    break;
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
    /// Ya haremos que pida el tipo de enemigo
    /// </summary>
    public void AnnotateKill()
    {
        // TODO: Impelemntar filtro de enemigos
        if (victoryCondition == VictoryCondition.DefeatAllEnemies) { }
        enemiesDestroyed++;
    }
}
