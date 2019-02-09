using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAndStatsItem : MonoBehaviour
{
    public Material highlightMaterial;


    protected MapAndStatsManager mapAndStatsManager;
    protected Material initialMaterial;
    protected MeshRenderer meshRenderer;

    protected bool highLighted = false;
    protected GUISkin guiSkin;

    #region Unity Methods

    protected  virtual void Start()
    {
        mapAndStatsManager = FindObjectOfType<MapAndStatsManager>();
        meshRenderer = GetComponent<MeshRenderer>();
        initialMaterial = meshRenderer.material;
        guiSkin = Resources.Load<GUISkin>("GUI/Provisional Skin");
    }

    protected void OnMouseDown()
    {
        //
        if (!mapAndStatsManager.PlayerLocked)
        {
            ButtonFunction();
        }        
    }

    protected void OnMouseEnter()
    {
        //
        if (!mapAndStatsManager.PlayerLocked)
        {
            HighlightButton();
        }
    }

    private void OnMouseExit()
    {
        //
        if (!mapAndStatsManager.PlayerLocked)
        {
            UnHighlightButton();
        }
    }

    #endregion

    #region Methods

    public virtual void ButtonFunction() { }

    public virtual void HighlightButton() { meshRenderer.material = highlightMaterial; }

    public virtual void UnHighlightButton() { meshRenderer.material = initialMaterial;  }

    #endregion
}
