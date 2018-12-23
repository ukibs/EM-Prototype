using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cinematic01 : BaseCinematic
{
    protected enum Steps
    {
        Invalid = -1,

        BadGuyStarts,
        EmTurns,
        EmResponds,
        BadGuyAgain,
        CarolComments,
        BadguyReplicates,
        CarolClose,

        Count
    }

    public Transform player;
    public MeshRenderer playerFace;
    public Material playerHappyFace;
    public float timeToTurnPlayer = 0.2f;
    [Tooltip("A quién apunta en cada caso")]
    public Transform[] cameraObjectives;
    public AudioClip[] voiceClips;
    public GUISkin guiSkin;

    private Steps currentStep;
    private string[] cinematicLines = new string[] { "Ey! You! Floating ball!",
                                                    ""/*"Bup?"*/,
                                                    "What da ‘ell ‘re you doin’ ‘ere!",
                                                    ""/*"Bup, bup!"*/,
                                                    "Carol: EM, those rude guys are the ones you have to fight.",
                                                    "Well, if you came ‘ere, it’s ‘cause you want a fight!",
                                                    "Carol: See? They are asking for it."};
    private InputManager inputManager;
    private Transform cinematicCamera;
    private float timeTurningPlayer = 0;
    private Fade fade;
    private AudioSource audioSource;
    

    // Start is called before the first frame update
    void Start()
    {
        inputManager = FindObjectOfType<InputManager>();
        cinematicCamera = Camera.main.transform;
        fade = FindObjectOfType<Fade>();
        audioSource = GetComponent<AudioSource>();
        //
        audioSource.clip = voiceClips[0];
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        if (inputManager.FireButtonDown && (int)currentStep < (int)Steps.Count - 1)
        {
            currentStep++;
            //
            audioSource.clip = voiceClips[(int)currentStep];
            audioSource.Play();
        }
        //
        cinematicCamera.LookAt(cameraObjectives[(int)currentStep]);
        //
        if(currentStep == Steps.EmTurns)
        {
            timeTurningPlayer += dt;
            Quaternion objectiveRotation = Quaternion.LookRotation(-Vector3.forward);
            player.rotation = Quaternion.Slerp(player.rotation, objectiveRotation, timeTurningPlayer / timeToTurnPlayer);
        }
        //
        if(currentStep == Steps.EmResponds)
        {
            //MeshRenderer emFaceMaterial = player.transform.Find("Face").GetComponent<MeshRenderer>();
            //if (emFaceMaterial != playerHappyFace)
            playerFace.material = playerHappyFace;
            playerFace.materials[0] = playerHappyFace;
        }
        //
        if(currentStep == Steps.CarolClose)
        {
            cinematicCamera.transform.Translate(Vector3.up * 5 * dt);
            if(fade.direction == -1)
            {
                fade.direction = 1;
                fade.speed = 0.5f;
            }
            //
            if(fade.alpha >= 1 && !audioSource.isPlaying)
            {
                SceneManager.LoadScene("Level 1 Test");
            }
        }
    }

    private void OnGUI()
    {
        //
        Rect textRect = new Rect(Screen.width / 2 - 250, Screen.height - 100, 500, 20);
        //
        if (cinematicLines[(int)currentStep] != null)
        {
            GUI.Label(textRect, cinematicLines[(int)currentStep], guiSkin.label);
        }
    }
}
