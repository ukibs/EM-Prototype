using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityButton : MapAndStatsItem
{

    public AttackMode attackRelated;

    private string nameInMenu;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        switch (attackRelated)
        {
            case AttackMode.Pulse:
                nameInMenu = "Pulse";
                break;
            case AttackMode.Canon:
                nameInMenu = "Canon";
                break;
        }
    }

    // Update is called once per frame
    void OnGUI()
    {
        //
        Vector3 pointInScreen = Camera.main.WorldToScreenPoint(transform.position);
        //
        GUI.Label(new Rect(pointInScreen.x - 300,
            Screen.height - pointInScreen.y - 50, 300, 120), nameInMenu, guiSkin.label);
    }

    #region Methods

    public override void ButtonFunction()
    {
        // Luego hacemos esto bien
        if(GameManager.instance.CurrentExperience >= 10)
        {

        }
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
