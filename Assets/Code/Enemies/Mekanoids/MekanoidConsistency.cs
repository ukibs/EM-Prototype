using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MekanoidConsistency : EnemyConsistency
{
    #region Public Attributes

    public GameObject face;
    public Material deadFaceMaterial;
    public GameObject explosionPrefab;
    public GameObject smokePrefab;
    public AudioClip explosionClip;

    #endregion

    #region Methods

    protected override void ManageDamage(float damageReceived, Vector3 point)
    {
        base.ManageDamage(damageReceived, point);
        //
        // Cambio de cara
        face.GetComponent<MeshRenderer>().material = deadFaceMaterial;
        //
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(explosion, 10);
        //
        //GameObject smoke = 
        Instantiate(smokePrefab, transform);
        //
        if (audioSource != null && explosionClip != null)
        {
            audioSource.clip = explosionClip;
            audioSource.Play();
        }
    }

    #endregion
}
