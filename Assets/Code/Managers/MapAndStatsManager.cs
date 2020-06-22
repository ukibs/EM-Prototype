using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAndStatsManager : MonoBehaviour
{
    //public RectTransform mapTransform;
    //public RectTransform playerBackgroundTransform;
    public float transitionTime = 0.5f;
    public GUISkin guiSkin;
    // Provisional
    //public GameObject canonButton;

    private GameManager gameManager;
    private Transform cameraTransform;
    private Vector3[] playerAbilitiesPositions;
    private int positionsIndex = 0;
    private float timeFromTransitionOrder = 0;
    //private Stat 
    private bool playerLocked = false;

    private bool levelHighlighted;

    #region Properties

    public bool PlayerLocked
    {
        get { return playerLocked; }
        set { playerLocked = value; }
    }

    public bool InMap
    {
        get
        {
            return positionsIndex == 0;
        }
    }

    #endregion

    public bool LevelHighlighted
    {
        get { return levelHighlighted; }
        set { levelHighlighted = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        //
        gameManager = GameManager.instance;
        // Ajustamos estas dos cosas al tamaño de pantalla
        //mapTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        //playerBackgroundTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        //playerBackgroundTransform.position = new Vector3(Screen.width * 1.5f, 0, 0);
        //
        cameraTransform = Camera.main.transform;
        //
        playerAbilitiesPositions = new Vector3[2];
        //cameraPositions[0] = new Vector3(0, 0, 0);
        //cameraPositions[1] = new Vector3(Screen.width * 1.5f, 0, 0);
        playerAbilitiesPositions[0] = cameraTransform.GetChild(0).localPosition;
        playerAbilitiesPositions[1] = new Vector3(playerAbilitiesPositions[0].x - 15, playerAbilitiesPositions[0].y, playerAbilitiesPositions[0].z);
        //
        CheckUnlockedStuff();
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        if (Input.anyKeyDown)
        {
            SwitchSection();
        }
        //
        timeFromTransitionOrder += dt;
        //
        //cameraTransform.position = Vector3.Lerp(cameraTransform.position, 
        //    cameraPositions[positionsIndex], timeFromTransitionOrder / transitionTime);
        cameraTransform.GetChild(0).localPosition = Vector3.Lerp(cameraTransform.GetChild(0).localPosition,
            playerAbilitiesPositions[positionsIndex], timeFromTransitionOrder / transitionTime);
    }

    private void OnGUI()
    {
        //
        if (levelHighlighted)
        {

        }
        //
        if (positionsIndex == 1)
            ShowUpgradeSection();
    }

    #region Methods

    //
    public void SwitchSection()
    {
        positionsIndex++;
        positionsIndex = positionsIndex % playerAbilitiesPositions.Length;
        //Debug.Log("Positions index: " + positionsIndex);
        timeFromTransitionOrder = 0;
    }

    //
    //public void ActivateNextWeapon()
    //{
    //    GameManager.instance.playerAttributes.unlockedAttackActions++;
    //    canonButton.SetActive(true);
    //}

    //
    void CheckUnlockedStuff()
    {

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
        if (GUI.Button(new Rect(Screen.width / 2, baseHeight + 30, 30, 30), "+", guiSkin.button))
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
           muzzleSpeed + " m/s", guiSkin.label);
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

    #endregion
}
