using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ayuda de carol para el nivel de prueba con enemigos
/// </summary>
public class CarolHelpFirstTest : CarolBaseHelp
{
    private enum Step
    {
        Invalid = -1,

        InitialWait,
        CarolPropests,
        CarolProposeSomething,
        CarolOrder,
        CarolSatisfied,
        CarolCloses,

        Count
    }

    
    private float timeFromLastCheck = 0;
    private Step currentStep;
    private InputManager inputManager;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        inputManager = FindObjectOfType<InputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7))
        {
            SceneManager.LoadScene("Menu");
        }
        //
        if(currentStep == Step.InitialWait)
        {
            timeFromLastCheck += dt;
            //if(timeFromLastCheck >= initialWait)
            //{
            //    //
            //    audioSource.clip = audioClips[(int)currentStep];
            //    audioSource.Play();
            //    //
            //    currentStep++;
            //    timeFromLastCheck = 0;
            //}
        }
        //
        if (!audioSource.isPlaying && 
            (currentStep == Step.CarolPropests || currentStep == Step.CarolProposeSomething || currentStep == Step.CarolSatisfied) )
        {
            //
            if (currentStep == Step.CarolProposeSomething) gameManager.unlockedAttackActions = 2;
            //
            //audioSource.clip = audioClips[(int)currentStep];
            audioSource.Play();
            //
            currentStep++;
        }
        //
        if(currentStep == Step.CarolOrder && inputManager.SwitchWeaponButton)
        {
            //
            //audioSource.clip = audioClips[(int)currentStep];
            audioSource.Play();
            //
            currentStep++;
        }
    }

    private void OnGUI()
    {
        //
        Rect textRect = new Rect(Screen.width / 2 - 250, Screen.height - 100, 500, 20);
        //
        if(currentStep == Step.CarolOrder)
        {
            //GUI.Label(textRect, helpMessages[0], gUISkin.label);
        }
        //
        if (currentStep == Step.CarolCloses)
        {
            //GUI.Label(textRect, helpMessages[1], gUISkin.label);
        }
        //
        GUI.Label(new Rect(Screen.width / 2 - 250, 50, 500, 20), "Press TAB to lock on enemies");
    }
}
