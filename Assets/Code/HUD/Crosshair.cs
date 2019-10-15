using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    #region UI

    #endregion

    #region Public Variables
    [Header("Variables")]
    public float foo;
    #endregion

    #region Private Variables
    RectTransform _interior;
    RectTransform _exterior;

    Image _interiorSprite;
    // Image _exteriorCW;
    // Image _exteriorCCW;
    #endregion

    #region Monobehaviour
    private void Awake()
    {
        // Get the hierarchy
        _interior = transform.GetChild(0).GetComponent<RectTransform>();
        _exterior = transform.GetChild(1).GetComponent<RectTransform>();

        // Store our sprites
        _interiorSprite = _interior.GetChild(0).GetComponent<Image>();
        
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    #endregion

    #region Methods

    #endregion
}
