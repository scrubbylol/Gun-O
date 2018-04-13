using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Invector.CharacterController
{
    public class vThirdPersonController : vThirdPersonAnimator
    {
        protected virtual void Start()
        {
			#if !UNITY_EDITOR
	            Cursor.visible = false;
			#endif

			Text ammoText = GameObject.Find ("Ammo").GetComponent<Text> ();
			ammoText.text = ammoCapacity.ToString() + " / " + ammoCapacity.ToString ();
        }

        public virtual void Sprint(bool value)
        {                                   
			isSprinting = value;
        }

		public virtual void Knife()
        {
            if (locomotionType == LocomotionType.OnlyFree) return;
            isStrafing = !isStrafing;

			if (isStrafing) {
				rifleObject.SetActive (false);
				rifleBack.SetActive (true);
				knifeObject.SetActive (true);
				knifeBack.SetActive (false);

				animator.SetBool ("HasKnife", true);
				weaponName.text = "Knife";
				audSource.PlayOneShot (knifeDeploySound, 0.25f);

				if (isReloading) {
					isReloading = false;
					isAiming = false;
					ResetRifle ();
				}

				if (isAiming) {
					isAiming = false;
				}
			} else {
				rifleObject.SetActive (true);
				rifleBack.SetActive (false);
				knifeObject.SetActive (false);
				knifeBack.SetActive (true);

				animator.SetBool ("HasKnife", false);
				weaponName.text = "Rifle";
				audSource.PlayOneShot (rifleDeploySound, 0.25f);
			}
        }

        public virtual void Jump()
        {
            // conditions to do this action
            bool jumpConditions = isGrounded && !isJumping;
            // return if jumpCondigions is false
            if (!jumpConditions) return;
            // trigger jump behaviour
            jumpCounter = jumpTimer;            
            isJumping = true;
            // trigger jump animations            
            if (_rigidbody.velocity.magnitude < 1)
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            else
                animator.CrossFadeInFixedTime("JumpMove", 0.2f);
        }

		public virtual void Aim(bool value) {
			if (!animator.GetBool ("HasKnife")) {
				isAiming = value;

				if (!isReloading) {
					crosshairPrefab.GetComponent<Image> ().enabled = true;
				}

				if (!isAiming) {
					isShooting = false;
					crosshairPrefab.GetComponent<Image> ().enabled = false;
				}

				if (isSprinting) {
					isSprinting = false;
				}
			}
		}

		public virtual void Shoot(bool value) {
			if (!animator.GetBool ("HasKnife")) {
				if (isAiming) {
					isShooting = value;
				}

				if (isReloading) {
					isShooting = false;
				}
			} else {
				isKnifing = value;
			}
		}

		public virtual void Reload(bool value) {
			if (!isReloading && !animator.GetBool ("HasKnife")) {
				isReloading = value;
				crosshairPrefab.GetComponent<Image> ().enabled = false;
			}
		}

		public virtual void Score(bool value) {
			if (value) {
				scoreBoard.SetActive (true);
			} else {
				scoreBoard.SetActive (false);
			}
		}

        public virtual void RotateWithAnotherTransform(Transform referenceTransform)
        {
            var newRotation = new Vector3(transform.eulerAngles.x, referenceTransform.eulerAngles.y, transform.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newRotation), strafeRotationSpeed * Time.fixedDeltaTime);
            targetRotation = transform.rotation;
        }

		public virtual void InstantRotateWithAnotherTransform(Transform referenceTransform)
		{
			var newRotation = new Vector3(transform.eulerAngles.x, referenceTransform.eulerAngles.y, transform.eulerAngles.z);
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newRotation), 99);
			targetRotation = transform.rotation;
		}
    }
}