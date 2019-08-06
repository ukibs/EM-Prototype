using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactInfoManager : MonoBehaviour {

    private List<ImpactInfo> impactInfoList;

    private RobotControl robotControl;
    private ImpactInfo rapidFireImpactInfo;
    private bool rapidFireActive;

    private Camera mainCamera;

    

    #region Properties

    public List<ImpactInfo> ImpactInfoList { get { return impactInfoList; } }
    
    public ImpactInfo RapidFireImpactInfo { get { return rapidFireImpactInfo; } }

    #endregion

    // Use this for initialization
    void Start () {
        impactInfoList = new List<ImpactInfo>(100);
        //
        robotControl = FindObjectOfType<RobotControl>();
        // Iniciamos el de rapid fire
        rapidFireImpactInfo = new ImpactInfo();
        rapidFireImpactInfo.info = 0 + "";
        //
        mainCamera = Camera.main;
    }
	
	// Update is called once per frame
	void Update () {
        //
        float dt = Time.deltaTime;
        //
        for(int i = 0; i < impactInfoList.Count; i++)
        {
            impactInfoList[i].timeAlive += dt;
            //impactInfoList[i].screenPosition.y += dt;
            impactInfoList[i].screenPosition.y++;
            //impactInfoList[i].screenPosition = new Vector3(impactInfoList[i].screenPosition.x, impactInfoList[i].screenPosition.y + 1);
            if (impactInfoList[i].timeAlive > impactInfoList[i].lifeTime)
            {
                impactInfoList.RemoveAt(i);
            }
        }
        //
        rapidFireImpactInfo.timeAlive += dt;
        rapidFireImpactInfo.screenPosition.y++;
        if (rapidFireImpactInfo.timeAlive > rapidFireImpactInfo.lifeTime)
            rapidFireImpactInfo.damageValue = 0;
	}

    #region Methods
    
    // 
    public void SendImpactInfo(Vector3 point, int damageReceived, string extraInfo = "")
    {
        // TODO: Sacar los del player para manejar los de fuego rápido por separado

        // Cribamos valores bajos de momento
        if (damageReceived < 1) return;
        //
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(point);
        if (screenPosition.z < 0) return;

        //
        if (!CheckIfPlayerInRapidFire())
        {
            //
            ImpactInfo newImpactInfo = new ImpactInfo();
            newImpactInfo.damageValue = damageReceived;
            //newImpactInfo.info = force + " N";

            newImpactInfo.info = damageReceived + "";
            newImpactInfo.extraInfo = extraInfo;
            newImpactInfo.position = point;
            newImpactInfo.screenPosition = screenPosition;

            // TODO: Esto parece sobrar
            // Asegurarse de que es necesario
            if (newImpactInfo != null)
                impactInfoList.Add(newImpactInfo);
        }
        else
        {
            rapidFireImpactInfo.damageValue += damageReceived;
            rapidFireImpactInfo.info = Int32.Parse(rapidFireImpactInfo.info) + damageReceived + "";
            rapidFireImpactInfo.extraInfo = extraInfo;
            rapidFireImpactInfo.position = point;
            rapidFireImpactInfo.screenPosition = screenPosition;
            //
            rapidFireActive = true;
            rapidFireImpactInfo.timeAlive = 0;
        }

    }
    
    /// <summary>
    /// Chequeo de si el player está utilizando fuego rápido
    /// TODO: Chequear mejor si el impacto proviene de fuego rápido
    /// </summary>
    /// <returns></returns>
    bool CheckIfPlayerInRapidFire()
    {
        bool playerInRapidFire = false;

        playerInRapidFire = 
            robotControl.ActiveAttackMode == AttackMode.RapidFire && 
            robotControl.CurrentActionCharging == ActionCharguing.Attack;

        return playerInRapidFire;
    }

    public void EndRapidFire()
    {

    }

    #endregion
}

public class ImpactInfo
{
    public float lifeTime = 2;
    public float timeAlive = 0;
    //public float currentY;

    // Lo guardamos como entero para optimizar. Y por comodidad
    public int damageValue;
    public string info;
    public string extraInfo;
    public Vector3 position;
    public Vector3 screenPosition = new Vector3(-100, 0);   //Valor simbolico para manejarlo
}