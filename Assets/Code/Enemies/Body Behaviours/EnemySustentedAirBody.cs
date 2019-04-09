using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySustentedAirBody : MekanoidBodyBehaviour
{
    // De momento global
    // Problablemente la hagamos respecto al player
    public float idealHeight = 100;

    // Start is called before the first frame update
    protected override void Start()
    {
        // Ñapa
        transform.position = new Vector3(transform.position.x, idealHeight, transform.position.z);
        //
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        //
        base.Update();
        // Que se mantengan a cierta altura por encima del prota
        float offsetFromIdealHeight;
        if (HasRemainingTurrets())
            offsetFromIdealHeight = idealHeight + player.transform.position.y - transform.position.y;
        else
            offsetFromIdealHeight = player.transform.position.y - transform.position.y;
        if (offsetFromIdealHeight > 0)
            rb.AddForce(Vector3.up * motorForce, ForceMode.Impulse);
        // Chequear si está muerto
        //if()
    }

    protected override void GiveItGas()
    {
        rb.AddForce(transform.forward * motorForce, ForceMode.Impulse);
    }
}
