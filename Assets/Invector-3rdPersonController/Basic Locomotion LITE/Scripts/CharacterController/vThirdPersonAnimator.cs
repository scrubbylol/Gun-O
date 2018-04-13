using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Invector.CharacterController
{
    public abstract class vThirdPersonAnimator : vThirdPersonMotor
    {
        public virtual void UpdateAnimator()
        {
            if (animator == null || !animator.enabled) return;

            animator.SetBool("IsStrafing", isStrafing);
            animator.SetBool("IsGrounded", isGrounded);
			animator.SetBool("IsAiming", isAiming);
			animator.SetBool("IsShooting", isShooting);
			animator.SetBool("IsReloading", isReloading);
			animator.SetBool ("IsKnifing", isKnifing);
            animator.SetFloat("GroundDistance", groundDistance);

            if (!isGrounded)
                animator.SetFloat("VerticalVelocity", verticalVelocity);

            if (isStrafing)
            {
                // strafe movement get the input 1 or -1
                //animator.SetFloat("InputHorizontal", direction, 0.1f, Time.deltaTime);
            }

			// Change position and rotation of the rifle
			if (isAiming) {
				rifleObject.transform.localPosition = new Vector3 (0.2095602f, -0.00831561f, 0.01695112f);
				rifleObject.transform.localEulerAngles = new Vector3 (3.317f, 273.797f, -98.546f);

				if (!isReloading) {
					crosshairPrefab.GetComponent<Image> ().enabled = true;
				}
			} else {
				rifleObject.transform.localPosition = new Vector3 (0.24f, -0.014f, -0.039f);
				rifleObject.transform.localEulerAngles = new Vector3 (9.076f, 296.704f, -100.777f);
			}

			if (isShooting && !animator.GetBool("HasKnife")) {
				recoilTimer += Time.deltaTime;
				fireTimer += Time.deltaTime;
				if (fireTimer >= fireRate && ammoCurrent > 0) {
					//GameObject bullet = Instantiate (bulletPrefab, rifleTip.transform.position, rifleTip.transform.rotation);
					ammoCurrent -= 1;
					//audSource.PlayOneShot (rifleShotSound, 0.25f);
					UpdateUI ();

					if (ammoCurrent == 0) {
						reloadIndicator.GetComponent<Animation> ().Play ("Falloff_Back");
						reloadIndicator.GetComponent<Animation> ().PlayQueued ("Falloff_Back_Blink");
					}

					Quaternion muzzleRotation = Quaternion.LookRotation (-rifleTip.transform.right);
					GameObject muzzle = Instantiate (rifleMuzzleFx, rifleTip.transform.position, muzzleRotation, rifleTip.transform);
					Destroy (muzzle, 0.135f);

					float recoilSpread = Mathf.Floor (recoilTimer) * 8;
					int spreadX = Random.Range (-10, 10);
					int spreadY = Random.Range (-10, 10);

 					float fspreadX = (spreadX <= 0) ? spreadX - recoilSpread : spreadX + recoilSpread;
					float fspreadY = (spreadY <= 0) ? spreadY - recoilSpread : spreadY + recoilSpread;

					float x = Screen.width / 2f - 5f + fspreadX;
					float y = Screen.height / 2f + 5f + fspreadY;
					crosshairPrefab.transform.localScale *= 1.025f;

					var v = Camera.main.ScreenPointToRay (new Vector3 (x, y, 0));
					Debug.DrawRay (rifleTip.transform.position, v.direction * 10, Color.red);

					var ray = new Ray (rifleTip.transform.position, v.direction * 10);
					var hit = new RaycastHit ();

					if (Physics.Raycast (ray, out hit)) {
						if (hit.transform.tag.Equals ("Player")) {
							GameObject bloodFx = Instantiate (bloodFxPrefab, hit.point - (hit.point.normalized / 99f), Quaternion.LookRotation (hit.normal), GameObject.Find ("Fx").transform);

							//Vector3 thePos = Camera.main.WorldToScreenPoint (hit.point + (Vector3.up * 0.5f) + (Vector3.up * (GameObject.Find ("Damage_Fx").transform.childCount * 0.25f)));
							//GameObject dmg = Instantiate (damageText, thePos, damageText.transform.rotation, GameObject.Find("Damage_Fx").transform);
							//dmg.name = "Damage_Text";
							//Destroy (dmg, 2.0f);

							if (hit.collider.name.Equals (playerHead.transform.name)) {
								//dmg.GetComponent<Text> ().text = "-25";
								if (hit.transform.name.Equals ("Dummy")) {
									hit.transform.GetComponent<DummyTest> ().health -= 25;
								} else {
									hit.transform.GetComponent<vThirdPersonMotor> ().health -= 25;
								}
							} else {
								if (hit.transform.name.Equals ("Dummy")) {
									hit.transform.GetComponent<DummyTest> ().health -= 10;
								} else {
									hit.transform.GetComponent<vThirdPersonMotor> ().health -= 10;
								}
							}

							//TODO: UPDATE THE UI OF TARGET
							//TODO: MAKE TARGET CAN'T DO ANY ACTIONS

							if (hit.transform.name.Equals ("Dummy")) {
								if (hit.transform.GetComponent<DummyTest> ().health <= 0 && !hit.transform.GetComponent<Animator> ().GetBool ("IsDead")) {
									hit.transform.GetComponent<Animator> ().SetBool ("IsDead", true);
								}
							} else {
								if (hit.transform.GetComponent<vThirdPersonMotor> ().health <= 0 && !hit.transform.GetComponent<vThirdPersonMotor> ().animator.GetBool ("IsDead")) {
									hit.transform.GetComponent<vThirdPersonMotor> ().animator.SetBool ("IsDead", true);
								}
							}

							Destroy (bloodFx, 1.0f);
						} else {
							if (!hit.transform.name.Equals ("clip")) {
								GameObject bulletHole = Instantiate (bulletHolePrefab, hit.point - (hit.transform.forward / 99f), Quaternion.LookRotation (hit.normal), GameObject.Find ("Fx").transform);
								GameObject bulletHoleFx = Instantiate (bulletHoleFxPrefab, hit.point - (hit.transform.forward / 99f), Quaternion.LookRotation (hit.normal), GameObject.Find ("Fx").transform);
								Destroy (bulletHole, 30.0f);
								Destroy (bulletHoleFx, 2.0f);
							}
						}
					}

					//bullet.GetComponent<Rigidbody> ().velocity = bullet.transform.forward * bulletSpeed;
					//bullet.GetComponent<Rigidbody>().velocity = v.direction * bulletSpeed;

					//Destroy (bullet, 3.0f);

					fireTimer = 0;
				}
			} else {
				fireTimer = fireRate;

				if (animator.GetBool ("HasKnife")) {
					isShooting = false;

				}
			}

			if (isKnifing) {
				if (!animator.GetBool ("HasKnife")) {
					isKnifing = false;
				}
			}

			// Recoil deduction
			if (recoilTimer > 0 && !isShooting) {
				recoilTimer -= Time.deltaTime;
				recoilFixTime += Time.deltaTime;
				if (recoilFixTime > 0.05f && crosshairPrefab.transform.localScale.x > 1) {
					crosshairPrefab.transform.localScale -= Vector3.one * 0.03f;
					recoilFixTime = 0;
				}
			}

			if (isReloading) {
				reloadTimer += Time.deltaTime;

				// Drop rifle clip
				if (reloadTimer > 0.725f && !rifleClipDropped) {
					GameObject rifleClip = rifleObject.transform.GetChild (0).gameObject;
					//GameObject newRifleClip = Instantiate (rifleClip, playerLeftHand.transform.position, rifleClip.transform.rotation, playerLeftHand.transform);
					//newRifleClip.transform.SetAsFirstSibling ();

					rifleClip.GetComponent<Rigidbody> ().isKinematic = false;
					rifleClip.GetComponent<Rigidbody> ().velocity = -rifleClip.transform.forward * 2;
					rifleClip.transform.SetParent (GameObject.Find ("Fx").transform);
					rifleClipDropped = true;
				}

				if (reloadTimer > 1.25f && !newRifleClipGot) {
					GameObject newRifleClip = Instantiate (rifleClipObject, playerLeftHand.transform);
					newRifleClip.name = "clip";
					newRifleClip.transform.localPosition = new Vector3 (-0.081f, -0.042f, 0.046f);
					newRifleClip.transform.localEulerAngles = new Vector3 (200.65f, -110.6f, -73.58f);
					newRifleClipGot = true;
				}

				if (reloadTimer > 1.85f && !setNewRifleClip) {
					GameObject newRifleClip = playerLeftHand.transform.GetChild (5).gameObject;
					newRifleClip.transform.SetParent (rifleObject.transform);
					newRifleClip.transform.SetAsFirstSibling ();
					newRifleClip.transform.localPosition = rifleClipObject.transform.position;
					newRifleClip.transform.localEulerAngles = rifleClipObject.transform.eulerAngles;

					setNewRifleClip = true;
				}

				if (reloadTimer >= 3.0f) {
					isReloading = false;

					if (ammoCurrent == 0) {
						reloadIndicator.GetComponent<Animation> ().Play ("Falloff_Back_Out");
					}
					ammoCurrent = ammoCapacity;

					UpdateUI ();
					if (isAiming && !Input.GetKey(KeyCode.Mouse1)) {
						isAiming = false;
					}

					reloadTimer = 0;
					rifleClipDropped = false;
					newRifleClipGot = false;
					setNewRifleClip = false;
				}

			}

            // fre movement get the input 0 to 1
            animator.SetFloat("InputVertical", speed, 0.1f, Time.deltaTime);
        }

        public void OnAnimatorMove()
        {
            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (isGrounded)
            {
                transform.rotation = animator.rootRotation;

                var speedDir = Mathf.Abs(direction) + Mathf.Abs(speed);
                speedDir = Mathf.Clamp(speedDir, 0, 1);
                var strafeSpeed = (isSprinting ? 1.5f : 1f) * Mathf.Clamp(speedDir, 0f, 1f);
                
                // strafe extra speed
                if (isStrafing)
                {
					if (strafeSpeed <= 0.5f) {
						ControlSpeed (strafeWalkSpeed);
					} else if (strafeSpeed > 0.5f && strafeSpeed <= 1f) {
						ControlSpeed (strafeRunningSpeed);
					} else {
						ControlSpeed (strafeSprintSpeed);
					}
                }
                else if (!isStrafing)
                {
                    // free extra speed                
					if (speed <= 0.5f) {
						ControlSpeed (freeWalkSpeed);
					}
                    else if (speed > 0.5 && speed <= 1f)
                        ControlSpeed(freeRunningSpeed);
                    else
                        ControlSpeed(freeSprintSpeed);
                }
            }

			if (isAiming) {

			}
        }
			
		public virtual void UpdateUI() {
			Text ammoText = GameObject.Find ("Ammo").GetComponent<Text> ();
			ammoText.text = ammoCurrent.ToString() + " / " + ammoCapacity.ToString ();
			Text healthText = GameObject.Find ("Health").GetComponent<Text> ();
			healthText.text = health.ToString () + " %";
		}

		public virtual void ResetRifle() {
			if (playerLeftHand.transform.childCount == 6) {
				GameObject newRifleClip = playerLeftHand.transform.GetChild (5).gameObject;
				newRifleClip.transform.SetParent (rifleObject.transform);
				newRifleClip.transform.SetAsFirstSibling ();
				newRifleClip.transform.localPosition = rifleClipObject.transform.position;
				newRifleClip.transform.localEulerAngles = rifleClipObject.transform.eulerAngles;
			} else {
				if (!rifleObject.transform.GetChild (0).name.Equals ("clip")) {
					GameObject rifleClip = Instantiate (rifleClipObject);
					rifleClip.name = "clip";
					rifleClip.transform.SetParent (rifleObject.transform);
					rifleClip.transform.SetAsFirstSibling ();
					rifleClip.transform.localPosition = rifleClipObject.transform.position;
					rifleClip.transform.localEulerAngles = rifleClipObject.transform.eulerAngles;
				}
			}

			reloadTimer = 0;
			rifleClipDropped = false;
			newRifleClipGot = false;
			setNewRifleClip = false;
		}
    }
}