using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicTestTrigger : MonoBehaviour
{
    private PhysicsTestingManager physicsTestingManager;

    // Start is called before the first frame update
    void Start()
    {
        physicsTestingManager = FindObjectOfType<PhysicsTestingManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        physicsTestingManager.StartNewTest(other.gameObject);
    }
}
