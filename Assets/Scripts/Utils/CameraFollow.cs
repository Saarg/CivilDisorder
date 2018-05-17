using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils {
	public class CameraFollow : MonoBehaviour {
		[SerializeField] bool follow = false;
		[SerializeField] Transform target;
		[SerializeField] Vector3 offset;
		[Range(0, 10)]
		[SerializeField] float lerpPositionMultiplier = 1f;
		[Range(0, 10)]		
		[SerializeField] float lerpRotationMultiplier = 1f;

		Vector3 startPos;
		Quaternion startRot;

		Rigidbody rb;

		void Start () {
			Player.onPlayerSpawn += OnPlayerSpawn;

			GameManager.onStartGame += OnStartGame;
			GameManager.onEndGame += OnEndGame;

			startPos = transform.position;
			startRot = transform.rotation;

			rb = GetComponent<Rigidbody>();
		}

		void OnDestroy() {
			GameManager.onStartGame -= OnStartGame;
			GameManager.onEndGame -= OnEndGame;
		}

		void OnPlayerSpawn(Player p) {
			if (p.hasAuthority) {
				target = p.transform;
			}
		}

		void OnStartGame() {
			follow = true;

			rb.isKinematic = false;
		}

		void OnEndGame() {
			follow = false;

			transform.position = startPos;	
			transform.rotation = startRot;

			rb.isKinematic = true;
		}

		void FixedUpdate() {
			if (!follow) return;

			this.rb.velocity.Normalize();

			Quaternion curRot = transform.rotation;

			Rigidbody rb = target.GetComponent<Rigidbody>();
			if (rb == null)
				transform.LookAt(target);
			else {
				transform.LookAt(target.position/* + target.forward * rb.velocity.sqrMagnitude*/);				
			}
			
			Vector3 tPos = target.position + target.TransformDirection(offset);
			if (tPos.y < target.position.y) {
				tPos.y = target.position.y;
			}

			transform.position = Vector3.Lerp(transform.position, tPos, Time.fixedDeltaTime * lerpPositionMultiplier);
			transform.rotation = Quaternion.Lerp(curRot, transform.rotation, Time.fixedDeltaTime * lerpRotationMultiplier);
		}
	}
}
