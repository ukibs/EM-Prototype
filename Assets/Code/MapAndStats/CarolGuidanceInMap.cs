using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarolGuidanceInMap : MonoBehaviour
{
    private enum Steps
    {
        Invalid = -1,

        CarolCongratulates,
        CarolSendsPlayerToStatsWindow,
        CarolActivatesNewWeapon,
        CarolActivatesNextMission,

        Count
    }

    public AudioClip[] audioClips;

    private MapAndStatsManager mapAndStatsManager;
    private AudioSource audioSource;
    private Steps guidanceSteps;

    // Start is called before the first frame update
    void Start()
    {
        //
        // if(GameManager.instance.gameStep)
        //
        mapAndStatsManager = FindObjectOfType<MapAndStatsManager>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClips[0];
        audioSource.Play();
        mapAndStatsManager.PlayerLocked = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Depending on the step...
        switch (guidanceSteps)
        {
            case Steps.CarolCongratulates:
                if (!audioSource.isPlaying)
                {
                    //
                    mapAndStatsManager.SwitchSection();
                    //
                    guidanceSteps++;
                    audioSource.clip = audioClips[(int)guidanceSteps];
                    audioSource.Play();
                }
                break;
            case Steps.CarolSendsPlayerToStatsWindow:
                if (!audioSource.isPlaying)
                {
                    //
                    GameManager.instance.unlockedAttackActions++;
                    mapAndStatsManager.ActivateNextWeapon();                    
                    mapAndStatsManager.PlayerLocked = false;
                    //
                    guidanceSteps++;
                    audioSource.clip = audioClips[(int)guidanceSteps];
                    audioSource.Play();
                }
                break;
            // Este se cerrará cuando el jugador vuelva al mapa por su cuenta
            case Steps.CarolActivatesNewWeapon:
                if (mapAndStatsManager.InMap)
                {
                    //
                    //mapAndStatsManager.Ac
                    //
                    guidanceSteps++;
                    audioSource.clip = audioClips[(int)guidanceSteps];
                    audioSource.Play();
                }
                break;

        }
    }
}
