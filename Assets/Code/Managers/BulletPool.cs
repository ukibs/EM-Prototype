using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    //
    private List<GameObject> dangerousBullets;

    //
    public List<GameObject> DangerousBullets { get { return dangerousBullets; } }

    // Start is called before the first frame update
    void Start()
    {
        dangerousBullets = new List<GameObject>(10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddDangerousBulletToList(GameObject incomingBullet)
    {
        dangerousBullets.Add(incomingBullet);
    }

    public void RemoveDangerousBulletFromList(GameObject incomingBullet)
    {
        dangerousBullets.Remove(incomingBullet);
    }
}
