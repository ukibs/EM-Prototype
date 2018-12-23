using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BallsScene : MonoBehaviour
{
    private enum Step
    {
        Invalid = -1,

        Presentation,
        JumpTesting,
        SprintTesting,
        FirstBall,
        LockingCamera,
        Trouble,
        HangOnThere,
        Solution,
        End,

        Count
    }


    public AudioClip welcomeMessage;
    public AudioClip[] stepMessages;
    public GameObject trainingBallPrefab;
    public GameObject testingSpherePrefab;
    public GUISkin guiSkin;
    public float presentationMessageDuration = 2;
    public float timeTillLockingAdvice = 15;
    public float troubleStepDuration = 5;
    public float hangOnStepDuration = 20;
    public int hitsWithBallsInWalls = 10;
    public float timeBetweenBalls = 0.1f;
    public int ballsToGenerateInTrouble = 250;
    public GameObject ascendingCurrent;
    public GameObject playerFace;
    public Material happyFace;

    private AudioSource audioSource;
    private GameManager gameManager;
    private Step step = Step.Presentation;
    private string[] presentationMessages = new string[] { "Good day, EM-00!",
                                                           "Time to check your systems!" };
    private int presentationMessageIndex = 0;
    private float timeFromLastCheck = 0;
    private int checkPointsDone = 0;
    private string[] tutorialText = new string[] { "Jump to the points (space) ",
                                                    "Use sprint to reach quickly the points (shift)",
                                                    "Use your pulse attack to send some of the balls to stream (left mouse)",
                                                    "Lock enemies with the camera (tab or R1)",
                                                    "Ooops, we have a problem here",
                                                    "Use this to defend yourself (lCtrl)",
                                                    "Clean the remaining balls",
                                                    "Yes! Now you are ready for action"};
    private int currentHitsWithBallsInWalls = 10;
    private int generatedBallsInTrouble = 0;

    private int ballsToClean;
    private int cleanedBalls;

    private Fade fade;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gameManager = FindObjectOfType<GameManager>();
        fade = FindObjectOfType<Fade>();
        //
        audioSource.clip = welcomeMessage;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        //
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7))
        {
            SceneManager.LoadScene("Menu");
        }
        //
        float dt = Time.deltaTime;
        //
        switch (step)
        {
            case Step.Presentation:

                //
                if (!audioSource.isPlaying)
                {
                    //
                    SetTutorialSphere(new Vector3(30, 15, -30));
                    SetTutorialSphere(new Vector3(-30, 15, -30));
                    SetTutorialSphere(new Vector3(0, 30, 50));
                    //
                    gameManager.unlockedJumpActions = 1;
                    //
                    audioSource.clip = stepMessages[(int)step];
                    audioSource.Play();
                    //
                    step++;
                }
                break;
            case Step.FirstBall:
                //
                timeFromLastCheck += dt;
                if(timeFromLastCheck > timeTillLockingAdvice)
                {
                    //
                    audioSource.clip = stepMessages[(int)step];
                    audioSource.Play();
                    //
                    step++;
                }
                break;
            case Step.Trouble:
                //
                // TODO: Hacerlo con pool, por dios
                // Pequeña ñapa para aligrar un poco
                timeFromLastCheck += dt;
                if (timeFromLastCheck >= timeBetweenBalls)
                {
                    timeFromLastCheck -= timeBetweenBalls;
                    SetTrainingBall(new Vector3(Random.Range(-75, 75), 150, Random.Range(-75, 75)));
                    generatedBallsInTrouble++;
                }
                //
                if(generatedBallsInTrouble >= ballsToGenerateInTrouble)
                {
                    gameManager.unlockedDefenseActions = 1;
                    //
                    audioSource.clip = stepMessages[(int)step];
                    audioSource.Play();
                    //
                    step++;
                }

                break;
            case Step.HangOnThere:
                //
                timeFromLastCheck += dt;
                if(timeFromLastCheck >= hangOnStepDuration)
                {
                    timeFromLastCheck = 0;
                    cleanedBalls = 0;
                    ballsToClean = generatedBallsInTrouble;
                    gameManager.unlockedAttackActions = 1;
                    ascendingCurrent.SetActive(true);
                    //
                    audioSource.clip = stepMessages[(int)step];
                    audioSource.Play();
                    //
                    step++;
                }
                break;
            case Step.End:
                // Start the fade
                if(fade.direction == -1)
                {
                    fade.direction = 1;
                    fade.speed = 0.2f;
                }
                // Get the fuck out of here
                if(fade.alpha == 1 && !audioSource.isPlaying)
                {
                    // Ya veremos si lo madnamos aqui o a un menú
                    SceneManager.LoadScene("Menu");
                }
                break;
        }
    }

    private void OnGUI()
    {
        //
        Rect textRect = new Rect(Screen.width / 2 - 250, Screen.height - 100, 500, 20);
        //
        if(step == Step.Presentation)
        {
            GUI.Label(textRect, presentationMessages[presentationMessageIndex], guiSkin.label);
        }
        //
        if(step > 0 && tutorialText[(int)step - 1] != null)
        {
            GUI.Label(textRect, tutorialText[(int)step - 1], guiSkin.label);
        }
        // Checking stuff
        GUI.Label(new Rect(10, 10, 200, 20), "Cleaned balls " + cleanedBalls + " / " + ballsToClean, guiSkin.label);

    }

    #region Methods

    void SetTutorialSphere(Vector3 position)
    {
        Instantiate(testingSpherePrefab, position, Quaternion.identity);
    }

    void SetTrainingBall(Vector3 position)
    {
        Instantiate(trainingBallPrefab, position, Quaternion.identity);
    }

    public void TriggerTutorialSphere(GameObject triggererObject)
    {
        switch (step)
        {
            case Step.JumpTesting:
                checkPointsDone++;
                TutorialPoint tutorialPoint = FindObjectOfType<TutorialPoint>();
                // Chequeamos que estén las dos
                if (checkPointsDone == 3)
                // Chequeamos que no queden
                //if(tutorialPoint == null)
                {
                    // Sacamos puntos de sprint
                    SetTutorialSphere(new Vector3(75, 15, 75));
                    SetTutorialSphere(new Vector3(75, 15, -75));
                    SetTutorialSphere(new Vector3(-75, 15, 75));
                    SetTutorialSphere(new Vector3(-75, 15, -75));
                    //
                    gameManager.unlockedSprintActions = 1;
                    //
                    audioSource.clip = stepMessages[(int)step];
                    audioSource.Play();
                    //
                    step++;
                    checkPointsDone = 0;
                }
                break;
            case Step.SprintTesting:
                checkPointsDone++;
                // Chequeamos que estén las dos
                if (checkPointsDone == 4)
                {
                    // Sacamos punto de bola
                    SetTutorialSphere(new Vector3(0, 5, 100));
                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            SetTrainingBall(new Vector3(i * 20, 150, j * 20));
                        }
                    }
                    ballsToClean = 25;
                    ascendingCurrent.SetActive(true);
                    //
                    gameManager.unlockedAttackActions = 1;
                    //
                    audioSource.clip = stepMessages[(int)step];
                    audioSource.Play();
                    //
                    step++;
                }
                
                break;
            
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void CleanBall()
    {
        cleanedBalls++;
        //Debug.Log("One less ball");
        if(cleanedBalls >= ballsToClean)
        {
            if(step == Step.LockingCamera)
            {
                gameManager.unlockedAttackActions = 0;
                ascendingCurrent.SetActive(false);
            }
            //
            timeFromLastCheck = 0;
            //
            audioSource.clip = stepMessages[(int)step];
            audioSource.Play();
            //
            step++;
        }
    }

    #endregion

}
