using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAndStatsManager : MonoBehaviour
{
    //public RectTransform mapTransform;
    //public RectTransform playerBackgroundTransform;
    public float transitionTime = 0.5f;
    // Provisional
    public GameObject canonButton;

    private Transform cameraTransform;
    private Vector3[] cameraPositions;
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
        // Ajustamos estas dos cosas al tamaño de pantalla
        //mapTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        //playerBackgroundTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        //playerBackgroundTransform.position = new Vector3(Screen.width * 1.5f, 0, 0);
        //
        cameraTransform = Camera.main.transform;
        //
        cameraPositions = new Vector3[2];
        //cameraPositions[0] = new Vector3(0, 0, 0);
        //cameraPositions[1] = new Vector3(Screen.width * 1.5f, 0, 0);
        cameraPositions[0] = cameraTransform.position;
        cameraPositions[1] = new Vector3(cameraPositions[0].x + 16, cameraPositions[0].y, cameraPositions[0].z);
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
            //SwitchSection();
        }
        //
        timeFromTransitionOrder += dt;
        //
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, 
            cameraPositions[positionsIndex], timeFromTransitionOrder / transitionTime);
    }

    private void OnGUI()
    {
        //
        if (levelHighlighted)
        {

        }
    }

    #region Methods

    //
    public void SwitchSection()
    {
        positionsIndex++;
        positionsIndex = positionsIndex % cameraPositions.Length;
        //Debug.Log("Positions index: " + positionsIndex);
        timeFromTransitionOrder = 0;
    }

    //
    public void ActivateNextWeapon()
    {
        GameManager.instance.playerAttributes.unlockedAttackActions++;
        canonButton.SetActive(true);
    }

    //
    void CheckUnlockedStuff()
    {

    }

    #endregion
}
