using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum VictoryCondition
{
    Invalid = -1,

    DefeatAllEnemies,

    Count
}

public class ProvLevelManager : MonoBehaviour
{
    private int enemiesToDestroy;
    private int enemiesDestroyed = 0;

    private RobotControl robotControl;
    private VictoryCondition victoryCondition;
    private GameManager gameManager;
    private Fade fade;

    private bool victory = false;
    private bool finished = false;

    // Start is called before the first frame update
    void Start()
    {
        //
        robotControl = FindObjectOfType<RobotControl>();
        gameManager = FindObjectOfType<GameManager>();
        fade = FindObjectOfType<Fade>();
        //

        //
        switch (victoryCondition)
        {
            case VictoryCondition.DefeatAllEnemies:
                EnemyConsistency[] enemiesInLevel = FindObjectsOfType<EnemyConsistency>();
                enemiesToDestroy = enemiesInLevel.Length;
                break;
        }
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
            SceneManager.LoadScene("Map");
        }
    }

    private void OnGUI()
    {
        // Ya lo haremos en el hud mas adelante
        GUI.Label(new Rect(30, 100, 200, 20), "Enemies destroyed: " + enemiesDestroyed);

    }

    void CheckVictory()
    {
        switch (victoryCondition)
        {
            case VictoryCondition.DefeatAllEnemies:
                if(enemiesDestroyed >= enemiesToDestroy)
                {
                    victory = true;
                    //EndLevel();
                }
                break;
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
        enemiesDestroyed++;
    }
}
