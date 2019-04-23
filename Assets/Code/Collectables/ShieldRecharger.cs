using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldRecharger : MonoBehaviour
{
    //
    public float chargeAmount = 1000;
    public float rechargeRate = 10;

    //
    private void OnTriggerStay(Collider other)
    {
        PlayerIntegrity playerIntegrity = other.GetComponent<PlayerIntegrity>();
        if(playerIntegrity != null)
        {
            //
            if(playerIntegrity.CurrentShield < playerIntegrity.maxShield){
                //
                float amountToRecharge = rechargeRate * Time.deltaTime;
                playerIntegrity.CurrentShield += amountToRecharge;
                chargeAmount -= amountToRecharge;
                //
                if(chargeAmount <= 0)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    //
    public void ResetGenerator()
    {
        gameObject.SetActive(true);
        chargeAmount = 1000;
    }
}
