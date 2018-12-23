using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MurderBall : MonoBehaviour {

    public float forceToApply = 30;

    private Rigidbody rb;
    private RobotControl player;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        player = FindObjectOfType<RobotControl>();
	}
	
	// Update is called once per frame
	void Update () {
        //Vector3 playerDirection = (player.transform.position - transform.position).normalized;
        //rb.AddForce(playerDirection * 3000, ForceMode.Force);
        ControlCheck();
	}

    private void OnCollisionStay(Collision collision)
    {
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;
        rb.AddForce(playerDirection * forceToApply, ForceMode.Force);
    }

    // Chequeo para que no se salgan de la zona de tuto
    void ControlCheck()
    {
        // Luego lo revisamos
        if(transform.position.y < -10 || transform.position.y > 1000)
        {
            transform.position = new Vector3(0, 50, 0);
        }
    }
}
