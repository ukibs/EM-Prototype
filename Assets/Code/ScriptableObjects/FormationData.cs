using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Formation", menuName = "FormationData", order = 1)]
public class FormationData : ScriptableObject
{
    public FormationInfo formationInfo;
}

[System.Serializable]
public class FormationInfo
{
    public FormationType formationType;
    public float distanceBetweenMembers = 1;
    public int maxMembersPerRow = 9;
}

public enum FormationType
{
    Invalid = -1,

    Arrow,
    Circle,

    Count
}