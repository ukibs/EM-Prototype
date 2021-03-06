﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    #region Private Attributes

    Vector2 stickAxis = new Vector2();
    bool sprintButton;
    bool jumpButton;
    bool fireButton;
    bool fireButtonDown;
    Vector2 mouseMovement = new Vector2();

    bool switchWeaponButton;
    bool switchDefenseButton;
    bool switchJumpButton;
    bool switchSprintButton;

    bool markObjectiveButton;
    //bool switchObjectiveButton;   // Este lo aplicaremos más adelante
    bool defenseButton;
    Vector2 rightStickAxis = new Vector2();
    Vector2 crossAxis = new Vector2();
    bool menuButton;
    //
    private bool changeAttackDown;
    private bool changeDefenseDown;
    private bool changeSprintDown;
    private bool changeJumpDown;
    //
    //private Vector2 previousMousePosition;
    private Vector2 previousCrossAxis;
    //
    private bool pauseButton;
    // Ahora revisamos donde meterla
    private float rightAxisSensivity = 1.0f;

    #endregion

    #region Properties

    public Vector2 StickAxis { get { return stickAxis; } }
    public bool SprintButton { get { return sprintButton; } }
    public bool JumpButton { get { return jumpButton; } }
    public bool FireButton { get { return fireButton; } }
    public bool FireButtonDown { get { return fireButtonDown; } }
    public Vector2 MouseMovement { get { return mouseMovement; } }

    public bool MarkObjectiveButton { get { return markObjectiveButton; } }
    public bool DefenseButton { get { return defenseButton; } }
    public Vector2 RightStickAxis { get { return rightStickAxis; } }
    public Vector2 CrossAxis { get { return crossAxis; } }
    public bool MenuButton { get { return menuButton; } }

    //
    public bool SwitchWeaponButton { get { return switchWeaponButton; } }
    public bool SwitchDefenseButton { get { return switchDefenseButton; } }
    public bool SwitchJumpButton { get { return switchJumpButton; } }
    public bool SwitchSprintButton { get { return switchSprintButton; } }

    //
    public bool ChangeAttackDown { get { return changeAttackDown; } }
    public bool ChangeDefenseDown { get { return changeDefenseDown; } }
    public bool ChangeSprintDown { get { return changeSprintDown; } }
    public bool ChangeJumpDown { get { return changeJumpDown; } }

    //
    public bool PauseButton { get { return pauseButton; } }

    // Probalbemente aqui
    public float RightAxisSensivity
    {
        get { return rightAxisSensivity; }
        set
        {
            rightAxisSensivity = value;
            rightAxisSensivity = Mathf.Clamp(rightAxisSensivity, 0.1f, 1);
        }
    }

    #endregion

    // Use this for initialization
    void Start()
    {

        //previousMousePosition = Input.mousePosition;

    }

    // Update is called once per frame
    void Update()
    {

        // TODO: Separar flechas del teclado de los ejes normales (wasd)
        // TODO: Ya estaba normalizado. Revisar si la brusquedad viene cuando se aplica a la rotación del player
        stickAxis.x = Input.GetAxisRaw("Horizontal");
        stickAxis.y = Input.GetAxisRaw("Vertical");
        stickAxis.Normalize();

        //
        UpdateMouseMovement();

        //
        UpdateSelectionCross();

        sprintButton = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Joystick1Button1) /* Gamepad B*/;

        jumpButton = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Joystick1Button0) /* Gamepad A*/;

        //fireButton = Input.GetAxisRaw("Fire1"); /* Gamepad B, aún no*/
        fireButton = Input.GetAxisRaw("Cross Triggers") > 0 || Input.GetKey(KeyCode.Mouse0);

        fireButtonDown = Input.GetAxisRaw("Cross Triggers") > 0 || Input.GetKeyDown(KeyCode.Mouse0);

        // TODO: Chequear en mando
        markObjectiveButton = Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Joystick1Button9) /* R3 */;

        // Switch buttons
        switchWeaponButton = Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Joystick1Button5);
        switchDefenseButton = Input.GetKeyDown(KeyCode.Alpha2) || changeDefenseDown;
        switchJumpButton = Input.GetKeyDown(KeyCode.Alpha3) || changeJumpDown;
        switchSprintButton = Input.GetKeyDown(KeyCode.Alpha4) || changeSprintDown;


        defenseButton = Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Joystick1Button3) /* Gamepad Y*/;

        menuButton = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7) /* Gamepad Start*/;

        // Stick derecho
        rightStickAxis = new Vector2(Input.GetAxisRaw("Right Horizontal"), Input.GetAxisRaw("Right Vertical")) * rightAxisSensivity;
        rightStickAxis.Normalize();

        //
        pauseButton = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7);


    }

    void UpdateMouseMovement()
    {
        //
        //Vector2 newmousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        //mouseMovement = newmousePosition - previousMousePosition;
        //previousMousePosition = newmousePosition;

        mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    void UpdateSelectionCross()
    {
        //
        crossAxis = new Vector2(Input.GetAxisRaw("Cross Horizontal"), Input.GetAxisRaw("Cross Vertical"));
        //
        changeAttackDown = crossAxis.x > 0.2f && previousCrossAxis.x <= 0.2f;
        changeDefenseDown = crossAxis.y > 0.2f && previousCrossAxis.y <= 0.2f;
        changeSprintDown = crossAxis.x < -0.2f && previousCrossAxis.x >= -0.2f;
        changeJumpDown = crossAxis.y < -0.2f && previousCrossAxis.y >= -0.2f;
        //
        previousCrossAxis = crossAxis;
    }

}
