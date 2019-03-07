using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBullet : Bullet
{
    // Genereic ones
    public float explosionRange;
    public float explosionForce;
    public float explosionDamage;

    // Realistic ones
    public float explosiveLoad;
    
    // TODO: Decidir si usamos fragmentos

    protected override void OnCollisionEnter(Collision collision)
    {

        base.OnCollisionEnter(collision);
    }

    private void GenerateExplosion()
    {

    }
}
