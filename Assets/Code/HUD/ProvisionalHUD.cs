using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvisionalHUD : MonoBehaviour {

    //public GUIStyle guiStyle;
    public GUISkin guiSkin;

    public Texture crossTexture;
    public Texture diamondsTexture;

    // Color Textures
    public Texture chargeTexture;
    public Texture playerShieldTexture;
    public Texture playerHealthTexture;
    public Texture enemyChasisTexture;
    public Texture enemyCoreTexture;

    // Jump ones
    public Texture jumpTexture;

    // Sprint ones 
    public Texture sprintTexture;

    // Attack ones
    public Texture pulseAttackIcon;
    public Texture rapidFireIcon;
    public Texture cannonIcon;

    //Defense ones
    public Texture sphereDefenseTexture;

    private RobotControl robotControl;
    private Camera mainCamera;
    private ImpactInfoManager impactInfoManager;
    private SpringCamera cameraControl;
    private GameManager gameManager;
    private PlayerIntegrity playerIntegrity;

	// Use this for initialization
	void Start () {
        robotControl = FindObjectOfType<RobotControl>();
        mainCamera = Camera.main;
        impactInfoManager = FindObjectOfType<ImpactInfoManager>();
        cameraControl = mainCamera.GetComponent<SpringCamera>();
        gameManager = FindObjectOfType<GameManager>();
        playerIntegrity = FindObjectOfType<PlayerIntegrity>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGUI()
    {
        //
        GUI.DrawTexture(new Rect(Screen.width/2 - 50, Screen.height/2 - 50, 100, 100), crossTexture);
        
        //
        ChargeDrawing();

        //
        PlayerHealthAndShields();

        // Abilities diamond and icons
        GUI.DrawTexture(new Rect(Screen.width - 200, Screen.height - 200, 200, 200), diamondsTexture);
        Texture iconToUse = null;

        // Jump ones
        if(gameManager.unlockedJumpActions > 0)
        {
            GUI.DrawTexture(new Rect(Screen.width - 150, Screen.height - 100, 100, 100), jumpTexture);
        }

        //Sprint ones
        if(gameManager.unlockedSprintActions > 0)
        {
            GUI.DrawTexture(new Rect(Screen.width - 200, Screen.height - 150, 100, 100), sprintTexture);
        }

        // Attack ones
        if (gameManager.unlockedAttackActions > 0)
        {
            AttackMode attackMode = robotControl.ActiveAttackMode;
            switch (attackMode)
            {
                case AttackMode.Pulse: iconToUse = pulseAttackIcon; break;
                case AttackMode.RapidFire: iconToUse = rapidFireIcon; break;
                case AttackMode.Canon: iconToUse = cannonIcon; break;
            }
            GUI.DrawTexture(new Rect(Screen.width - 100, Screen.height - 150, 100, 100), iconToUse);
        }

        //Defense ones
        if(gameManager.unlockedDefenseActions > 0)
        {
            GUI.DrawTexture(new Rect(Screen.width - 150, Screen.height - 200, 100, 100), sphereDefenseTexture);
        }

        //
        if (!cameraControl.TargetingPlayer)
        {
            EnemyStats();
            
        }

        //
        for (int i = 0; i < impactInfoManager.ImpactInfoList.Count; i++)
        {
            //
            if (impactInfoManager.ImpactInfoList[i].screenPosition.x == -100)
            {
                impactInfoManager.ImpactInfoList[i].screenPosition = 
                    mainCamera.WorldToScreenPoint(impactInfoManager.ImpactInfoList[i].position);
                // Clamp them
                //impactInfoManager.ImpactInfoList[i].screenPosition.x =
                //    Mathf.Clamp(impactInfoManager.ImpactInfoList[i].screenPosition.x, 0, Screen.width - 200);
                //impactInfoManager.ImpactInfoList[i].screenPosition.y =
                //    Mathf.Clamp(impactInfoManager.ImpactInfoList[i].screenPosition.y, 0, Screen.height - 20);
            }

            //Vector3 screenCoordinates = mainCamera.WorldToScreenPoint(impactInfoManager.ImpactInfoList[i].position);

            //
            GUI.Label(new Rect(impactInfoManager.ImpactInfoList[i].screenPosition.x,
                Screen.height - impactInfoManager.ImpactInfoList[i].screenPosition.y, 
                100, 100), impactInfoManager.ImpactInfoList[i].info, guiSkin.label);

            if(impactInfoManager.ImpactInfoList[i].extraInfo != null)
                GUI.Label(new Rect(impactInfoManager.ImpactInfoList[i].screenPosition.x, 
                    Screen.height - impactInfoManager.ImpactInfoList[i].screenPosition.y + 10, 100, 100), 
                    impactInfoManager.ImpactInfoList[i].extraInfo, guiSkin.label);
        }
    }

    //
    void ChargeDrawing()
    {
        
        // Decidimos las coordenadas
        Vector2 chargePos = new Vector2();
        switch (robotControl.CurrentActionCharging)
        {
            case ActionCharguing.Attack:
                chargePos = new Vector2(Screen.width - 100, Screen.height - 150);
                break;
            case ActionCharguing.Defense:
                chargePos = new Vector2(Screen.width - 150, Screen.height - 200);
                break;
            case ActionCharguing.Jump:
                chargePos = new Vector2(Screen.width - 150, Screen.height - 100);
                break;
            case ActionCharguing.Sprint:
                chargePos = new Vector2(Screen.width - 200, Screen.height - 150);
                break;
            default:
                chargePos = new Vector2(1000, 1000); //Lo mandamos a cuenca
                break;
        }

        //
        float chargedAmount = robotControl.ChargedAmount;
        GUI.DrawTexture(new Rect(chargePos.x, chargePos.y + ((1 - chargedAmount) * 100), 100, 100 * chargedAmount), chargeTexture);
    }

    //
    void PlayerHealthAndShields()
    {
        //
        float barMaxLength = 300;
        //
        float shieldBarLenght = playerIntegrity.CurrentShield / playerIntegrity.maxShield * barMaxLength; 
        GUI.DrawTexture(new Rect(30, 30, shieldBarLenght, 30), playerShieldTexture);
        //
        float healthBarLenght = playerIntegrity.CurrentHealth / playerIntegrity.maxHealth * barMaxLength;
        GUI.DrawTexture(new Rect(30, 60, healthBarLenght, 30), playerHealthTexture);
    }

    //
    void EnemyStats()
    {
        //
        EnemyConsistency enemyConsistency = cameraControl.CurrentTarget.GetComponent<EnemyConsistency>();
        //
        if (enemyConsistency != null)
        {
            //EnemyConsistency enemyConsistency = cameraControl.CurrentTarget.GetComponent<EnemyConsistency>();
            //
            float enemyChasisHealthForBar = enemyConsistency.CurrentChasisHealth / enemyConsistency.maxChasisHealth;
            enemyChasisHealthForBar = Mathf.Clamp01(enemyChasisHealthForBar);
            GUI.DrawTexture(new Rect(Screen.width / 2 + 150, Screen.height / 2 - 50, enemyChasisHealthForBar * 100f, 20), enemyChasisTexture);
            GUI.Label(new Rect(Screen.width / 2 + 150, Screen.height / 2 - 50, 100f, 20), " " + enemyConsistency.CurrentChasisHealth);
            //
            float enemyCoreHealthForBar = enemyConsistency.CurrentCoreHealth / enemyConsistency.maxCoreHealth;
            enemyCoreHealthForBar = Mathf.Clamp01(enemyCoreHealthForBar);
            GUI.DrawTexture(new Rect(Screen.width / 2 + 150, Screen.height / 2 - 30, enemyCoreHealthForBar * 100f, 20), enemyCoreTexture);
            GUI.Label(new Rect(Screen.width / 2 + 150, Screen.height / 2 - 30, 100f, 20), " " + enemyConsistency.CurrentCoreHealth);
            //
            
            // Raycast para sacar el blindaje a tiro
            RaycastHit hitInfo;
            if (Physics.Raycast(cameraControl.transform.position, cameraControl.transform.forward, out hitInfo))
            {
                //Debug.Log(hitInfo.transform.name);
                EnemyCollider enemyCollider = hitInfo.collider.GetComponent<EnemyCollider>();
                //Debug.Log("Enemy collider: " + enemyCollider);
                if (enemyCollider != null)
                {
                    
                    GUI.Label(new Rect(Screen.width / 2 + 150, Screen.height / 2, 100, 20), "Armor: " + enemyCollider.armor, guiSkin.label);

                }
            }
            
        }
        else
        {
            cameraControl.SwitchTarget(null);
        }
    }
}
