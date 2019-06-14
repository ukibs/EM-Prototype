using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple class to make objects targeteable by the player
/// Scripts like Enemy Consistency will inherit form it
/// </summary>
public class Targeteable : MonoBehaviour
{
    // Para poder hacer que se los pueda targetear según queramos
    public bool active = true;
}
