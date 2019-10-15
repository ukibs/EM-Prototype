using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ficha para tener mayor flexibilidad con el sistema de armas
/// </summary>
[CreateAssetMenu(fileName ="Weapon", menuName = "WeaponData", order = 1)]
public class WeaponData : ScriptableObject
{
    // Parte de arma ------------------------------------------------------------------------------

    public Weapon weapon;

    // Parte de proyectil ------------------------------------------------------------------------

    public Proyectile proyectile;

    
}

[System.Serializable]
public class Weapon
{
    [Tooltip("Bullets per second.")]
    public float rateOfFire = 2;    // Bullets per second
    [Tooltip("Minimum range for weapon starting to attack.")]
    public float range = 50;
    public GameObject proyectilePrefab;
    // Probablemente guardemos estos valores en la balas
    public float shootForce = 10;
    public float muzzleSpeed = 500;
    //
    public TypeOfFire typeOfFire = TypeOfFire.Direct;
    //
    public FiringSystem firingSystem = FiringSystem.Autonomous;

    public ShootCalculation shootCalculation = ShootCalculation.MuzzleSpeed;
    public GameObject shootParticlesPrefab;
    //
    public AudioClip shootingClip;
}

[System.Serializable]
public class Proyectile
{
    // Seguramente metamos también la velocidad de salida
    [Tooltip("Diameter in mm")]
    public float diameter;
    [Tooltip("Short for quick bullets long for artillery and other slow ones")]
    public float lifeTime = 10;
    // De momento lo manejamos así
    public bool dangerousEnough = false;
    //
    public GameObject impactParticlesPrefab;
    public GameObject bulletHolePrefab;
    //
    public GameObject impactOnBugParticlesPrefab;
    public GameObject bulletHoleBugPrefab;

    // De momento hacemos dos, para player y enemigo
    // TODO: hacer un atrapado más general
    public AudioClip impactOnPlayer;
    public AudioClip impactOnEnemy;
}