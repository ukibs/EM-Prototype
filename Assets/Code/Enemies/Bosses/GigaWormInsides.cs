using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GigaWormInsides : MonoBehaviour
{
    //
    private GigaWormBehaviour gigaWormBehaviour;
    private RobotControl player;

    // Start is called before the first frame update
    void Start()
    {
        gigaWormBehaviour = FindObjectOfType<GigaWormBehaviour>();
        player = FindObjectOfType<RobotControl>();
    }

    // Aqui trabajaremos la salida del ojete
    private void OnTriggerEnter(Collider other)
    {
        //
        RobotControl possiblePlayer = other.GetComponent<RobotControl>();
        if(possiblePlayer != null)
            ShitPlayer();
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
        gigaWormBehaviour.PlayerOut = true;
        gigaWormBehaviour.Active = true;
    }
}
