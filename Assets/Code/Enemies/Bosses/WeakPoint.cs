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

    #endregion

    #region Private Attributes

    private GigaWormBehaviour gigaWormBehaviour;
    private float currentHealthPoints;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        gigaWormBehaviour = GetComponentInParent<GigaWormBehaviour>();
        currentHealthPoints = maxHealthPoints;
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
        //
        if(active == false && currentHealthPoints > 0)
        {
            active = true;
        }
        //
        if(currentHealthPoints > 0)
        {
            currentHealthPoints--;
            if (currentHealthPoints <= 0)
            {
                gigaWormBehaviour.LoseWeakPoint();
                //PlayerReference.
                active = false;
                //Destroy(this);
                //Destroy(gameObject);
            }
        }
        
    }
}
