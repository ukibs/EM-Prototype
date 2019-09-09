using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    //
    private List<GameObject> dangerousBullets;
    // A ver cómo responde esto
    private List<BulletTypePool> bulletPoolsPerType;

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

    #region Bullet Management Methods

    public void RegisterBullets(GameObject bulletPrefab, float fireRate, float bulletLifeTime)
    {
        //
        if(bulletPoolsPerType.Count == 0)
        {

        }
        //
        for(int i = 0; i < bulletPoolsPerType.Count; i++)
        {

        }

    }

    #endregion

    #region Dangerous Bullet Methods

    public void AddDangerousBulletToList(GameObject incomingBullet)
    {
        dangerousBullets.Add(incomingBullet);
    }

    public void RemoveDangerousBulletFromList(GameObject incomingBullet)
    {
        dangerousBullets.Remove(incomingBullet);
    }

    #endregion
}

public class BulletTypePool
{
    public string prefabName;
    private List<GameObject> reserveBullets;
    private List<GameObject> activeBullets;
}