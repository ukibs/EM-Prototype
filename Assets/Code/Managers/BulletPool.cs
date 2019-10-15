using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    //
    public static BulletPool instance = null;

    //
    private List<GameObject> dangerousBullets;
    // A ver cómo responde esto
    private List<BulletTypePool> bulletPoolsPerType;

    //
    public List<GameObject> DangerousBullets { get { return dangerousBullets; } }

    //
    private CarolBaseHelp carolHelp;

    //
    void Awake()
    {
        //Check if there is already an instance of SoundManager
        if (instance == null)
            //if not, set it to this.
            instance = this;
        //If instance already exists:
        else if (instance != this)
            //Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        dangerousBullets = new List<GameObject>(10);
        bulletPoolsPerType = new List<BulletTypePool>();
        carolHelp = FindObjectOfType<CarolBaseHelp>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Bullet Management Methods

    //
    public void RegisterBullets(GameObject bulletPrefab, float fireRate, float bulletLifeTime)
    {
        //
        int maxExistantBullets = (int)(bulletLifeTime * fireRate);
        string enteringBulletName = bulletPrefab.name;
        Bullet bulletScript = bulletPrefab.GetComponent<Bullet>();
        // Si todavía no hay ninguna creada vamos directamente con esto
        /*if (bulletPoolsPerType.Count == 0)
        {
            BulletTypePool newBulletTypePool = new BulletTypePool(enteringBulletName);
            bulletPoolsPerType.Add(newBulletTypePool);
            //
            newBulletTypePool.bulletScript = bulletScript;
            newBulletTypePool.dangerousEnough = bulletScript.dangerousEnough;
            // Y ahora metemos las balas
            IntroduceBullets(newBulletTypePool, bulletPrefab, maxExistantBullets);
            return;
        }*/
        // Cuando ya hay por lo menos una creada
        for(int i = 0; i < bulletPoolsPerType.Count; i++)
        {
            //string enteringBulletName = bulletPrefab.name;
            if (bulletPoolsPerType[i].prefabName.Equals(enteringBulletName))
            {
                //
                IntroduceBullets(bulletPoolsPerType[i], bulletPrefab, maxExistantBullets);
                return;
            }
        }
        
        // Y en caso de que entre una no registrada
        BulletTypePool newBulletTypePool = new BulletTypePool(enteringBulletName);
        bulletPoolsPerType.Add(newBulletTypePool);
        //
        newBulletTypePool.bulletScript = bulletScript;
        newBulletTypePool.dangerousEnough = bulletScript.dangerousEnough;
        // Y ahora metemos las balas
        IntroduceBullets(newBulletTypePool, bulletPrefab, maxExistantBullets);
        return;
    }

    //
    void IntroduceBullets(BulletTypePool bulletTypePool, GameObject bulletPrefab, int bulletsToIntroduce)
    {
        for (int i = 0; i < bulletsToIntroduce; i++)
        {
            GameObject newBullet = Instantiate(bulletPrefab, Vector3.zero, Quaternion.identity);
            newBullet.SetActive(false);
            bulletTypePool.reserveBullets.Add(newBullet);
        }
    }

    //
    public GameObject GetBullet(GameObject bulletPrefab)
    {
        //
        GameObject nextBullet = null;
        string bulletName = bulletPrefab.name;
        //
        for(int i = 0; i < bulletPoolsPerType.Count; i++)
        {
            if(bulletPoolsPerType[i].prefabName == bulletName)
            {
                //
                if (bulletPoolsPerType[i].reserveBullets.Count == 0)
                {
                    Debug.Log("No more bullets available");
                    return null;
                }
                // TODO: Resetear tiempo de vida
                nextBullet = bulletPoolsPerType[i].reserveBullets[0];
                nextBullet.SetActive(true);
                Bullet nextBulletScript = nextBullet.GetComponent<Bullet>();
                nextBulletScript.CurrentLifeTime = 0;
                //
                bulletPoolsPerType[i].reserveBullets.Remove(nextBullet);
                bulletPoolsPerType[i].activeBullets.Add(nextBullet);
                //
                if (bulletPoolsPerType[i].dangerousEnough)
                {
                    // Instanciamos el trail renderer
                    if (nextBulletScript.drawTrayectory)
                    {
                        //detectionTrail = Instantiate(carolHelp.dangerousProyetilesTrailPrefab, transform.position, Quaternion.identity);
                        //detectionTrailRenderer = detectionTrail.GetComponent<LineRenderer>();
                        //
                        nextBulletScript.AllocateTrailRenderer();
                    }

                    //
                    carolHelp.TriggerGeneralAdvice("DangerIncoming");
                    //
                    AddDangerousBulletToList(nextBullet);
                }
                //
                continue;
            }
        }      

        //
        return nextBullet;
    }

    //
    public void ReturnBullet(GameObject bulletToRetire)
    {
        // Quitamos la parte (Clone) del nombre
        // Está un poco guarro
        string bulletName = bulletToRetire.name.Split('(')[0];
        //
        for (int i = 0; i < bulletPoolsPerType.Count; i++)
        {
            if (bulletPoolsPerType[i].prefabName == bulletName)
            {
                bulletToRetire.SetActive(false);
                bulletPoolsPerType[i].activeBullets.Remove(bulletToRetire);
                bulletPoolsPerType[i].reserveBullets.Add(bulletToRetire);
                //
                if (bulletPoolsPerType[i].dangerousEnough)
                    RemoveDangerousBulletFromList(bulletToRetire);
                //
                return;
            }
        }
        // Si llega aqui es que la bala no ha sido preparada para trabajar con el pool
        Debug.Log("Bullet not registered in bullet pool");
        Destroy(bulletToRetire);
    }

    #endregion

    #region Dangerous Bullet Methods

    public void AddDangerousBulletToList(GameObject incomingBullet)
    {
        dangerousBullets.Add(incomingBullet);
    }

    public void RemoveDangerousBulletFromList(GameObject incomingBullet)
    {
        Debug.Log("Removing dangueros bullet from list");
        dangerousBullets.Remove(incomingBullet);
    }

    #endregion
}

public class BulletTypePool
{
    public string prefabName;
    public List<GameObject> reserveBullets;
    public List<GameObject> activeBullets;
    public bool dangerousEnough;
    public Bullet bulletScript;

    public BulletTypePool(string prefabName)
    {
        this.prefabName = prefabName;
        reserveBullets = new List<GameObject>();
        activeBullets = new List<GameObject>();
    }
}