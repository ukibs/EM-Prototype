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
        // Si todavía no hay ninguna creada vamos directamente con esto
        if(bulletPoolsPerType.Count == 0)
        {
            int maxExistantBullets = (int)(bulletLifeTime/fireRate);
            string bulletName = bulletPrefab.name;
            BulletTypePool newBulletTypePool = new BulletTypePool(bulletName);
            bulletPoolsPerType.Add(newBulletTypePool);
            //
            for(int i = 0; i < maxExistantBullets; i++)
            {

            }
            return;
        }
        //
        bool found = false;
        for(int i = 0; i < bulletPoolsPerType.Count; i++)
        {
            string enteringBulletName = bulletPrefab.name;
            if (bulletPoolsPerType[i].prefabName.Equals(enteringBulletName))
            {
                found = true;

            }
        }
        //
        if (!found)
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

    public BulletTypePool(string prefabName)
    {
        this.prefabName = prefabName;
        reserveBullets = new List<GameObject>();
        activeBullets = new List<GameObject>();
    }
}