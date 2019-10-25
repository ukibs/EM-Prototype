using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBaseBehaviour : Targeteable
{
    public CarolStepObject[] carolStepObjects;

    #region Private Attributes

    protected RobotControl player;
    //private Rigidbody rb;
    protected ProvLevelManager levelManager;
    protected CarolBaseHelp carolHelp;
    protected CameraReference cameraReference;
    protected AudioSource audioSource;
    protected EnemyManager enemyManager;

    #endregion

    // Start is called before the first frame update
    protected virtual void Start()
    {
        player = FindObjectOfType<RobotControl>();
        audioSource = GetComponent<AudioSource>();
        levelManager = FindObjectOfType<ProvLevelManager>();
        carolHelp = FindObjectOfType<CarolBaseHelp>();
        cameraReference = FindObjectOfType<CameraReference>();
        enemyManager = FindObjectOfType<EnemyManager>();
        //
        if(carolStepObjects.Length > 0)
            carolHelp.SetStepObjects(carolStepObjects);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }
}
