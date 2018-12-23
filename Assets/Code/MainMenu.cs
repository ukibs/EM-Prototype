using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string currentVersion = "1.1";
    public GUISkin guiSkin;

    // Start is called before the first frame update
    void Start()
    {
        
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
        //
        Vector2 buttonSize = new Vector2(300, 100);
        //
        GUI.Label(new Rect(Screen.width * 1 / 8, Screen.height * 1/4, 500, 50), "E. M. PROTOTYPE", guiSkin.customStyles[3]);
        GUI.Label(new Rect(Screen.width * 1 / 8 + 100, Screen.height * 1 / 4 + 50, 200, 30), "Prototype " + currentVersion, guiSkin.label);
        //
        if (GUI.Button(new Rect(Screen.width * 1/8, Screen.height * 1/2, buttonSize.x, buttonSize.y), "TUTORIAL", guiSkin.button)){
            SceneManager.LoadScene("Balls Tutorial");
        }

        //
        if (GUI.Button(new Rect(Screen.width * 1 / 8, Screen.height * 2 / 3, buttonSize.x, buttonSize.y), 
            "TUTORIALS ARE FOR PUSSIES", guiSkin.button))
        {
            SceneManager.LoadScene("Cinematic Test");
        }
    }
}
