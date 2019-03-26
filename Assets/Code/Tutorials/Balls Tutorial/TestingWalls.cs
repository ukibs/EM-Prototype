using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingWalls : MonoBehaviour
{
    //BallsScene tutorialManager;

    // Start is called before the first frame update
    void Start()
    {
        //tutorialManager = FindObjectOfType<BallsScene>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //MurderBall murderBall = collision.collider.GetComponent<MurderBall>();
        //if(murderBall != null && collision.relativeVelocity.magnitude > 100.0f)
        //    tutorialManager.HitWallsWithBalls();
    }
}
