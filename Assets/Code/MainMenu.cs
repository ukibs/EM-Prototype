using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class MainMenu : MonoBehaviour
{
    public string currentVersion = "0.2.1";
    public GUISkin guiSkin;
    public Vector2 rowsAndColumns = new Vector2(4, 3);
    public Vector2 buttonSize = new Vector2(300, 50);

    private GameManager gameManager;
    //
    private LevelData[] arcadeLevels;
    private LevelData[] bossLevels;

    //
    private List<Rect> buttonsVisualCoordinates;
    private List<Rect> buttonsFunctionalCoordinates;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        //
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //
        buttonsFunctionalCoordinates = new List<Rect>();
        buttonsVisualCoordinates = new List<Rect>();
        //
        //DetermineButtonsPositions();
        DetermineButtonsPositionsSettingRowsAndColumns();
    }

    // Update is called once per frame
    void Update()
    {
        // Ñapa tapida para cerrar el juego
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
        
        // Depending on the gamne mode
        //ShowLevelsAccordingToGameMode();
        ShowAllLevels();

        //
        CheckMouseOverButtons();
    }

    //
    void ShowLevelsAccordingToGameMode()
    {
        switch (gameManager.gameMode)
        {
            case GameMode.Arcade:
                // Arcade levels
                for (int i = 0; i <= gameManager.GameProgression; i++)
                {
                    // TODO: Que saque bien el número - nombre
                    if (GUI.Button(new Rect(Screen.width * 1 / 8, Screen.height * 1 / 2 + (i * 50), buttonSize.x, buttonSize.y),
                        "Level " + i, guiSkin.button))
                    {
                        gameManager.SelectLevel(i);
                        SceneManager.LoadScene("ProtLevel");
                    }
                }
                break;
            case GameMode.Bosses:
                if (GUI.Button(new Rect(Screen.width * 1 / 8, Screen.height * 1 / 2 + 50, buttonSize.x, buttonSize.y), "Boss test ", guiSkin.button))
                {
                    SceneManager.LoadScene("ProtLevel BOSS");
                }
                break;
        }
    }

    //
    void ShowAllLevels()
    {
        //
        for(int i = 0; i < gameManager.LevelsInfo.Length; i++)
        {
            //GUI.Label(levelInfoRect, i + "", guiSkin.label);
            // 
            if (GUI.Button(buttonsVisualCoordinates[i],
                i + "", guiSkin.button))
            {
                //
                if(gameManager.LevelsInfo[i].gameMode == GameMode.Arcade)
                {
                    gameManager.SelectLevel(i);
                    SceneManager.LoadScene("ProtLevel");
                }
                //
                else
                {
                    SceneManager.LoadScene("ProtLevel BOSS");
                }
            }
        }
    }

    // Determinaremos tanto a nivel visual como funcional
    // Puto junity y su coordenada y cambiada
    void DetermineButtonsPositions()
    {
        //
        for (int i = 0; i < gameManager.LevelsInfo.Length; i++)
        {
            //
            Rect newButtonVisualCoordinates = new Rect(Screen.width * 1 / 8, Screen.height * 1 / 2 + (i * 50), buttonSize.x, buttonSize.y);
            buttonsVisualCoordinates.Add(newButtonVisualCoordinates);
            //
            Rect newButtonFunctionalCoordinates = new Rect(Screen.width * 1 / 8, 
                Screen.height - newButtonVisualCoordinates.y - newButtonVisualCoordinates.height, 
                buttonSize.x, buttonSize.y);
            buttonsFunctionalCoordinates.Add(newButtonFunctionalCoordinates);
        }
    }

    //
    void DetermineButtonsPositionsSettingRowsAndColumns()
    {
        // Filas
        for (int i = 0; i < rowsAndColumns.y; i++)
        {
            // Y columnas
            for(int j = 0; j < rowsAndColumns.x; j++)
            {
                // Para que no se salga del array de levels info en caso de tener más
                if(i * (int)rowsAndColumns.y + j < gameManager.LevelsInfo.Length)
                {
                    //
                    Rect newButtonVisualCoordinates = new Rect(Screen.width * 1 / 8 + (j * buttonSize.x), 
                        Screen.height * 1 / 2 + (i * buttonSize.y), 
                        buttonSize.x, buttonSize.y);
                    buttonsVisualCoordinates.Add(newButtonVisualCoordinates);
                    //
                    Rect newButtonFunctionalCoordinates = new Rect(newButtonVisualCoordinates.x,
                        Screen.height - newButtonVisualCoordinates.y - newButtonVisualCoordinates.height,
                        buttonSize.x, buttonSize.y);
                    buttonsFunctionalCoordinates.Add(newButtonFunctionalCoordinates);
                }                
            }            
        }
    }

    //
    void CheckMouseOverButtons()
    {
        //
        Vector2 mouseCoordinates = Input.mousePosition;
        //
        for (int i = 0; i < buttonsFunctionalCoordinates.Count - 1; i++)
        {
            if(mouseCoordinates.x > buttonsFunctionalCoordinates[i].x &&
                mouseCoordinates.x < buttonsFunctionalCoordinates[i].x + buttonsFunctionalCoordinates[i].width &&
                mouseCoordinates.y > buttonsFunctionalCoordinates[i].y &&
                mouseCoordinates.y < buttonsFunctionalCoordinates[i].y + buttonsFunctionalCoordinates[i].height)
            {
                ShowLevelInfo(i);
            }
        }
    }

    // TODO: MIrarlo y dejarlo bien hecho
    // Intentar no repetir nombres y demás
    void ShowLevelInfo(int index)
    {
        //
        Vector2 nextPosition = new Vector2(Screen.width / 2 + 50, 100);
        Vector2 rectSize = new Vector2(300, 50);
        Rect levelInfoRect = new Rect(nextPosition, rectSize);
        //
        GUI.Label(levelInfoRect, gameManager.LevelsInfo[index].inGameName, guiSkin.customStyles[3]);
        //
        nextPosition.y += 60;
        levelInfoRect.position = nextPosition;
        GUI.Label(levelInfoRect, "Expected enemies ", guiSkin.label);
        //
        for(int i = 0; i < gameManager.LevelsInfo[index].enemyGroups.Length; i++)
        {
            nextPosition.y += 60;
            levelInfoRect.position = nextPosition;
            GUI.Label(levelInfoRect, gameManager.LevelsInfo[index].enemyGroups[i].name, guiSkin.label);
            //GUI.Label(levelInfoRect, i + "", guiSkin.label);
        }
    }
}
