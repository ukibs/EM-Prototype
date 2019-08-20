﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvisionalHUD : MonoBehaviour {

    //public GUIStyle guiStyle;
    public GUISkin guiSkin;

    public Texture crossTexture;
    //public Texture penetrationCrossRed;
    //public Texture penetrationCrossYellow;
    //public Texture penetrationCrossGreen;

    public Texture enemyMarkerTextureRed;
    public Texture enemyMarkerTextureYellow;
    public Texture enemyMarkerTextureGreen;

    public Texture[] enemyInScreenTextures;
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
    public Texture alertTexture;

    //
    public float damageIndicatorLifeTime = 0.5f;

    //
    public Texture radarTexture;
    public float radarRange = 500;
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
    private ProvLevelManager levelManager;
    private EnemyManager enemyManager;
    private BulletPool bulletPool;
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
        levelManager = FindObjectOfType<ProvLevelManager>();
        enemyManager = FindObjectOfType<EnemyManager>();
        bulletPool = FindObjectOfType<BulletPool>();
        //
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
        PlayerHealthAndShields();

        // Lo quitamos de momento
        if(EnemyAnalyzer.isActive && EnemyAnalyzer.enemyConsistency.IsMultipart)
            MarkEnemyPartsOnScreen();
        else
            MarkEnemiesOnScreen();

        //
        // DrawAbilityIcons();

        //
        if (!cameraControl.TargetingPlayer)
        {
            // Vamos a trabajar para que maneje varios tipos de objetivo
            // Enemy consistency (el viejo)
            // TODO: Gestionar los enemigos muertos de otra forma
            //if (cameraControl.CurrentTarget != null)
            if (EnemyAnalyzer.isActive)
            {
                EnemyConsistency enemyConsistency = EnemyAnalyzer.enemyConsistency;
                if (enemyConsistency != null)
                    EnemyInfoEC();
                // TODO: Hacerlo con weakpoints también
                else
                    EnemyInfoSimple();
            }
        }
        else
        {
            ShowCross();
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

        //
        ShowPlayerSpeed();

        //
        ShowChargedAmount();

        //
        ShowOverheat();

        //
        CheckAndDrawLevelEnd();

        //
        DrawDangerousBulletsDangerIndicators();
    }

    //
    void CheckAndDrawLevelEnd()
    {
        //
        if (levelManager.Finished)
        {
            Rect dimensions = new Rect(Screen.width * 1/4, Screen.height * 1/4, Screen.width * 1/2, Screen.width * 1/2);
            if (levelManager.Victory)
            {
                GUI.Label(dimensions, "VICTORY", guiSkin.customStyles[3]);
            }
            else
            {
                GUI.Label(dimensions, "DEFEAT", guiSkin.customStyles[3]);
            }
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
            //if (impactInfoManager.ImpactInfoList[i].screenPosition.x == -100)
            //{
            //    impactInfoManager.ImpactInfoList[i].screenPosition =
            //        mainCamera.WorldToScreenPoint(impactInfoManager.ImpactInfoList[i].position);
                // Clamp them
                //impactInfoManager.ImpactInfoList[i].screenPosition.x =
                //    Mathf.Clamp(impactInfoManager.ImpactInfoList[i].screenPosition.x, 0, Screen.width - 200);
                //impactInfoManager.ImpactInfoList[i].screenPosition.y =
                //    Mathf.Clamp(impactInfoManager.ImpactInfoList[i].screenPosition.y, 0, Screen.height - 20);
            //}

            //Vector3 screenCoordinates = mainCamera.WorldToScreenPoint(impactInfoManager.ImpactInfoList[i].position);

            // Datos del impact info manager
            // Cribamos los que estén por detrás del player
            // TODO: Mirar si renta más cribarlo en el momento de construirlo
            if (impactInfoManager.ImpactInfoList[i].screenPosition.z > 0)
            {
                GUI.Label(new Rect(impactInfoManager.ImpactInfoList[i].screenPosition.x,
                   Screen.height - impactInfoManager.ImpactInfoList[i].screenPosition.y,
                   200, 100), impactInfoManager.ImpactInfoList[i].damageValue + "", guiSkin.label);
                // Mesanje aosicado a la entrada
                if (impactInfoManager.ImpactInfoList[i].extraInfo != null)
                    GUI.Label(new Rect(impactInfoManager.ImpactInfoList[i].screenPosition.x,
                        Screen.height - impactInfoManager.ImpactInfoList[i].screenPosition.y + 20, 200, 100),
                        impactInfoManager.ImpactInfoList[i].extraInfo, guiSkin.label);
            }           
        }

        // El de fuego rápido
        if(impactInfoManager.RapidFireImpactInfo.damageValue > 0)
        {
            //impactInfoManager.RapidFireImpactInfo.screenPosition =
            //        mainCamera.WorldToScreenPoint(impactInfoManager.RapidFireImpactInfo.position);

            GUI.Label(new Rect(impactInfoManager.RapidFireImpactInfo.screenPosition.x,
                       Screen.height - impactInfoManager.RapidFireImpactInfo.screenPosition.y,
                       200, 100), impactInfoManager.RapidFireImpactInfo.damageValue + "", guiSkin.label);

            GUI.Label(new Rect(impactInfoManager.RapidFireImpactInfo.screenPosition.x,
                            Screen.height - impactInfoManager.RapidFireImpactInfo.screenPosition.y + 20, 200, 100),
                            impactInfoManager.RapidFireImpactInfo.extraInfo, guiSkin.label);
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
        float barMaxLength = Screen.width * 1 / 2;
        float barHeight = Screen.height * 1 / 24;
        float generalStartPoint = Screen.width - (barMaxLength * 3 / 2);

        // Y la vida
        float healthBarLenght = playerIntegrity.CurrentHealth / playerIntegrity.maxHealth * barMaxLength;
        // Que se centre la barra conforme se vacia/llena
        float healthStartPoint = generalStartPoint + ((barMaxLength - healthBarLenght) / 2);
        GUI.DrawTexture(new Rect(healthStartPoint, 30 /*+ barHeight*/, healthBarLenght, barHeight), playerHealthTexture);
        
        // Escudos
        // Mientras le queden
        if (playerIntegrity.CurrentShield > 0)
        {
            float shieldBarLenght = playerIntegrity.CurrentShield / playerIntegrity.maxShield * barMaxLength;
            // Que se centre la barra conforme se vacia/llena
            float shieldStartPoint = generalStartPoint + ((barMaxLength - shieldBarLenght) / 2);
            GUI.DrawTexture(new Rect(shieldStartPoint, 30, shieldBarLenght, barHeight), playerShieldTexture);
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
            GUI.DrawTexture(new Rect(generalStartPoint, 30, barMaxLength, barHeight), enemyHealthTexture);
            GUI.color = new Color(1, 1, 1, 1);
        }
        
    }

    //
    void ShowPlayerSpeed()
    {
        float playerCurrentSpeed = PlayerReference.playerRb.velocity.magnitude;
        playerCurrentSpeed *= 3.6f;
        int playerSpeedInt = (int)playerCurrentSpeed;
        GUI.Label(new Rect(30, Screen.height - 50, 300, 30), "Speed: " + playerSpeedInt + " km/h", guiSkin.label);
    }

    //
    void ShowCross()
    {
        GUI.DrawTexture(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 50, 100, 100), crossTexture);
    }

    // 
    void ShowChargedAmount()
    {
        //
        float chargedAmount = robotControl.ChargedAmount;
        Vector2 chargePos = new Vector2(Screen.width - 350, Screen.height - 100);
        //
        GUI.DrawTexture(new Rect(chargePos.x, chargePos.y, 300 * chargedAmount, 20), testingVelocityIcon);
        
        //
        GUI.Label(new Rect(chargePos.x, chargePos.y, 300, 20), "CHARGE", guiSkin.label);
    }

    //
    void ShowOverheat()
    {
        //
        float overHeat = robotControl.CurrentOverHeat;
        float maxOverheat = gameManager.maxOverheat;
        Vector2 overHeatPos = new Vector2(Screen.width - 350, Screen.height - 70);
        //
        GUI.DrawTexture(new Rect(overHeatPos.x, overHeatPos.y, 300 * overHeat / maxOverheat, 20), enemyChasisTexture);
        //
        if (robotControl.TotalOverheat)
        {
            //
            currentAlpha += fadeDirection * Time.deltaTime;
            if (currentAlpha >= 1) fadeDirection = -1;
            if (currentAlpha <= 0) fadeDirection = 1;
            GUI.color = new Color(1, 1, 1, currentAlpha);
            // De momento la referenciamos como enemyHelathTexture (color rojo)
            GUI.DrawTexture(new Rect(overHeatPos.x, overHeatPos.y, 300, 20), enemyHealthTexture);
            GUI.color = new Color(1, 1, 1, 1);
        }
        //
        GUI.Label(new Rect(overHeatPos.x, overHeatPos.y, 300, 20), "OVERHEAT", guiSkin.label);
    }

    /// <summary>
    /// For now is used for weakpoints
    /// </summary>
    void EnemyInfoSimple()
    {
        // TODO: Sacar distancia
        float distance = (playerIntegrity.transform.position - EnemyAnalyzer.enemyTransform.position).magnitude;
        int distanceToShow = (int)distance;
        GUI.Label(new Rect(Screen.width / 2 + 150, Screen.height / 2, 300, 30), "Distance: " + distanceToShow, guiSkin.label);

        // Empezamos pintando el marcardor en la posición del enemigo en patnalla
        Vector3 enemyScreenPosition = Camera.main.WorldToViewportPoint(cameraControl.CurrentTarget.position);
        //GUI.DrawTexture(new Rect(
        //    enemyScreenPosition.x * Screen.width - 50,
        //    Screen.height - enemyScreenPosition.y * Screen.height - 50, 100, 100),
        //    enemyMarkerTextureRed);
        GUI.DrawTexture(new Rect(
            (enemyScreenPosition.x * Screen.width) - 50,
            Screen.height - (enemyScreenPosition.y * Screen.height) - 50, 100, 100),
            enemyMarkerTextureRed);

        //
        WeakPoint weakPoint = EnemyAnalyzer.enemyTransform.GetComponent<WeakPoint>();
        if(weakPoint != null)
        {
            // Barra de vida
            float weakPointHealth = weakPoint.CurrentHealthPoins / weakPoint.maxHealthPoints;
            weakPointHealth = Mathf.Clamp01(weakPointHealth);
            GUI.DrawTexture(new Rect(Screen.width / 2 + 150, Screen.height / 2 - 30, weakPointHealth * 100f, 20), enemyHealthTexture);
            //GUI.Label(new Rect(Screen.width / 2 + 150, Screen.height / 2 - 30, 100f, 20), " " + weakPoint.CurrentHealthPoins);
        }
        
    }

    //
    void EnemyInfoEC()
    {
        
        //
        EnemyConsistency enemyConsistency = EnemyAnalyzer.enemyConsistency;
        //
        if (enemyConsistency != null)
        {
            // Marcador de posición estimada del enemigo
            Vector3 anticipatedPositionInScreen = mainCamera.WorldToViewportPoint(EnemyAnalyzer.estimatedToHitPosition);
            GUI.DrawTexture(new Rect(
                anticipatedPositionInScreen.x * Screen.width - 2,
                Screen.height - anticipatedPositionInScreen.y * Screen.height - 2, 4, 4),
                targetedEnemyEstimatedFuturePositionTexture);

            // Testing con la velocity
            //Vector3 rbDirection1 = EnemyAnalyzer.enemyTransform.position + (EnemyAnalyzer.enemyRb.velocity * 1);
            //rbDirection1 = mainCamera.WorldToViewportPoint(rbDirection1);
            //GUI.DrawTexture(new Rect(
            //    rbDirection1.x * Screen.width - 5,
            //    Screen.height - rbDirection1.y * Screen.height - 5, 10, 10),
            //    testingVelocityIcon);
            //Vector3 rbDirection2 = EnemyAnalyzer.enemyTransform.position + (EnemyAnalyzer.enemyRb.velocity * 2);
            //rbDirection2 = mainCamera.WorldToViewportPoint(rbDirection2);
            //GUI.DrawTexture(new Rect(
            //    rbDirection2.x * Screen.width - 5,
            //    Screen.height - rbDirection2.y * Screen.height - 5, 10, 10),
            //    testingVelocityIcon);
            
            // Barra de vida
            float enemyCoreHealthForBar = enemyConsistency.CurrentHealth / enemyConsistency.maxHealth;
            enemyCoreHealthForBar = Mathf.Clamp01(enemyCoreHealthForBar);
            GUI.DrawTexture(new Rect(Screen.width / 2 + 150, Screen.height / 2 - 30, enemyCoreHealthForBar * 100f, 20), enemyHealthTexture);
            //GUI.Label(new Rect(Screen.width / 2 + 150, Screen.height / 2 - 30, 100f, 20), " " + enemyConsistency.CurrentHealth);

            // TODO: Sacar vida de la parte extra
            EnemyCollider enemyCollider = EnemyAnalyzer.enemyCollider;
            if(enemyCollider != null && enemyCollider.isTargeteable && enemyCollider.maxLocationHealth > 0)
            {
                float enemyPartHealthForBar = enemyCollider.CurrentLocationHealth / enemyCollider.maxLocationHealth;
                enemyPartHealthForBar = Mathf.Clamp01(enemyPartHealthForBar);
                GUI.DrawTexture(new Rect(Screen.width / 2 + 150, Screen.height / 2, enemyPartHealthForBar * 100f, 20), enemyHealthTexture);
            }
                

            // 
            float distance = (playerIntegrity.transform.position - enemyConsistency.transform.position).magnitude;
            int distanceToShow = (int)distance;
            GUI.Label(new Rect(Screen.width / 2 + 150, Screen.height / 2, 300, 20), "Distance: " + distanceToShow, guiSkin.label);


            // Raycast para saber si el enemigo está a tiro
            RaycastHit hitInfo;
            Vector3 enemyDirection = EnemyAnalyzer.enemyTransform.TransformPoint(EnemyAnalyzer.enemyConsistency.centralPointOffset)
                                    - cameraControl.transform.position;
            if (Physics.Raycast(cameraControl.transform.position, enemyDirection, out hitInfo, enemyDirection.magnitude))
            {
                //Debug.Log(hitInfo.transform.name);
                EnemyCollider possibleHit = hitInfo.collider.GetComponent<EnemyCollider>();
                //Debug.Log("Enemy collider: " + enemyCollider);
                if (possibleHit != null)
                {

                    //GUI.DrawTexture(new Rect(Screen.width / 2 - 25, Screen.height / 2 - 25, 50, 50), enemyMarkerTextureRed);
                    Vector3 enemyScreenPosition = Camera.main.WorldToViewportPoint(cameraControl.CurrentTarget.position);
                    GUI.DrawTexture(new Rect(
                        (enemyScreenPosition.x * Screen.width) - 25,
                        Screen.height - (enemyScreenPosition.y * Screen.height) - 25, 50, 50),
                        enemyMarkerTextureRed);
                }
            }
            
        }
    }

    //
    //void Mark

    //
    void MarkEnemiesOnScreen()
    {
        // TODO: Hacerlo con Targeteable
        // TODO: Cogerlo por refencia del manager apropiado cuando lo tengamos listo
        Targeteable[] enemies = FindObjectsOfType<Targeteable>();
        if (enemies.Length == 0)
            return;
        //
        for (int i = 0; i < enemies.Length; i++)
        {
            //
            if (!enemies[i].active)
                continue;
            //
            if (!enemies[i].markWhenNotTargeted && enemies[i] != EnemyAnalyzer.targeteable)
                continue;
            // Distancia al centro de pantalla
            Vector3 posInScreen =  Camera.main.WorldToViewportPoint(enemies[i].transform.position);
            bool inScreen = posInScreen.x >= 0 && posInScreen.x <= 1 &&
                posInScreen.y >= 0 && posInScreen.y <= 1 &&
                posInScreen.z > 0;
            //
            if (inScreen)
            {
                //
                EnemyIdentifier enemyIdentifier = enemies[i].GetComponentInParent<EnemyIdentifier>();
                //
                GUI.DrawTexture(new Rect(
                    posInScreen.x * Screen.width - 15,
                    Screen.height - posInScreen.y * Screen.height - 50, 30, 30),
                    enemyInScreenTextures[0]);
                //enemyInScreenTextures[(int)enemyIdentifier.enemyType]);
            }
        }
    }

    //
    void MarkEnemyPartsOnScreen()
    {
        //
        List<EnemyCollider> targeteableColliders = EnemyAnalyzer.enemyConsistency.TargeteableColliders;
        for (int i = 0; i < targeteableColliders.Count; i++)
        {
            //
            Vector3 posInScreen = Camera.main.WorldToViewportPoint(targeteableColliders[i].transform.position);
            //
            GUI.DrawTexture(new Rect(
                posInScreen.x * Screen.width - 15,
                Screen.height - posInScreen.y * Screen.height - 50, 30, 30),
                enemyInScreenTextures[0]);
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
            float iconHeight = Screen.height * 1 / 4;
            float iconWidth = Screen.width * 1 / 4;
            GUI.DrawTexture(new Rect(Screen.width / 2 - (iconWidth / 2), Screen.height * 3/4, iconWidth, iconHeight), textureToUse);
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

        // TODO: Decidir si dinámico el alcance del radar

        // Primero dibujamos el radar
        GUI.DrawTexture(new Rect(0, Screen.height - radarDimensions.y, radarDimensions.x, radarDimensions.y), radarTexture);
        // Epicentro si toca
        DrawEpicenterInRadar(playerDirection);
        // Y enemigos
        // TODO: Cogerlo por refencia del manager apropiado cuando lo tengamos listo
        Targeteable[] enemies = FindObjectsOfType<Targeteable>();
        if (enemies.Length == 0)
            return;
        // Aqui pasamos por todos los enemigos
        for (int i = 0; i < enemies.Length; i++)
        {
            //
            if (!enemies[i].active)
                continue;
            //
            Vector3 offset = enemies[i].transform.position - playerIntegrity.transform.position;
            // TODO: Sacar también altura
            offset.y = 0;
            //
            float xzDistance = offset.magnitude;
            if (xzDistance > radarRange)
                offset = Vector3.ClampMagnitude(offset, radarRange);

            // Sacamos la posición para el radar
            Vector2 posInRadar = new Vector2(offset.x * radarDimensions.x / radarRange / 2 + (radarDimensions.x / 2),
                                    offset.z * radarDimensions.y / radarRange / 2 + (radarDimensions.y / 2));
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
            EnemyIdentifier enemyIdentifier = enemies[i].GetComponentInParent<EnemyIdentifier>();
            //
            if(enemyIdentifier != null)
            GUI.DrawTexture(new Rect(posInRadar.x, Screen.height - posInRadar.y, 10, 10), 
                enemyInScreenTextures[(int)enemyIdentifier.enemyType]);
        }

        // Y los ataques peligrosos también
        // TODO: Sacar función para no tener codigo repe
        for(int i = 0; i < bulletPool.DangerousBullets.Count; i++)
        {
            //
            Vector3 offset = bulletPool.DangerousBullets[i].transform.position - playerIntegrity.transform.position;
            // TODO: Sacar también altura
            offset.y = 0;
            //
            float xzDistance = offset.magnitude;
            if (xzDistance > radarRange)
                offset = Vector3.ClampMagnitude(offset, radarRange);
            //
            Vector2 posInRadar = new Vector2(offset.x * radarDimensions.x / radarRange / 2 + (radarDimensions.x / 2),
                                        offset.z * radarDimensions.y / radarRange / 2 + (radarDimensions.y / 2));
            // La adaptamos a la orientación del player
            // Desde el centro del radar, animalicao
            float radius = Mathf.Sqrt(Mathf.Pow(posInRadar.x - (radarDimensions.x / 2), 2)
                                + Mathf.Pow(posInRadar.y - (radarDimensions.y / 2), 2));
            float angle = Mathf.Atan2(posInRadar.y - (radarDimensions.y / 2),
                            posInRadar.x - (radarDimensions.x / 2));
            angle += playerDirection;
            posInRadar.x = radius * Mathf.Cos(angle) + (radarDimensions.x / 2);
            posInRadar.y = radius * Mathf.Sin(angle) + (radarDimensions.y / 2);
            //
            GUI.DrawTexture(new Rect(posInRadar.x, Screen.height - posInRadar.y, 10, 10), alertTexture);
        }
    }

    /// <summary>
    /// Dibuja epicentro en el radar
    /// </summary>
    private void DrawEpicenterInRadar(float playerDirection)
    {
        if (enemyManager.CurrentEpicenterMode == EnemyManager.EpicenterMode.FixedPoint)
        {
            //
            Vector3 epicenterOffset = enemyManager.EpicenterPoint - playerIntegrity.transform.position;
            epicenterOffset.y = 0;
            //
            if (epicenterOffset.magnitude > radarRange)
                epicenterOffset = Vector3.ClampMagnitude(epicenterOffset, radarRange);
            //
            // Sacamos la posición para el radar
            Vector2 posInRadar = new Vector2(epicenterOffset.x * radarDimensions.x / radarRange / 2 + (radarDimensions.x / 2),
                                    epicenterOffset.z * radarDimensions.y / radarRange / 2 + (radarDimensions.y / 2));
            // La adaptamos a la orientación del player
            // Desde el centro del radar, animalicao
            float radius = Mathf.Sqrt(Mathf.Pow(posInRadar.x - (radarDimensions.x / 2), 2)
                                + Mathf.Pow(posInRadar.y - (radarDimensions.y / 2), 2));
            float angle = Mathf.Atan2(posInRadar.y - (radarDimensions.y / 2),
                            posInRadar.x - (radarDimensions.x / 2));
            angle += playerDirection;
            posInRadar.x = radius * Mathf.Cos(angle) + (radarDimensions.x / 2);
            posInRadar.y = radius * Mathf.Sin(angle) + (radarDimensions.y / 2);
            // Y dibujamos
            GUI.DrawTexture(new Rect(posInRadar.x, Screen.height - posInRadar.y, 10, 10),
                enemyHealthTexture);
        }
    }

    //
    private void DrawDangerousBulletsDangerIndicators()
    {
        //
        for(int i = 0; i < bulletPool.DangerousBullets.Count; i++)
        {
            //
            Vector3 bulletScreenPosition = mainCamera.WorldToViewportPoint(bulletPool.DangerousBullets[i].transform.position);
            //
            if (bulletScreenPosition.z < 0)
                return;
            //
            Rect iconRect = new Rect(bulletScreenPosition.x * Screen.width - 25, 
                                    Screen.height - (bulletScreenPosition.y * Screen.height) - 25,
                                    50, 50);
            GUI.DrawTexture(iconRect, alertTexture);
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
