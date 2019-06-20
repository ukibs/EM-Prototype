using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarolStep", menuName = "CarolStep", order = 1)]
public class CarolStepObject : ScriptableObject
{
    public CarolStep carolStep;
}

/// <summary>
/// Clase para definir pasos de ayuda de Carol desde el editor
/// </summary>
[System.Serializable]
public class CarolStep
{
    public HelpTrigger helpTrigger;
    public AudioClip audioClip;
    public string stepText;
    [Tooltip("Para casos en los que se necesite más de un X. También vale para temporizador")]
    public int amount = 0;
}