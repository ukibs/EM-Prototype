﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrushingEsophagus : MonoBehaviour
{

    private enum WallStatus
    {
        Invalid = -1,

        Closing,
        Closed,
        Opening,

        Count
    }

    #region Public Attributes

    public float timeClosingWalls = 0.5f;
    public float timeWallsAreClosed = 1;
    public float timeOpeningWalls = 2;

    public AudioClip wallsCompleteCloseSound;

    #endregion

    #region Private Attributes

    private Transform[] crushingwalls;
    private PlayerIntegrity playerIntegrity;

    private float initialWallY;
    private float closedWallY;

    private WallStatus wallStatus = WallStatus.Closing;

    private int wallsCollidingWithPlayer = 0;

    private float currentCounterTime;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // Guardamos aqui los hijos para manejarlos con comodidad
        crushingwalls = new Transform[transform.childCount];

        for(int i = 0; i < crushingwalls.Length; i++)
        {
            crushingwalls[i] = transform.GetChild(i);
        }

        //
        initialWallY = crushingwalls[0].localPosition.y;
        closedWallY = initialWallY / 2;

        //
        playerIntegrity = FindObjectOfType<PlayerIntegrity>();
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        UpdateWalls(dt);
    }

    void UpdateWalls(float dt)
    {
        //.
        currentCounterTime += dt;
        //
        float progression = 0;
        //
        switch (wallStatus)
        {
            case WallStatus.Closing:
                //
                progression = dt / timeClosingWalls;
                //
                DisplaceWalls(progression, 1);
                //
                if (currentCounterTime >= timeClosingWalls)
                {
                    wallStatus = WallStatus.Closed;
                    currentCounterTime = 0;
                }
                    
                break;
            case WallStatus.Closed:
                //
                if (currentCounterTime >= timeWallsAreClosed)
                {
                    wallStatus = WallStatus.Opening;
                    currentCounterTime = 0;
                }
                    
                break;
            case WallStatus.Opening:
                //
                progression = dt / timeOpeningWalls;
                //
                DisplaceWalls(progression, -1);
                //
                if (currentCounterTime >= timeOpeningWalls)
                {
                    wallStatus = WallStatus.Closing;
                    currentCounterTime = 0;
                }
                    
                break;
        }
    }

    void DisplaceWalls(float timeProgression, float direction)
    {
        //
        for(int i = 0; i < crushingwalls.Length; i++)
        {
            //
            float positionProgression = Mathf.Abs(initialWallY - closedWallY) * timeProgression;
            //
            crushingwalls[i].Translate(Vector3.up * positionProgression * direction, Space.Self);
        }
    }
}
