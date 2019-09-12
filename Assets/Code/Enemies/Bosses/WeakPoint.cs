using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// La funcionalidad de estos puntos es avanzar fases en el enfrntamiento
// Demomento lo hacemos específico del gusano
// Luego lo generalizaroremos

public class WeakPoint : Targeteable
{
    #region Public Attibutes

    public float maxHealthPoints = 30;
    public bool reactionOnDamage = false;
    public GigaWormBehaviour gigaWormBehaviour;

    #endregion

    #region Private Attributes


    private float currentHealthPoints;
    private CarolBaseHelp carolBaseHelp;

    #endregion

    public float CurrentHealthPoins { get { return currentHealthPoints; } }

    // Start is called before the first frame update
    void Start()
    {
        //gigaWormBehaviour = GetComponentInParent<GigaWormBehaviour>();
        currentHealthPoints = maxHealthPoints;
        carolBaseHelp = FindObjectOfType<CarolBaseHelp>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        ReceiveBulletImpact();
    }

    public void ReceiveBulletImpact()
    {
        // TODO: Cambiar la detección a proximidad
        // Seguramente lo haga la propia Carol
        /*if(active == false && currentHealthPoints > 0)
        {
            active = true;
        }*/
        //
        if(currentHealthPoints > 0)
        {
            currentHealthPoints--;
            //
            if (reactionOnDamage)
            {
                gigaWormBehaviour.RespondToDamagedWeakPoint();
            }
            //
            if (currentHealthPoints <= 0)
            {
                gigaWormBehaviour.LoseWeakPoint();
                carolBaseHelp.WeakPointDestroyed();
                //TODO: Meteremos el churrazo de sangre
                active = false;
                //Destroy(this);
                //Destroy(gameObject);
                EnemyAnalyzer.Release();
            }
        }
        
    }
}
