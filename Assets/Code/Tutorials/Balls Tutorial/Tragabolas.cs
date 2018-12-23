using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tragabolas : MonoBehaviour
{
    private BallsScene tutorial;

    private void Start()
    {
        tutorial = FindObjectOfType<BallsScene>();
    }

    private void OnTriggerEnter(Collider other)
    {
        MurderBall trainingBall = other.GetComponent<MurderBall>();
        if(trainingBall != null)
        {
            tutorial.CleanBall();
            Destroy(trainingBall.gameObject);
        }
    }
}
