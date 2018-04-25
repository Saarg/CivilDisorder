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

		void Start () {
			Player.onPlayerSpawn += OnPlayerSpawn;

			GameManager.onStartGame += OnStartGame;
			GameManager.onEndGame += OnEndGame;
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
		}

		void OnEndGame() {
			follow = false;			
		}

		void LateUpdate() {
			if (!follow) return;

			Quaternion curRot = transform.rotation;

			Rigidbody rb = target.GetComponent<Rigidbody>();
			if (rb == null)
				transform.LookAt(target);
			else {
				transform.LookAt(target.position + target.forward * rb.velocity.sqrMagnitude);				
			}

			transform.position = Vector3.Lerp(transform.position, target.position + target.TransformDirection(offset), Time.deltaTime * lerpPositionMultiplier);
			transform.rotation = Quaternion.Lerp(curRot, transform.rotation, Time.deltaTime * lerpRotationMultiplier);
		}
	}
}
