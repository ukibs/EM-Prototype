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
    public GigaWormBehaviour gigaWormBehaviour;

    #endregion

    #region Private Attributes


    private float currentHealthPoints;
    private CarolBaseHelp carolBaseHelp;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //gigaWormBehaviour = GetComponentInParent<GigaWormBehaviour>();
        currentHealthPoints = maxHealthPoints;
        carolBaseHelp = FindObjectOfType<CarolBaseHelp>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
            if (currentHealthPoints <= 0)
            {
                gigaWormBehaviour.LoseWeakPoint();
                carolBaseHelp.WeakPointDestroyed();
                //PlayerReference.
                active = false;
                //Destroy(this);
                //Destroy(gameObject);
            }
        }
        
    }
}
