using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buildings {
	[RequireComponent(typeof(Collider))]
	public class BuildingCollision : MonoBehaviour {

		[SerializeField] LayerMask triggetLager;
		List<Rigidbody> rigidbodies;
		List<Collider> colliders;

		// Use this for initialization
		void Start () {
			int childCount = transform.childCount;
			rigidbodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());

			colliders = new List<Collider>(GetComponents<Collider>());
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		void OnTriggerEnter(Collider col) {
			if (triggetLager == (triggetLager | (1 << col.gameObject.layer))) {
				foreach(Collider c in colliders) {
					c.enabled = false;
				}

				foreach(Rigidbody rb in rigidbodies) {
					if (rb == null)
						continue;

					rb.isKinematic = false;
				}
			}
		}
	}
}
