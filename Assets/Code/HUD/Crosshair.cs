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

    [Header("Rapid Fire Settings")]
    public float maxOmega = 180.0f;
    public float omega = 180.0f;
    public float alpha = 360.0f;
    
    #endregion

    #region Private Variables

    private Sprite interiorSprite;
    private Sprite exteriorCWSprite;
    private Sprite exteriorCCWSprite;
    private GameManager mGameManager;
    private InputManager mInputManager;
    private RobotControl mRobotControl;
    private AttackMode currentAttackMode;
    private float currentInteriorScale;

    private RectTransform interiorRectT;
    private RectTransform exteriorCWRectT;
    private RectTransform exteriorCCWRectT;
    
    #endregion

    #region Monobehaviour
    
    private void Awake()
    {
        //InitializeCrosshair();
    }

    void Start()
    {
        mGameManager = FindObjectOfType<GameManager>();
        mInputManager = FindObjectOfType<InputManager>();
        mRobotControl = FindObjectOfType<RobotControl>();

        InitializeCrosshair();
    }

    void Update()
    {
        // TODO: cambiar el switch a, en vez de funcionar con enteros, funcione con los tipos de ataque del usuario
        switch (currentAttackMode)
        {
            case AttackMode.Invalid:
                // It should never enter this state
                break;

            case AttackMode.RapidFire:
                RapidFireBehaviour();
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
        float t = Time.time;
        // Scale the interior part of the Crosshair
        float easeUp = -(Mathf.Cos(Mathf.PI * 4 * t) + 1);
        float easeDown = (Mathf.Cos(Mathf.PI * 4 * t) + 1);
        float maxScaleOffset = 30;
        float cosDegreesPerSecond = 10;
        
        // Rotate the exterior part
        float maxOmega = 360.0f;
        float omega = 180.0f;
        float alpha = 360.0f;

        // If the player is shooting
        int playerIsShooting = mRobotControl.CurrentActionCharging == ActionCharguing.Attack ? 1 : 0;
        
        //
        float interiorScale = 100 + (playerIsShooting * maxScaleOffset * Mathf.Cos(t * cosDegreesPerSecond));
        interiorRectT.sizeDelta = new Vector2 (interiorScale, interiorScale);
        
        //
        exteriorCWRectT.localRotation = Quaternion.AngleAxis(playerIsShooting * t * maxOmega, Vector3.forward);
        exteriorCCWRectT.localRotation = Quaternion.AngleAxis(-(playerIsShooting * t * maxOmega), Vector3.forward);

    }

    /// <summary>
    /// Applies the UI adjustments to the crosshair
    /// </summary>
    void InitializeCrosshair()
    {
        // Get the Sprites of our prefab
        interiorSprite = transform.GetChild(0).transform.GetChild(0).GetComponent<Sprite>();
        exteriorCWSprite = transform.GetChild(1).transform.GetChild(0).GetComponent<Sprite>();
        exteriorCCWSprite = transform.GetChild(1).transform.GetChild(1).GetComponent<Sprite>();

        // And its Rect Transforms
        interiorRectT = transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>();
        exteriorCWRectT = transform.GetChild(1).transform.GetChild(0).GetComponent<RectTransform>();
        exteriorCCWRectT = transform.GetChild(1).transform.GetChild(1).GetComponent<RectTransform>();

        // Set the rotation of the counter clockwise exterior part of the crosshair
        Vector3 ccwEulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
        transform.GetChild(1).transform.GetChild(1).transform.eulerAngles = ccwEulerAngles;

        // Set the UI Adjustments
        Vector3 newInteriorScale = Vector3.one * interiorScale;
        Vector3 newExteriorScale = Vector3.one * exteriorScale;
        
        // Set the currentInteriorScale to the interiorScale
        currentInteriorScale = interiorScale;

        // Apply these changes
        transform.GetChild(0).transform.GetChild(0).transform.localScale = newInteriorScale;
        transform.GetChild(1).transform.GetChild(0).transform.localScale = newExteriorScale;
        transform.GetChild(1).transform.GetChild(1).transform.localScale = newExteriorScale;

        // Now set the initial attackMode
        currentAttackMode = mRobotControl.ActiveAttackMode;
    }
    
    #endregion
}
