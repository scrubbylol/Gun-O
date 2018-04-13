using UnityEngine;
using UnityEngine.UI;


#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace Invector.CharacterController
{
    public class vThirdPersonInput : MonoBehaviour
    {
        #region variables

        [Header("Default Inputs")]
        public string horizontalInput = "Horizontal";
        public string verticallInput = "Vertical";
        public KeyCode jumpInput = KeyCode.Space;
        public KeyCode sprintInput = KeyCode.LeftShift;
		public KeyCode aimInput = KeyCode.Mouse1;
		public KeyCode shootInput = KeyCode.Mouse0;
		public KeyCode reloadInput = KeyCode.R;
		public KeyCode scoreInput = KeyCode.Tab;
		public KeyCode knifeInput = KeyCode.Alpha3;

        [Header("Camera Settings")]
        public string rotateCameraXInput ="Mouse X";
        public string rotateCameraYInput = "Mouse Y";

		[HideInInspector]
        public vThirdPersonCamera tpCamera;                // access camera info        
        [HideInInspector]
        public string customCameraState;                    // generic string to change the CameraState        
        [HideInInspector]
        public string customlookAtPoint;                    // generic string to change the CameraPoint of the Fixed Point Mode        
        [HideInInspector]
        public bool changeCameraState;                      // generic bool to change the CameraState        
        [HideInInspector]
        public bool smoothCameraState;                      // generic bool to know if the state will change with or without lerp  
        [HideInInspector]
        public bool keepDirection;                          // keep the current direction in case you change the cameraState

        protected vThirdPersonController cc;                // access the ThirdPersonController component                

		public GameObject playerTorso;
		public GameObject shootTarget;

		public float playerRotationTimer;

        #endregion

        protected virtual void Start()
        {
			CharacterInit ();
        }

        protected virtual void CharacterInit()
        {
            cc = GetComponent<vThirdPersonController>();
            if (cc != null)
                cc.Init();

			tpCamera = FindObjectOfType<vThirdPersonCamera> ();
			if (tpCamera)
				tpCamera.SetMainTarget (this.transform);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        protected virtual void LateUpdate()
        {
            if (cc == null) return;             // returns if didn't find the controller		    
            InputHandle();                      // update input methods
            UpdateCameraStates();               // update camera states
        }

        protected virtual void FixedUpdate()
        {
            cc.AirControl();
            CameraInput();
			AimInput ();
        }

        protected virtual void Update()
        {
            cc.UpdateMotor();                   // call ThirdPersonMotor methods               
            cc.UpdateAnimator();                // call ThirdPersonAnimator methods		 
        }

        protected virtual void InputHandle()
        {
            ExitGameInput();
            CameraInput();

            if (!cc.lockMovement)
            {
                MoveCharacter();
                SprintInput();
				AimInput ();
				ShootInput ();
				ReloadInput ();
				KnifeInput();
				ScoreInput ();
                JumpInput();
            }
        }

        #region Basic Locomotion Inputs      

        protected virtual void MoveCharacter()
        {            
            cc.input.x = Input.GetAxis(horizontalInput);
            cc.input.y = Input.GetAxis(verticallInput);
        }

        protected virtual void KnifeInput()
        {
			if (cc.animator.GetBool ("HasKnife")) {
				knifeInput = KeyCode.Alpha1;
			} else {
				knifeInput = KeyCode.Alpha3;
			}

			if (Input.GetKeyDown (knifeInput)) {
				cc.Knife ();

				tpCamera.defaultDistance = 1.5f;
				cc.crosshairPrefab.GetComponent<Image> ().enabled = false;
			}
        }

        protected virtual void SprintInput()
        {
			if (Input.GetKey(sprintInput) && (!cc.isAiming || (cc.isAiming && cc.isReloading)))
                cc.Sprint(true);
            else if(Input.GetKeyUp(sprintInput))
                cc.Sprint(false);
        }

		protected virtual void AimInput()
		{
			float targetX = (shootTarget.transform.position.x > 0) ? -12 : -12;
			float targetY = (shootTarget.transform.position.y > 0) ? 35 : 35;

			if (!cc.animator.GetBool ("HasKnife")) {
				if (Input.GetKey (aimInput) && !cc.isReloading) {
					tpCamera.defaultDistance = 0.85f;
					cc.Aim (true);

					playerRotationTimer += Time.deltaTime;

					var v3LookPoint = new Vector3 ();
					//var ray = Camera.main.ScreenPointToRay (new Vector3 (0.5f, 0.5f, 0));
					var ray = new Ray (GameObject.Find ("rifle_tip").transform.position, shootTarget.transform.TransformDirection (Vector3.forward));
					var hit = new RaycastHit ();

					//v3LookPoint = new Vector3 (v3LookPoint.x + targetXP, v3LookPoint.y + targetYP, v3LookPoint.z + targetZP);
					//playerTorso.transform.LookAt (v3LookPoint);

					//Debug.Log (shootTarget.transform.position);

					if (Physics.Raycast (ray, out hit)) {
						v3LookPoint = hit.point;
					}

					//v3LookPoint = new Vector3 (shootTarget.transform.position.x + targetXP, shootTarget.transform.position.y + targetYP, shootTarget.transform.position.z);
					//playerTorso.transform.LookAt (v3LookPoint);

					var newRotation = new Vector3 (shootTarget.transform.eulerAngles.x + targetX, shootTarget.transform.eulerAngles.y + targetY, shootTarget.transform.eulerAngles.z);
					playerTorso.transform.rotation = Quaternion.Lerp (playerTorso.transform.rotation, Quaternion.Euler (newRotation), cc.freeRotationSpeed * playerRotationTimer / 5);
				} else if (Input.GetKeyUp (aimInput) && !cc.isReloading) {
					tpCamera.defaultDistance = 1.5f;
					cc.Aim (false);
					cc.InstantRotateWithAnotherTransform (tpCamera.transform);
					playerRotationTimer = 0;
				}
			}
		}

		protected virtual void ShootInput()
		{
			if (!cc.animator.GetBool ("HasKnife")) {
				if (Input.GetKeyDown (shootInput) && cc.ammoCurrent > 0 && !cc.isReloading)
					cc.Shoot (true);
				else if (Input.GetKey (shootInput) && cc.ammoCurrent == 0 && !cc.isReloading)
					cc.Shoot (false);
				else if (Input.GetKeyUp (shootInput))
					cc.Shoot (false);
			} else {
				if (Input.GetKeyDown (shootInput)) {
					cc.Shoot (true);
				} else if (Input.GetKeyUp (shootInput)) {
					cc.Shoot (false);
				}
			}
		}

		protected virtual void ReloadInput()
		{
			if (Input.GetKeyDown (reloadInput) && cc.ammoCurrent != cc.ammoCapacity) {
				tpCamera.defaultDistance = 1.5f;
				cc.Aim (true);
				cc.Reload (true);
				cc.Shoot (false);
			}
		}

        protected virtual void JumpInput()
        {
            if (Input.GetKeyDown(jumpInput))
                cc.Jump();
        }

		protected virtual void ScoreInput()
		{
			if (Input.GetKey (scoreInput))
				cc.Score (true);
			else if (Input.GetKeyUp (scoreInput))
				cc.Score (false);
		}

        protected virtual void ExitGameInput()
        {
            // just a example to quit the application 
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!Cursor.visible)
                    Cursor.visible = true;
                else
                    Application.Quit();
            }
        }

        #endregion

        #region Camera Methods

        protected virtual void CameraInput()
        {
            if (tpCamera == null)
                return;
            var Y = Input.GetAxis(rotateCameraYInput);
            var X = Input.GetAxis(rotateCameraXInput);

            tpCamera.RotateCamera(X, Y);

            // tranform Character direction from camera if not KeepDirection
            if (!keepDirection) 
                cc.UpdateTargetDirection(tpCamera != null ? tpCamera.transform : null);
            
			// rotate the character with the camera while strafing        
            //RotateWithCamera(tpCamera != null ? tpCamera.transform : null);            
        }

        protected virtual void UpdateCameraStates()
        {
            // CAMERA STATE - you can change the CameraState here, the bool means if you want lerp of not, make sure to use the same CameraState String that you named on TPCameraListData
            if (tpCamera == null)
            {
                tpCamera = FindObjectOfType<vThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }            
        }

        protected virtual void RotateWithCamera(Transform cameraTransform)
        {
            if (cc.isStrafing && !cc.lockMovement && !cc.lockMovement)
            {                
				cc.RotateWithAnotherTransform(cameraTransform);                
            }
        }

        #endregion     
    }
}