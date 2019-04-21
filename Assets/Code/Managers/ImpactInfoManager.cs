using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactInfoManager : MonoBehaviour {

    private List<ImpactInfo> impactInfoList;

    #region Properties

    public List<ImpactInfo> ImpactInfoList { get { return impactInfoList; } }
    

    #endregion

    // Use this for initialization
    void Start () {
        impactInfoList = new List<ImpactInfo>(100);
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
	}

    #region Methods

    public void SendImpactInfo(Vector3 point, float force)
    {
        ImpactInfo newImpactInfo = new ImpactInfo();
        //newImpactInfo.info = force + " N";
        newImpactInfo.info = (int)force + "";
        newImpactInfo.position = point;
        //
        if(newImpactInfo != null)
            impactInfoList.Add(newImpactInfo);
    }

    public void SendImpactInfo(Vector3 point, float force, string extraInfo)
    {
        ImpactInfo newImpactInfo = new ImpactInfo();
        newImpactInfo.info = force + " N";
        newImpactInfo.extraInfo = extraInfo;
        newImpactInfo.position = point;
        //newImpactInfo.currentY = point.y;
        //
        if (newImpactInfo != null)
            impactInfoList.Add(newImpactInfo);
    }
    

    #endregion
}

public class ImpactInfo
{
    public float lifeTime = 1;
    public float timeAlive = 0;
    //public float currentY;
    public string info;
    public string extraInfo;
    public Vector3 position;
    public Vector3 screenPosition = new Vector3(-100, 0);   //Valor simbolico para manejarlo
}