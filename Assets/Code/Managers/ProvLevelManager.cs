using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProvLevelManager : MonoBehaviour
{
    private int enemiesDestroyed = 0;
    private RobotControl robotControl;

    // Start is called before the first frame update
    void Start()
    {
        robotControl = FindObjectOfType<RobotControl>();   
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.anyKey && robotControl == null)
        {
            SceneManager.LoadScene("Menu");
        }
    }

    private void OnGUI()
    {
        // Ya lo haremos en el hud mas adelante
        GUI.Label(new Rect(30, 100, 200, 20), "Enemies destroyed: " + enemiesDestroyed);

    }

    /// <summary>
    /// Ya haremos que pida el tipo de enemigo
    /// </summary>
    public void AnnotateKill()
    {
        enemiesDestroyed++;
    }
}
