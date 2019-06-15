using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyedPiece : MonoBehaviour
{
    //
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    // Start is called before the first frame update
    void Start()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
    }

    public void Restore()
    {
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        //gameObject.SetActive(false);
    }
}
