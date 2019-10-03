using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace CameraManager
{

    /// <summary>
    /// MOVE = 0, SHOOTING = 1
    /// </summary>
    public enum State
    {
        Move = 0,
        Aiming
    }
    
    public class CameraSM : MonoBehaviour
    {

        #region Public
        public CameraBehaviour cb;
        #endregion

        #region Private

        [SerializeField]
        private Transform cameraMoving;
        [SerializeField]
        private Transform cameraShooting;
        private State state;
        private bool hittingShoot = false;
        private SpringCamera sc;
        [SerializeField]
        private CameraAiming ca;

        [SerializeField] private InputManager im;
        private bool sprinting = false;
        #endregion

        #region Properties

        public State State
        {
            get { return state; }
            set { state = value; }
        }
        
        public Transform CameraMoving
        {
            get { return cameraMoving; }
            set { cameraMoving = value; }
        }

        public Transform CameraShooting
        {
            get { return cameraShooting; }
            set { cameraShooting = value; }
        }

        /// <summary>
        /// Set this in Awake() before Start of this component is executed
        /// </summary>
        public CameraBehaviour CBehaviour
        {
            get { return cb;  }
            set { cb = value;  }
        }

        #endregion
        
        #region MonoBehaviour

         
        private void Start()
        {
            CheckEverythingIsRight();
        }
    
        private void Update()
        {
            sprinting = im.SprintButton;
            hittingShoot = im.FireButton;
            if (hittingShoot && !sprinting)
            {
                state = State.Aiming;
            }
            else
            {
                state = State.Move;
            }
            Assert.IsTrue(ProcessStateMachineUpdate(),
                "Error in state machine of CameraSM.cs, see more logs to see what is wrong.");
        }

        #endregion
        
        #region Methods

        private bool ProcessStateMachineUpdate()
        {
            if (!cb)
            {
                return false;
            }
            switch (state)
            {
                case State.Move:
                    cb.SetCamera(cameraMoving, 1.0f);
                    break;
                case State.Aiming:
                    cb.SetCamera(cameraShooting, 1.5f);
                    break;
                default:
                    Assert.IsTrue(false, "Unknown state.");
                    return false;
            }

            return true;
        }

        private void CheckEverythingIsRight()
        {
            Assert.IsNotNull(cb, "Please, set the CameraBehaviour before Start() of CameraSM.");
            Assert.IsNotNull(cameraMoving, "Please, set the moving camera.");
            Assert.IsNotNull(cameraShooting, "Please, set the shooting camera.");
            // Force at least that the camera works...
            cb = cb ? cb : FindObjectOfType<CameraBehaviour>();
            cameraMoving = cameraMoving ? cameraMoving : FindObjectOfType<SpringCamera>().transform;
            cameraShooting = cameraShooting ? cameraShooting : FindObjectOfType<CameraAiming>().transform;
            im = FindObjectOfType<InputManager>();
            Assert.IsNotNull(im, "Fatal error, non existing InputManager.");
        }
        
        #endregion

        #region Properties

        

        #endregion

        
       
    }   

}
