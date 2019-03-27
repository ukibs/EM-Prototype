using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvisionalHUD : MonoBehaviour {

    //public GUIStyle guiStyle;
    public GUISkin guiSkin;

    public Texture crossTexture;
    public Texture enemyMarkerTexture;
    public Texture enemyInScreenTexture;
    public Texture targetedEnemyEstimatedFuturePositionTexture;
    public Texture testingVelocityIcon;
    public Texture diamondsTexture;
    public Texture diamondsBackgroundTexture;
    public Texture shieldDamageTexture;
    public Texture hullDamageTexture;

    // Color Textures
    public Texture chargeTexture;
    public Texture playerShieldTexture;
    public Texture playerHealthTexture;
    public Texture enemyChasisTexture;
    public Texture enemyHealthTexture;

    // Jump ones
    public Texture jumpTexture;

    // Sprint ones 
    public Texture sprintTexture;

    // Attack ones
    public Texture pulseAttackIcon;
    public Texture rapidFireIcon;
    public Texture cannonIcon;
    public Texture particleCascadeIcon;
    public Texture piercingIcon;

    //Defense ones
    public Texture sphereDefenseTexture;

    //
    public float damageIndicatorLifeTime = 0.5f;

    //
    public Texture radarTexture;
    // TODO: Meter texturas para diferenciar alturas

    // Controls
    public Texture keyboardControls;
    public Texture gamepadControls;

    private RobotControl robotControl;
    private Camera mainCamera;
    private ImpactInfoManager impactInfoManager;
    private SpringCamera cameraControl;
    private GameManager gameManager;
    private PlayerIntegrity playerIntegrity;
    private List<DamageIndicator> damageIndicators;
    //
    private Vector2 radarDimensions;
    // De momento que sea un tercion del alto de la pantalla
    private float radarProportion = 0.3f;
    //
    private float currentAlpha = 1;
    private float fadeDirection = -1;

	// Use this for initialization
	void Start () {
        robotControl = FindObjectOfType<RobotControl>();
        mainCamera = Camera.main;
        impactInfoManager = FindObjectOfType<ImpactInfoManager>();
        cameraControl = mainCamera.GetComponent<SpringCamera>();
        gameManager = FindObjectOfType<GameManager>();
        playerIntegrity = FindObjectOfType<PlayerIntegrity>();
        damageIndicators = new List<DamageIndicator>(20);
        //
        radarDimensions = new Vector2(Screen.height * radarProportion, Screen.height * radarProportion);
	}
	
	// Update is called once per frame
	void Update () {
        //
        float dt = Time.deltaTime;
        //
        UpdateDamageIndicators(dt);
	}

    private void OnGUI()
    {
        //
        GUI.DrawTexture(new Rect(Screen.width/2 - 50, Screen.height/2 - 50, 100, 100), crossTexture);
        
        

        //
        PlayerHealthAndShields();

        //
        MarkEnemiesOnScreen();

        //
        DrawAbilityIcons();

        //
        if (!cameraControl.TargetingPlayer)
        {
            EnemyStats();
            
        }

        //
        DrawImpactInfo();

        //
        DrawDamageIndicators();

        //
        DrawRadarWithEnemies();

        //
        if (GameControl.paused)
        {
            DrawControls();
        }
    }

    //
    void DrawControls()
    {
        //Detect controllers on the beggining
        if (Input.GetJoystickNames().Length > 0 && Input.GetJoystickNames()[0] != "" )
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), gamepadControls);
        }
        else
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), keyboardControls);
        }
    }

    //
    void DrawImpactInfo()
    {
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

            // Datos del impact info manager
            GUI.Label(new Rect(impactInfoManager.ImpactInfoList[i].screenPosition.x,
                Screen.height - impactInfoManager.ImpactInfoList[i].screenPosition.y,
                100, 100), impactInfoManager.ImpactInfoList[i].info, guiSkin.label);

            if (impactInfoManager.ImpactInfoList[i].extraInfo != null)
                GUI.Label(new Rect(impactInfoManager.ImpactInfoList[i].screenPosition.x,
                    Screen.height - impactInfoManager.ImpactInfoList[i].screenPosition.y + 10, 100, 100),
                    impactInfoManager.ImpactInfoList[i].extraInfo, guiSkin.label);
        }
    }

    //
    void DrawAbilityIcons()
    {
        // Abilities diamond and icons
        GUI.DrawTexture(new Rect(Screen.width - 200, Screen.height - 200, 200, 200), diamondsBackgroundTexture);
        GUI.DrawTexture(new Rect(Screen.width - 200, Screen.height - 200, 200, 200), diamondsTexture);
        //
        ChargeDrawing();
        //
        Texture iconToUse = null;

        // Jump ones
        if (gameManager.unlockedJumpActions > 0)
        {
            GUI.DrawTexture(new Rect(Screen.width - 150, Screen.height - 100, 100, 100), jumpTexture);
        }

        //Sprint ones
        if (gameManager.unlockedSprintActions > 0)
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
                case AttackMode.ParticleCascade: iconToUse = particleCascadeIcon; break;
                case AttackMode.Piercing: iconToUse = piercingIcon; break;
            }
            GUI.DrawTexture(new Rect(Screen.width - 100, Screen.height - 150, 100, 100), iconToUse);
        }

        //Defense ones
        if (gameManager.unlockedDefenseActions > 0)
        {
            GUI.DrawTexture(new Rect(Screen.width - 150, Screen.height - 200, 100, 100), sphereDefenseTexture);
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
        // Escudos
        // Mientras le queden
        if (playerIntegrity.CurrentShield > 0)
        {
            float shieldBarLenght = playerIntegrity.CurrentShield / playerIntegrity.maxShield * barMaxLength;
            GUI.DrawTexture(new Rect(30, 30, shieldBarLenght, 30), playerShieldTexture);
        }
        // Cuando estén agotados
        else
        {
            //
            currentAlpha += fadeDirection * Time.deltaTime;
            if (currentAlpha >= 1) fadeDirection = -1;
            if (currentAlpha <= 0) fadeDirection = 1;
            GUI.color = new Color(1, 1, 1, currentAlpha);
            // De momento la referenciamos como enemyHelathTexture (color rojo)
            GUI.DrawTexture(new Rect(30, 30, barMaxLength, 30), enemyHealthTexture);
            GUI.color = new Color(1, 1, 1, 1);
        }
        //
        float healthBarLenght = playerIntegrity.CurrentHealth / playerIntegrity.maxHealth * barMaxLength;
        GUI.DrawTexture(new Rect(30, 60, healthBarLenght, 30), playerHealthTexture);
    }

    //
    void EnemyStats()
    {
        
        //
        EnemyConsistency enemyConsistency = EnemyAnalyzer.enemyConsistency;
        //
        if (enemyConsistency != null)
        {
            // Empezamos pintando el marcardor en la posición del enemigo en patnalla
            Vector3 enemyScreenPosition = Camera.main.WorldToViewportPoint(cameraControl.CurrentTarget.position);
            GUI.DrawTexture(new Rect(
                enemyScreenPosition.x * Screen.width - 50,
                Screen.height - enemyScreenPosition.y * Screen.height - 50, 100, 100),
                enemyMarkerTexture);

            // Marcador de posición estimada del enemigo
            Vector3 anticipatedPositionInScreen = mainCamera.WorldToViewportPoint(EnemyAnalyzer.estimatedToHitPosition);
            GUI.DrawTexture(new Rect(
                anticipatedPositionInScreen.x * Screen.width - 5,
                Screen.height - anticipatedPositionInScreen.y * Screen.height - 5, 10, 10),
                targetedEnemyEstimatedFuturePositionTexture);

            // Testing con la velocity
            Vector3 rbDirection1 = EnemyAnalyzer.enemyTransform.position + (EnemyAnalyzer.enemyRb.velocity * 1);
            rbDirection1 = mainCamera.WorldToViewportPoint(rbDirection1);
            GUI.DrawTexture(new Rect(
                rbDirection1.x * Screen.width - 5,
                Screen.height - rbDirection1.y * Screen.height - 5, 10, 10),
                testingVelocityIcon);
            Vector3 rbDirection2 = EnemyAnalyzer.enemyTransform.position + (EnemyAnalyzer.enemyRb.velocity * 2);
            rbDirection2 = mainCamera.WorldToViewportPoint(rbDirection2);
            GUI.DrawTexture(new Rect(
                rbDirection2.x * Screen.width - 5,
                Screen.height - rbDirection2.y * Screen.height - 5, 10, 10),
                testingVelocityIcon);


            // Barra de vida (chasis)
            //float enemyChasisHealthForBar = enemyConsistency.CurrentChasisHealth / enemyConsistency.maxChasisHealth;
            //enemyChasisHealthForBar = Mathf.Clamp01(enemyChasisHealthForBar);
            //GUI.DrawTexture(new Rect(Screen.width / 2 + 150, Screen.height / 2 - 50, enemyChasisHealthForBar * 100f, 20), enemyChasisTexture);
            //GUI.Label(new Rect(Screen.width / 2 + 150, Screen.height / 2 - 50, 100f, 20), " " + enemyConsistency.CurrentChasisHealth);
            // Barra de vida (core)
            float enemyCoreHealthForBar = enemyConsistency.CurrentHealth / enemyConsistency.maxHealth;
            enemyCoreHealthForBar = Mathf.Clamp01(enemyCoreHealthForBar);
            GUI.DrawTexture(new Rect(Screen.width / 2 + 150, Screen.height / 2 - 30, enemyCoreHealthForBar * 100f, 20), enemyHealthTexture);
            GUI.Label(new Rect(Screen.width / 2 + 150, Screen.height / 2 - 30, 100f, 20), " " + enemyConsistency.CurrentHealth);

            
            
            // TODO: Sacar distancia

            
            // Raycast para sacar el blindaje a tiro
            // Lo quitamos de momento a ver como afecta al performance
            //RaycastHit hitInfo;
            //if (Physics.Raycast(cameraControl.transform.position, cameraControl.transform.forward, out hitInfo))
            //{
            //    //Debug.Log(hitInfo.transform.name);
            //    EnemyCollider enemyCollider = hitInfo.collider.GetComponent<EnemyCollider>();
            //    //Debug.Log("Enemy collider: " + enemyCollider);
            //    if (enemyCollider != null)
            //    {
                    
            //        GUI.Label(new Rect(Screen.width / 2 + 150, Screen.height / 2, 100, 20), "Armor: " + enemyCollider.armor, guiSkin.label);

            //    }
            //}
            
        }
        else
        {
            // TODO: Revisar si quería ahcef esto aqui
            //cameraControl.SwitchTarget(null);
        }

        // TODO: Punto de estimada posición futura de enemuigo
        //robotControl.
    }

    //
    void MarkEnemiesOnScreen()
    {
        //
        // TODO: Cogerlo por refencia del manager apropiado cuando lo tengamos listo
        EnemyConsistency[] enemies = FindObjectsOfType<EnemyConsistency>();
        if (enemies.Length == 0)
            return;
        //
        for (int i = 0; i < enemies.Length; i++)
        {
            // Distancia al centro de pantalla
            Vector3 posInScreen =  Camera.main.WorldToViewportPoint(enemies[i].transform.position);
            bool inScreen = posInScreen.x >= 0 && posInScreen.x <= 1 &&
                posInScreen.y >= 0 && posInScreen.y <= 1 &&
                posInScreen.z > 0;
            //
            if (inScreen)
            {
                GUI.DrawTexture(new Rect(
                    posInScreen.x * Screen.width - 15,
                    Screen.height - posInScreen.y * Screen.height - 50, 30, 30),
                    enemyInScreenTexture);
            }
        }
    }

    //
    public void AddDamageIndicator(float angle, DamageType damageType)
    {
        DamageIndicator newDamageIndicator = new DamageIndicator();
        newDamageIndicator.angle = angle;
        newDamageIndicator.damageType = damageType;
        //
        damageIndicators.Add(newDamageIndicator);
    }

    //
    void UpdateDamageIndicators(float dt)
    {
        for(int i = 0; i < damageIndicators.Count; i++)
        {
            damageIndicators[i].timeAlive += dt;
            // Luego metemos parámetro bien
            if (damageIndicators[i].timeAlive >= damageIndicatorLifeTime)
                damageIndicators.RemoveAt(i);
        }
    }

    //
    private void DrawDamageIndicators()
    {
        GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
        for (int i = 0; i < damageIndicators.Count; i++)
        {
            Vector2 pivotPoint = new Vector2(Screen.width / 2, Screen.height / 2);
            GUIUtility.RotateAroundPivot(damageIndicators[i].angle, pivotPoint);
            //
            Texture textureToUse = shieldDamageTexture;
            switch (damageIndicators[i].damageType)
            {
                case DamageType.Shield:
                    textureToUse = shieldDamageTexture;
                    break;
                case DamageType.Hull:
                    textureToUse = hullDamageTexture;
                    break;
            }
            //
            float alpha = 1 - (damageIndicators[i].timeAlive / damageIndicatorLifeTime);
            GUI.color = new Color(1,1,1,alpha);
            //
            GUI.DrawTexture(new Rect(Screen.width / 2 - 50, Screen.height * 3/4, 100, 100), textureToUse);
            //
            GUIUtility.RotateAroundPivot(-damageIndicators[i].angle, pivotPoint);
        }
        GUI.EndGroup();
        //
        GUI.color = new Color(1, 1, 1, 1);
    }

    //
    void DrawRadarWithEnemies()
    {
        //
        // Igual pillamos el current up del player
        // La camara, coño
        float playerDirection = Vector3.SignedAngle(Vector3.forward, cameraControl.transform.forward, Vector3.up);
        playerDirection *= Mathf.Deg2Rad;
        //
        float maxDetection = 300;
        // Primero dibujamos el radar
        GUI.DrawTexture(new Rect(0, Screen.height - radarDimensions.y, radarDimensions.x, radarDimensions.y), radarTexture);
        // Y enemigos
        // TODO: Cogerlo por refencia del manager apropiado cuando lo tengamos listo
        EnemyConsistency[] enemies = FindObjectsOfType<EnemyConsistency>();
        if (enemies.Length == 0)
            return;
        //
        for (int i = 0; i < enemies.Length; i++)
        {
            Vector3 offset = enemies[i].transform.position - playerIntegrity.transform.position;
            // TODO: Sacar también altura
            offset.y = 0;
            float xzDistance = offset.magnitude;
            if(xzDistance < maxDetection)
            {
                // Sacamos la posición para el radar
                Vector2 posInRadar = new Vector2(offset.x * radarDimensions.x / maxDetection / 2 + (radarDimensions.x / 2),
                                        offset.z * radarDimensions.y / maxDetection / 2 + (radarDimensions.y / 2));
                // La adaptamos a la orientación del player
                // Desde el centro del radar, animalicao
                float radius = Mathf.Sqrt(Mathf.Pow(posInRadar.x - (radarDimensions.x/2), 2) 
                                    + Mathf.Pow(posInRadar.y - (radarDimensions.y / 2), 2));
                float angle = Mathf.Atan2(posInRadar.y - (radarDimensions.y / 2), 
                                posInRadar.x - (radarDimensions.x / 2));
                angle += playerDirection;
                posInRadar.x = radius * Mathf.Cos(angle) + (radarDimensions.x / 2);
                posInRadar.y = radius * Mathf.Sin(angle) + (radarDimensions.y / 2);
                // Y dibujamos
                GUI.DrawTexture(new Rect(posInRadar.x, Screen.height - posInRadar.y, 10, 10), enemyInScreenTexture);
            }
        }
    }
}

public enum DamageType
{
    Invalid = -1,

    Shield,
    Hull,

    Count
}

public class DamageIndicator
{
    public float angle;
    public float timeAlive;
    public float alpha;
    public DamageType damageType;
}
