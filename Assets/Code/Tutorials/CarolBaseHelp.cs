using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
public enum HelpTrigger
{
    Invalid = -1,

    Automatic,
    CountDown,
    SearchingWeakPoints,
    AwaitingTrigger,
    WeakPointDestruction,

    Count
}

/// <summary>
/// Base para las ayudas de Carol que se quieran generar
/// </summary>
public class CarolBaseHelp : MonoBehaviour
{
    
    // Cada codigo tendrá su set personalizado de steps
    // Intentaré generalizar los triggers para avanzar a través de ellos

    public CarolStepObject[] carolStepObjects;
    public GUISkin gUISkin;

    protected AudioSource audioSource;
    protected GameManager gameManager;

    protected RobotControl player;

    protected int currentStep = 0;
    // Con esta variable controlaremos el progreso del step
    // Ya sea tiempo o puntos destruidos
    protected float stepProgress = 0;

    protected CarolStep CurrentStep { get { return carolStepObjects[currentStep].carolStep; } }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gameManager = FindObjectOfType<GameManager>();
        player = FindObjectOfType<RobotControl>();
        // Le damos al primer clip
        PlayClip(CurrentStep.audioClip);
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        
        //
        CheckStepProgress(dt);

        // Lo ponemos aqui para que funcione en otras fases, pero habrá que revisarlo
        SearchForWeakPoint();

    }

    private void OnGUI()
    {
        if(CurrentStep.stepText != "")
        {
            //
            Rect textRect = new Rect(Screen.width / 2 - 250, Screen.height - 100, 500, 20);
            //
            GUI.Label(textRect, CurrentStep.stepText, gUISkin.label);
        }
    }

    //
    void CheckStepProgress(float dt)
    {
        //
        switch (CurrentStep.helpTrigger)
        {
            case HelpTrigger.SearchingWeakPoints:
                if (SearchForWeakPoint())
                {
                    //currentStep++;
                    NextStep();
                }
                break;
            case HelpTrigger.CountDown:
                stepProgress += dt;
                if (stepProgress >= CurrentStep.amount)
                    NextStep();
                break;
        }
    }

    // Avanzamos paso
    public void NextStep()
    {
        //
        currentStep++;
        stepProgress = 0;
        PlayClip(CurrentStep.audioClip);
    }

    //
    public void WeakPointDestroyed()
    {
        //
        stepProgress++;
        // Si carol estaba esperando la destruccion de uno...
        //if (carolSteps[currentStep].helpTrigger == HelpTrigger.AwaitingTrigger)
        if(CurrentStep.amount >= stepProgress)
        {
            NextStep();
        }
        // Probablemente trabajemos más casos
    }

    // TODO: Poner cuadro de dialogo detrás del texto

    void PlayClip(AudioClip clip)
    {
        // Si tiene clip de audio le damos
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Función que te busca puntos debiles
    /// Devuelve booleano para cuando se necesite como trigger
    /// TODO: Habrá que hacer que se lo llame en algunas otras situaciones
    /// </summary>
    /// <returns></returns>
    bool SearchForWeakPoint()
    {
        //
        bool anyFound = false;
        //
        float detectorRange = 100;
        //
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, detectorRange);
        for(int i = 0; i < hitColliders.Length; i++)
        {
            WeakPoint weakPoint = hitColliders[i].GetComponent<WeakPoint>();
            if(weakPoint != null)
            {
                // Lo ponemos como activo para que pueda ser targeteado
                // TODO: Poner nombre de variable más claro, coño
                weakPoint.active = true;
                anyFound = true;
            }
        }
        //
        return anyFound;
    }
}

