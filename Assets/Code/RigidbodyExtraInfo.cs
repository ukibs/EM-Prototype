using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyExtraInfo : MonoBehaviour
{
    //
    private Rigidbody rb;
    private Vector3 previousVelocity;

    // TODO: Poner booleano o del estilo para permitir daño en enemigos
    // Ahora no sufren daño por impactos generales para evitar que se maten solos
    // Permitirlo cuando ocurra por acción del jugador (ej: roca lanzada por él)

    //
    public Vector3 PreviousVelocity { get { return previousVelocity; } }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // We take it after any 
    void LateUpdate()
    {
        previousVelocity = rb.velocity;
    }
}
