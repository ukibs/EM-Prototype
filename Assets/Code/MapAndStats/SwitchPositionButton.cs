using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchPositionButton : MapAndStatsItem
{
    #region Methods

    public override void ButtonFunction()
    {
        mapAndStatsManager.SwitchSection();
    }
    

    #endregion
}
