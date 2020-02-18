using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LevelStatus
{
    Invalid = -1,

    Locked, 
    Unlocked,
    Completed,
    ExtraCompleted,

    Count
}

public class LevelSelectionItem : MapAndStatsItem
{
    // Provisional
    //public string levelDestination;
    //public string missionName;

    //public int levelNumber;

    public int levelOrder;          // De momento lo menajmos aqui
    public LevelData levelData;
    public float yOffset = 40f;

    private Camera mainCamera;
    private Vector3 mcInitialPositon;

    #region Unity Methods

    protected override void Start()
    {
        base.Start();

        mainCamera = Camera.main;
        mcInitialPositon = mainCamera.transform.position;

        //
        if (GameManager.instance.GameProgression < levelOrder)
            gameObject.SetActive(false);
        else
            Debug.Log("Level active");
    }

    protected void Update()
    {
        //
        if (highLighted &&
            (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump")))
        {
            ButtonFunction();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.name.Equals("Map Player Variant"))
        {
            HighlightButton();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.name.Equals("Map Player Variant"))
        {
            UnHighlightButton();
        }
    }

    private void OnGUI()
    {
        //
        if (highLighted)
        {
            //
            Vector3 pointInScreen = Camera.main.WorldToScreenPoint(transform.position);
            //
            GUI.Label(new Rect(pointInScreen.x - 150, 
                Screen.height - pointInScreen.y - 100, 300, 120), levelData.levelInfo.inGameName, guiSkin.label);
            //
            ShowLevelInfo();
        }
    }

    #endregion

    #region Methods

    public override void ButtonFunction()
    {
        // Cargamos los datos
        GameManager.instance.CurrentLevelInfo = levelData.levelInfo;
        GameManager.instance.gameMode = levelData.levelInfo.gameMode;
        // Accedemos al nivel
        StartCoroutine(LoadLevel("ProtLevel"));
    }

    public override void HighlightButton()
    {
        base.HighlightButton();
        highLighted = true;
    }

    public override void UnHighlightButton()
    {
        base.UnHighlightButton();
        highLighted = false;
    }

    // TODO: MIrarlo y dejarlo bien hecho
    // Intentar no repetir nombres y demás
    void ShowLevelInfo()
    {
        Vector2 pos = new Vector2(100, 100);
        //
        Vector2 rectSize = new Vector2(300, 50);
        Rect levelInfoRect = new Rect(pos, rectSize)
        {
            position = pos
        };
        // Nombre del nivel
        GUI.Label(levelInfoRect, levelData.levelInfo.inGameName, guiSkin.customStyles[3]);
        //
        string message = "";
        switch (levelData.levelInfo.victoryCondition)
        {
            case VictoryCondition.DefeatAnyEnemy:
                message = "Defeat " + levelData.levelInfo.enemiesToDefeat + " enemies";
                break;
            case VictoryCondition.SlayTheBeast:
                message = "Slay the beast";
                break;
        }

        //
        pos.y += yOffset;
        levelInfoRect.position = pos;
        GUI.Label(levelInfoRect, message, guiSkin.customStyles[1]);
        //
        pos.y += yOffset;
        levelInfoRect.position = pos;
        GUI.Label(levelInfoRect, levelData.levelInfo.description, guiSkin.customStyles[0]);
        //
        pos.y += yOffset;
        levelInfoRect.position = pos;
        GUI.Label(levelInfoRect, "Expected enemies ", guiSkin.label);
        //
        for (int i = 0; i < levelData.levelInfo.enemiesSpawnSettings.Length; i++)
        {
            pos.y += yOffset;
            levelInfoRect.position = pos;
            // TODO: Sacar el nombre con el in-game name
            EnemyConsistency enemyConsistency = levelData.levelInfo.enemiesSpawnSettings[i].enemyPrefab.GetComponent<EnemyConsistency>();
            //
            string nameToShow = "";
            if (enemyConsistency != null && enemyConsistency.inGameName != "Size Category")
                nameToShow = enemyConsistency.inGameName;
            else
                nameToShow = levelData.levelInfo.enemiesSpawnSettings[i].enemyPrefab.name;
            //
            GUI.Label(levelInfoRect, nameToShow, guiSkin.customStyles[0]);
            //GUI.Label(levelInfoRect, i + "", guiSkin.label);
        }
    }

    #endregion

    #region AsyncLoad Methods

    public IEnumerator LoadLevel(string level)
    {
        //levelMenu.SetActive(true);

        float counter = 1;
        float currentCounter = 0;

        // Set the level number in the game manager
        GameManager.instance.CurrentLevel = levelOrder;
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(level);

        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            mainCamera.transform.position = 
                Vector3.Lerp(mcInitialPositon, transform.position, Mathf.Min(asyncLoad.progress, currentCounter));
            currentCounter += Time.deltaTime;

            if (asyncLoad.progress >= .9f && currentCounter >= counter)
            {
                //loadingText.text = "Press any key to continue";
                //loadingIcon.SetActive(false);

                //if (Input.anyKeyDown)
                //{
                    asyncLoad.allowSceneActivation = true;

                //    Time.timeScale = 1f;
                //}
            }

            yield return null;
        }
    }

    #endregion
}
