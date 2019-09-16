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

    public LevelData levelData;

    #region Unity Methods

    protected override void Start()
    {
        base.Start();
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
        // Accedemos al nivel
        if(levelData.levelInfo.gameMode == GameMode.Arcade)
            SceneManager.LoadScene("ProtLevel");
        else
            SceneManager.LoadScene("ProtLevel BOSS");
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
        //
        Vector2 nextPosition = new Vector2(Screen.width / 2 + 50, 100);
        Vector2 rectSize = new Vector2(300, 50);
        Rect levelInfoRect = new Rect(nextPosition, rectSize);
        //
        GUI.Label(levelInfoRect, levelData.levelInfo.inGameName, guiSkin.customStyles[3]);
        //
        nextPosition.y += 60;
        levelInfoRect.position = nextPosition;
        GUI.Label(levelInfoRect, "Expected enemies ", guiSkin.label);
        //
        for (int i = 0; i < levelData.levelInfo.enemiesSpawnSettings.Length; i++)
        {
            nextPosition.y += 60;
            levelInfoRect.position = nextPosition;
            GUI.Label(levelInfoRect, levelData.levelInfo.enemiesSpawnSettings[i].enemyPrefab.name, guiSkin.label);
            //GUI.Label(levelInfoRect, i + "", guiSkin.label);
        }
    }

    #endregion
}
