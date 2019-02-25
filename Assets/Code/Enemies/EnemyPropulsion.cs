using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Code for enemy flyers
/// For now we will make a copter like propulsor
/// </summary>
public class EnemyPropulsion : MonoBehaviour
{
    // De momento global
    // Problablemente la hagamos respecto al player
    public float idealHeight = 100;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Ñapa
        transform.position = new Vector3(transform.position.x, idealHeight, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        float offsetFromIdealHeight = idealHeight - transform.position.y;
        rb.AddForce(Vector3.up * offsetFromIdealHeight, ForceMode.Impulse);
    }
}
