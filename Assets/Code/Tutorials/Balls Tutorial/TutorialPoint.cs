using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPoint : MonoBehaviour
{
    private BallsScene ballsSceneManager;

    private void Start()
    {
        ballsSceneManager = FindObjectOfType<BallsScene>();
    }

    private void OnTriggerEnter(Collider other)
    {
        ballsSceneManager.TriggerTutorialSphere(other.gameObject);
        Destroy(gameObject);
    }
}
