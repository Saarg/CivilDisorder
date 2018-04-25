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

		// Use this for initialization
		void Start () {
			rigidbodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());

			colliders = new List<Collider>(GetComponents<Collider>());
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
