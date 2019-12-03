using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CameraBehaviour, call SetCamera() if you want it to move to the next objective.
/// </summary>
public class CameraBehaviour : MonoBehaviour
{
    
    #region Private

    private int id = -1;
    private float t = 0.0f;
    private Transform currentBehaviour = null;
    [SerializeField]
    private float spd = 1f;
    private bool transitioning = false;
    private Transform beforeBehaviour = null;

    #endregion
    
    #region Public

    #endregion
    
    #region Properties

    public Transform CurrentBehaviour
    {
        get { return currentBehaviour; }
    }

    public int Id
    {
        get { return id; }
    }

    public float T
    {
        get { return t; }
    }

    public bool Transitioning
    {
        get { return transitioning; }
    }
    
    #endregion
    
    #region MonoBehaviour
    
    void Start()
    {
        currentBehaviour = transform;
    }

    
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        //Debug.Log(transitioning);
        if (transitioning)
        {
            Transition();
        }
        else
        {
            transform.position = currentBehaviour.position;
            transform.rotation = currentBehaviour.rotation;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Transitioning to the current transform
    /// </summary>
    private void Transition()
    {
        //Debug.Log(currentBehaviour);
        // Transition finished, return
        if (t >= 1f)
        {
            transitioning = false;
            return;
        }
        // Update transition
        float dt = Time.deltaTime;
        transform.position = Vector3.Lerp(beforeBehaviour.position, currentBehaviour.position, t);
        transform.rotation = Quaternion.Lerp(beforeBehaviour.rotation, currentBehaviour.rotation, t);
        t += spd * dt; 
    }
    
    /// <summary>
    /// Sets an objective for the camera.
    /// </summary>
    /// <param name="go"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    public bool SetCamera(GameObject go, float speed = 1f)
    {
        if (go.GetInstanceID() == id || transitioning) 
            return false;
        beforeBehaviour = currentBehaviour;
        currentBehaviour = go.transform;
        spd = speed;
        transitioning = true;
        id = go.GetInstanceID();
        t = 0.0f;
        return true;
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    public bool SetCamera(Transform go, float speed = 1f)
    {
        if (go.gameObject.GetInstanceID() == id || transitioning)
            return false;
        beforeBehaviour = currentBehaviour;
        currentBehaviour = go.transform;
        transitioning = true;
        spd = speed;
        id = go.gameObject.GetInstanceID();
        t = 0.0f;
        return true;
    }
    
    #endregion
}
