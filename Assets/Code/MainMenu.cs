using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string currentVersion = "0.2";
    public GUISkin guiSkin;

    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        //
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void OnGUI()
    {
        // TODO: Que pinte botones para los nieveles
        // Title
        GUI.Label(new Rect(Screen.width * 1 / 8, Screen.height * 1 / 4, 500, 50), "E. M. PROTOTYPE", guiSkin.customStyles[3]);
        GUI.Label(new Rect(Screen.width * 1 / 8 + 100, Screen.height * 1 / 4 + 50, 200, 30), "Prototype " + currentVersion, guiSkin.label);
        //
        Vector2 buttonSize = new Vector2(300, 50);
        //
        for(int i = 0; i <= gameManager.GameProgression; i++)
        {
            if (GUI.Button(new Rect(Screen.width * 1 / 8, Screen.height * 1 / 2 + (i * 50), buttonSize.x, buttonSize.y), "Level " + i, guiSkin.button))
            {
                gameManager.SelectLevel(i);
                SceneManager.LoadScene("ProtLevel");
            }
        }
        
        //
        //if (GUI.Button(new Rect(Screen.width * 1/8, Screen.height * 1/2, buttonSize.x, buttonSize.y), "TUTORIAL", guiSkin.button)){
        //    SceneManager.LoadScene("Balls Tutorial");
        //}

        ////
        //if (GUI.Button(new Rect(Screen.width * 1 / 8, Screen.height * 2 / 3, buttonSize.x, buttonSize.y), 
        //    "TUTORIALS ARE FOR PUSSIES", guiSkin.button))
        //{
        //    SceneManager.LoadScene("Cinematic Test");
        //}
    }
}
