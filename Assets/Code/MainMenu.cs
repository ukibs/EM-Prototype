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
        if (!CheckMouseOverButtons())
            ShowUpgradeSection();
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
    bool CheckMouseOverButtons()
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
                return true;
            }
        }
        //
        return false;
    }

    // TODO: MIrarlo y dejarlo bien hecho
    // Intentar no repetir nombres y demás
    void ShowLevelInfo(int index)
    {
        //
        if (gameManager.LevelsInfo == null) return;
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
        for(int i = 0; i < gameManager.LevelsInfo[index].enemiesSpawnSettings.Length; i++)
        {
            nextPosition.y += 60;
            levelInfoRect.position = nextPosition;
            GUI.Label(levelInfoRect, gameManager.LevelsInfo[index].enemiesSpawnSettings[i].enemyPrefab.name, guiSkin.label);
            //GUI.Label(levelInfoRect, i + "", guiSkin.label);
        }
    }

    // Aqui meteremos el sistema de mejora
    void ShowUpgradeSection()
    {
        //
        float baseHeight = 100;
        // Palas

        // Atributos a mejorar
        // Force per second
        GUI.Label(new Rect(Screen.width / 2, baseHeight, 300, 30), "Force per second: ", guiSkin.label);
        GUI.Label(new Rect(Screen.width / 2, baseHeight + 30, 300, 30), gameManager.playerAttributes.forcePerSecond.CurrentValue + " N", guiSkin.label);
        if(GUI.Button(new Rect(Screen.width / 2, baseHeight + 30, 30, 30), "+", guiSkin.button))
        {
            gameManager.playerAttributes.forcePerSecond.improvementsPurchased++;
        }
        // Mass per second
        GUI.Label(new Rect(Screen.width / 2, baseHeight + 60, 300, 30), "Mass per second: ", guiSkin.label);
        GUI.Label(new Rect(Screen.width / 2, baseHeight + 90, 300, 30), gameManager.playerAttributes.massPerSecond + " g", guiSkin.label);
        // Rapid fire rate of fire
        GUI.Label(new Rect(Screen.width / 2, baseHeight + 120, 300, 30), "Rapid fire rate: ", guiSkin.label);
        GUI.Label(new Rect(Screen.width / 2, baseHeight + 150, 300, 30), gameManager.playerAttributes.rapidFireRate.CurrentValue + " bullets/s", guiSkin.label);
        if (GUI.Button(new Rect(Screen.width / 2, baseHeight + 150, 30, 30), "+", guiSkin.button))
        {
            gameManager.playerAttributes.rapidFireRate.improvementsPurchased++;
        }

        // Resultados de la mejora de los atributos
        // Velocida de salida
        float muzzleSpeed = (gameManager.playerAttributes.forcePerSecond.CurrentValue / (gameManager.playerAttributes.massPerSecond / 1000));
        GUI.Label(new Rect(Screen.width / 2 + 300, baseHeight, 300, 30), "Muzzle speed: ", guiSkin.label);
        GUI.Label(new Rect(Screen.width / 2 + 300, baseHeight + 30, 300, 30), 
           muzzleSpeed  + " m/s", guiSkin.label);
        // Energía cinética
        float kineticEnergy = GeneralFunctions.GetBodyKineticEnergy(muzzleSpeed, gameManager.playerAttributes.massPerSecond) / 1000000;
        GUI.Label(new Rect(Screen.width / 2 + 300, baseHeight + 60, 300, 30), "Proyectile K energy: ", guiSkin.label);
        GUI.Label(new Rect(Screen.width / 2 + 300, baseHeight + 90, 300, 30),
            kineticEnergy + " kJ", guiSkin.label);
        // Energía cinética (fuego rápido)
        float massPerBullet = gameManager.playerAttributes.massPerSecond * (1 / gameManager.playerAttributes.rapidFireRate.CurrentValue);
        float rapidFireKineticEnergy = GeneralFunctions.GetBodyKineticEnergy(muzzleSpeed, massPerBullet) / 1000000;
        GUI.Label(new Rect(Screen.width / 2 + 300, baseHeight + 120, 300, 30), "Proyectile K energy: ", guiSkin.label);
        GUI.Label(new Rect(Screen.width / 2 + 300, baseHeight + 150, 300, 30),
            rapidFireKineticEnergy + " kJ", guiSkin.label);


        // TODO: Meter la potencia de la bala

        // Carga máxima
        GUI.Label(new Rect(Screen.width / 2, baseHeight + 180, 300, 30), "Max paddle charge: ", guiSkin.label);
        GUI.Label(new Rect(Screen.width / 2, baseHeight + 210, 300, 30),
            gameManager.playerAttributes.maxCharge + " s", guiSkin.label);

        //Cuerpo EM
        // Fuerza para mover cuerpo
        GUI.Label(new Rect(Screen.width / 2, baseHeight + 240, 300, 30), "Body movement force: ", guiSkin.label);
        GUI.Label(new Rect(Screen.width / 2, baseHeight + 270, 300, 30),
            gameManager.playerAttributes.movementForcePerSecond + " N/s", guiSkin.label);

        // Vida y escudo
        // Kinetick shield energy
        GUI.Label(new Rect(Screen.width / 2, baseHeight + 300, 300, 30), "Kinetic shield max energy: ", guiSkin.label);
        GUI.Label(new Rect(Screen.width / 2, baseHeight + 330, 300, 30),
            gameManager.playerAttributes.maxShield.CurrentValue + " J", guiSkin.label);
        if (GUI.Button(new Rect(Screen.width / 2, baseHeight + 330, 30, 30), "+", guiSkin.button))
        {
            gameManager.playerAttributes.maxShield.improvementsPurchased++;
        }

        GUI.Label(new Rect(Screen.width / 2, baseHeight + 360, 300, 30), "Kinetic shield recharge rate: ", guiSkin.label);
        GUI.Label(new Rect(Screen.width / 2, baseHeight + 390, 300, 30),
            gameManager.playerAttributes.shieldRechargeRate + " J/s", guiSkin.label);

    }
}
