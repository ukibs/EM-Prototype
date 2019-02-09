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
    public string levelDestination;
    public string missionName;

    public int levelNumber;

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
                Screen.height - pointInScreen.y - 100, 300, 120), missionName, guiSkin.label);
        }
    }

    #endregion

    #region Methods

    public override void ButtonFunction()
    {
        
        // Accedemos al nivel
        SceneManager.LoadScene(levelDestination);
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

    #endregion
}
