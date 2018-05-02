using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buildings {
	[RequireComponent(typeof(Collider))]
	public class BuildingCollision : MonoBehaviour {

		[SerializeField] bool canBetriggered = false;
		[SerializeField] LayerMask triggetLager;
		List<Rigidbody> rigidbodies;
		List<Collider> colliders;

		List<Vector3> childPositions;
		List<Quaternion> childRotations;

		// Use this for initialization
		void Start () {
			rigidbodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());

			colliders = new List<Collider>(GetComponents<Collider>());

			childPositions = new List<Vector3>(transform.childCount);
			childRotations = new List<Quaternion>(transform.childCount);

			foreach (Transform child in transform) {
				childPositions.Add(child.localPosition);
				childRotations.Add(child.localRotation);
			}

			GameManager.onStartCountDown += Reset;
		}

		void Reset () {
			StartCoroutine(CReset());
		}

		IEnumerator CReset() {
			foreach(Collider c in colliders) {
				c.enabled = true;
			}

			for (int i = 0; i < transform.childCount; i++) {
				rigidbodies[i].isKinematic = true;
				
				transform.GetChild(i).localPosition = childPositions[i];
				transform.GetChild(i).localRotation = childRotations[i];
				yield return null;
			}
		}

		void OnTriggerEnter(Collider col) {
			if (GameManager.Instance != null && GameManager.Instance.gameState == GameManager.GameStates.Game && triggetLager == (triggetLager | (1 << col.gameObject.layer))) {
				foreach(Collider c in colliders) {
					c.enabled = false;
				}

				foreach(Rigidbody rb in rigidbodies) {
					if (rb == null)
						continue;

					rb.isKinematic = false;
				}

				col.gameObject.SendMessageUpwards("AddScore", 5000f, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
