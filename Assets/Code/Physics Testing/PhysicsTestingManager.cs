using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsTestingManager : MonoBehaviour
{
    private List<RunningTest> runningTests;

    // Start is called before the first frame update
    void Start()
    {
        runningTests = new List<RunningTest>(10);
    }

    // Update is called once per frame
    void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        for(int i = 0; i < runningTests.Count; i++)
        {
            runningTests[i].UpdateTesting(dt);
        }
    }

    #region Methods

    public void StartNewTest(GameObject objectToTest)
    {
        RunningTest newRunningTest = new RunningTest(objectToTest);
    }

    #endregion
}

public class RunningTest
{
    GameObject testingObject;
    Rigidbody objectRigidbody;
    private float testDuration;
    private float timeFormStart;
    public bool finished = false;

    public RunningTest(GameObject objectToTest)
    {
        this.testingObject = objectToTest;
        objectRigidbody = testingObject.GetComponent<Rigidbody>();
    }

    public void UpdateTesting(float dt)
    {
        // Chequearemos que el objeto no se haya destruido

        // 
        timeFormStart += dt;
        if (timeFormStart >= testDuration)
        {
            // Testeo final

            //
            finished = true;
        }
    }
}
