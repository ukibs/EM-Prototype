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

         
        void Start()
        {
            Assert.IsNotNull(cb, "Please, set the CameraBehaviour before Start() of CameraSM.");
            Assert.IsNotNull(cameraMoving, "Please, set the moving camera.");
            Assert.IsNotNull(cameraShooting, "Please, set the shooting camera.");
            sc = GetComponent<SpringCamera>();
        }
    
        void Update()
        {

            hittingShoot = Input.GetMouseButton(0);
            if (hittingShoot)
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
                    //(GABI): make the springCamera use only the ui when shooting, when not, move and Ui. 
                    if (cb.Transitioning == false && sc.useOnlyUI) sc.useOnlyUI = false;
                    break;
                case State.Aiming:
                    if (!sc.useOnlyUI) sc.useOnlyUI = true;
                    cb.SetCamera(cameraShooting, 1.5f);
                    break;
                default:
                    Assert.IsTrue(false, "Unknown state.");
                    return false;
            }

            return true;
        }

        IEnumerator ChangeStateTest()
        {
            yield return new WaitForSeconds(3f);
            if (state == State.Aiming) state = State.Move;
            else state = State.Aiming;
            StartCoroutine(ChangeStateTest());
        }
        
        #endregion

        #region Properties

        

        #endregion

        
       
    }   

}
