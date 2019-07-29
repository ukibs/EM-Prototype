using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GigaWormInsides : MonoBehaviour
{
    //
    public float stunnedAcidDamage = 50;
    public float recoveringAcidDamage = 200;
    public float recoveringRepulsionForce = 2;


    //
    private bool playerOut = true;
    private GigaWormBehaviour gigaWormBehaviour;
    private RobotControl player;
    private Rigidbody playerRb;

    //
    public bool PlayerOut {
        get { return playerOut; }
        set { playerOut = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        gigaWormBehaviour = FindObjectOfType<GigaWormBehaviour>();
        player = FindObjectOfType<RobotControl>();
        playerRb = PlayerReference.playerRb;
    }

    //
    private void Update()
    {
        //
        float dt = Time.deltaTime;
        //
        if ((gigaWormBehaviour.CurrentState == GigaWormBehaviour.WormStatus.Stunned && !playerOut) ||
            (gigaWormBehaviour.CurrentState == GigaWormBehaviour.WormStatus.Recovering && !playerOut))
        {
            InsidesDamageToPlayer(dt);
        }
    }

    // Aqui trabajaremos la salida del ojete
    private void OnTriggerEnter(Collider other)
    {
        //
        RobotControl possiblePlayer = other.GetComponent<RobotControl>();
        if(possiblePlayer != null)
            ShitPlayer();
    }

    // Daño al player mientras esté dentro del bicho
    void InsidesDamageToPlayer(float dt)
    {
        //
        float damageToApply = 0;
        switch (gigaWormBehaviour.CurrentState)
        {
            case GigaWormBehaviour.WormStatus.Stunned:
                damageToApply = stunnedAcidDamage;
                break;
            case GigaWormBehaviour.WormStatus.Recovering:
                damageToApply = recoveringAcidDamage;
                playerRb.AddForce(-transform.right * recoveringRepulsionForce * dt, ForceMode.Impulse);
                break;
        }
        // 
        PlayerReference.playerIntegrity.ReceiveEnvionmentalDamage(100 * dt);
    }

    // Eject the player to the exterior
    void ShitPlayer()
    {
        //
        //if(exitPoint.position.y < 0)
        //    player.transform.position = new Vector3(exitPoint.position.x, 0, exitPoint.position.z);
        //
        //else
        player.transform.position = gigaWormBehaviour.ExitPoint.position;
        //
        if (player.transform.position.y < 0)
            player.transform.position = new Vector3(player.transform.position.x, 1, player.transform.position.z);
        // TODO: Desfijar núcleo
        gigaWormBehaviour.wormCore.active = false;
        // Aqui indicaremos que el plyer ha sido cagado
        playerOut = true;
        gigaWormBehaviour.Active = true;
    }
}
