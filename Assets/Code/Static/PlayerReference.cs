using UnityEngine;

// Referencia estática al player para que no haya qye estar haciento 
// GetComponent y FindObjectOfType todo el rato
// Le tenemos aqui de momento
public static class PlayerReference
{
    public static Transform playerTransform;
    public static Rigidbody playerRb;
    public static PlayerIntegrity playerIntegrity;
    public static RobotControl playerControl;
    public static bool isAlive;

    //
    public static int currentAttackAction;
    public static Rigidbody currentProyectileRB;

    public static void Initiate(GameObject playerGO)
    {
        playerTransform = playerGO.transform;
        playerRb = playerGO.GetComponent<Rigidbody>();
        playerIntegrity = playerGO.GetComponent<PlayerIntegrity>();
        playerControl = playerGO.GetComponent<RobotControl>();
        isAlive = true;
    }

    // TODO: Que pueda acceder directamente al gamemanager
    public static float GetCurrentWeaponPenetrationEstimation(/*GameManager gameManager*/)
    {

        // Sacamos la info de penetración del arma equipada
        float penetrationCapacity = -1;

        Rigidbody rFProyectileBody = PlayerReference.currentProyectileRB;
        Bullet rfProyectileData = playerControl.ProyectileToUse.GetComponent<Bullet>();
        penetrationCapacity = GeneralFunctions.Navy1940PenetrationCalc(
            rFProyectileBody.mass, rfProyectileData.diameter, playerControl.CurrentMuzzleSpeed);
        //
        return penetrationCapacity;
    }

    //
    public static void Die()
    {
        isAlive = false;
    }
}