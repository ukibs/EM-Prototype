using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base para las ayudas de Carol que se quieran generar
/// </summary>
public class CarolBaseHelp : MonoBehaviour
{
    // Cada codigo tendrá su set personalizado de steps
    // Intentaré generalizar los triggers para avanzar a través de ellos

    public AudioClip[] audioClips;
    public float initialWait = 20;
    public GUISkin gUISkin;

    protected AudioSource audioSource;
    protected GameManager gameManager;
    protected string[] helpMessages = new string[] { " Press Change weapon (right cross or 1)",
                                                    " Press Escape to end the pain"};

    // Start is called before the first frame update
    protected virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // TODO: Poner cuadro de dialogo detrás del texto
}
