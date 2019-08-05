using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeadBody : MonoBehaviour
{
    //
    public float rigidbodiesLifeTime = 15;
    //
    private Rigidbody[] rbs;
    private ConfigurableJoint[] cjs;
    // Start is called before the first frame update
    void Start()
    {
        //
        cjs = gameObject.GetComponentsInChildren<ConfigurableJoint>();
        rbs = gameObject.GetComponentsInChildren<Rigidbody>();
        //
        for (int i = 0; i < cjs.Length; i++)
        {
            Destroy(cjs[i], rigidbodiesLifeTime);
        }
        //
        for (int i = 0; i < rbs.Length; i++)
        {
            Destroy(rbs[i], rigidbodiesLifeTime + 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
