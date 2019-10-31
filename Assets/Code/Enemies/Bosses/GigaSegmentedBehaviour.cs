using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GigaSegmentedBehaviour : BossBaseBehaviour
{
    #region Public Attributes

    public float startSpeed = 30;

    #endregion

    #region Private Attributes

    private GameObject previousSegment;
    private GameObject posteriorSegment;

    private BodyPart bodyPartBehaviour;

    #endregion

    #region Properties

    public bool IsActiveHead{ get { return previousSegment == null; } }

    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        //
        GetPreviousAndPosteriorSegements();
        //
        base.Start();
        //
        currentSpeed = startSpeed;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        //
        float dt = Time.deltaTime;
        //
        if (IsActiveHead)
        {
            transform.Translate(Vector3.forward * currentSpeed * dt);
            transform.Rotate(Vector3.up * 1 * dt);
        }
    }

    #region Methods

    private void GetPreviousAndPosteriorSegements()
    {
        int index = transform.GetSiblingIndex();
        //
        if (index == 0)
        {
            // En este caso es la cabeza activa
        }
        else
        {
            previousSegment = transform.parent.GetChild(index - 1).gameObject;
            bodyPartBehaviour = gameObject.AddComponent<BodyPart>();
            bodyPartBehaviour.previousBodyPart = previousSegment.transform;
            bodyPartBehaviour.bossBehaviour = this;
        }
            
        //
        if (index == transform.parent.childCount - 1)
        {
            // En este caso es la cola, desactivamos el weakpoint de conexion
            transform.GetChild(3).gameObject.SetActive(false);
        }
        else
            posteriorSegment = transform.parent.GetChild(index + 1).gameObject;
    }

    public override void LoseWeakPoint(string tag = "") { }

    public override void RespondToDamagedWeakPoint(string tag = "") { }

    public override void ImpactWithTerrain(bool hardEnough) { }

    #endregion
}
