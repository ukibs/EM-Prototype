using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    #region UI

    #endregion

    #region Public Variables
    
    [Header("UI Adjustments")]
    [Range(0.25f, 1.0f)] public float interiorScale;
    [Range(0.25f, 1.0f)] public float exteriorScale;
    [Range(0.5f, 1.0f)] public float rapidFireMaxScale;
    #endregion

    #region Private Variables

    private GameManager mGameManager;
    private InputManager mInputManager;
    private AttackMode attackModes;
    
    #endregion

    #region Monobehaviour
    
    private void Awake()
    {
        InitializeCrosshair();
    }

    void Start()
    {
        mGameManager = FindObjectOfType<GameManager>();
        mInputManager = FindObjectOfType<InputManager>();
    }

    void Update()
    {
        // TODO: cambiar el switch a, en vez de funcionar con enteros, funcione con los tipos de ataque del usuario
        switch (attackModes)
        {
            case AttackMode.Invalid:
                // It should never enter this state
                break;

            case AttackMode.RapidFire:
                break;
            
            case AttackMode.Pulse:
                break;
            
            case AttackMode.Canon:
                break;
            
            case AttackMode.ParticleCascade:
                break;
        }
    }
    
    #endregion

    #region Methods

    /// <summary>
    /// Behaviour of the crosshair when RapidFire is selected
    /// </summary>
    void RapidFireBehaviour()
    {
        #region Interior

        float maxScale = rapidFireMaxScale;
        float freq = mGameManager.playerAttributes.rapidFireRate.baseValue;
        

        #endregion

        #region Exterior

        

        #endregion
    }
    
    /// <summary>
    /// Applies the UI adjustments to the crosshair
    /// </summary>
    void InitializeCrosshair()
    {
        // Set the rotation of the counter clockwise exterior part of the crosshair
        Vector3 ccwEulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
        transform.GetChild(1).transform.GetChild(1).transform.eulerAngles = ccwEulerAngles;

        // Apply the UI Adjustments
        Vector3 newInteriorScale = Vector3.one * interiorScale;
        Vector3 newExteriorScale = Vector3.one * exteriorScale;

        transform.GetChild(0).transform.GetChild(0).transform.localScale = newInteriorScale;
        transform.GetChild(1).transform.GetChild(0).transform.localScale = newExteriorScale;
        transform.GetChild(1).transform.GetChild(1).transform.localScale = newExteriorScale;
    }
    
    #endregion
}
