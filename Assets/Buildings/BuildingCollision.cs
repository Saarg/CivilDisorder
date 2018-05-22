using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buildings {
	public class BuildingCollision : MonoBehaviour {
		static List<BuildingCollision> buildings = new List<BuildingCollision>();
		public static List<BuildingCollision> Buildings { get { return buildings; }}

		[SerializeField] LayerMask triggerLayer;
		List<Rigidbody> rigidbodies;
		public List<Rigidbody> Rigidbodies { get { return rigidbodies; }}
		public List<Vector3> targetPos;
		public List<Quaternion> targetRot;
		
		List<Collider> colliders;

		List<Vector3> childPositions;
		List<Quaternion> childRotations;

		bool triggered = false;
		public bool Triggered { get { return triggered; } }

		// Use this for initialization
		void Start () {
			rigidbodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());

			colliders = new List<Collider>(GetComponents<Collider>());
			if (colliders.Count == 0) {
				triggered = true;
			}

			childPositions = new List<Vector3>(transform.childCount);
			childRotations = new List<Quaternion>(transform.childCount);

			foreach (Transform child in transform) {
				childPositions.Add(child.localPosition);
				childRotations.Add(child.localRotation);
				targetPos.Add(child.position);
				targetRot.Add(child.rotation);
			}

			GameManager.onStartCountDown += Reset;

			buildings.Add(this);
		}

		void OnDestroy() {
			GameManager.onStartCountDown -= Reset;
			buildings.Remove(this);			
		}

		void Reset () {
			if (colliders.Count == 0) {
				triggered = true;
			} else {
				triggered = false;
			}
			StartCoroutine(CReset());
		}

		// void FixedUpdate()
		// {
		// 	if (!triggered)
		// 		return;

		// 	int i = 0;
		// 	foreach (Transform child in transform) {
		// 		if (GameManager.Instance.isServer || targetPos[i] == Vector3.zero)
		// 			continue;
				
		// 		child.position = Vector3.Lerp(child.position, targetPos[i], 0.01f);
		// 		child.rotation = Quaternion.Lerp(child.rotation, targetRot[i], 0.01f);

		// 		i++;
		// 	}
		// }

		IEnumerator CReset() {
			foreach(Collider c in colliders) {
				c.enabled = true;
			}

			for (int i = 0; i < transform.childCount; i++) {
				if (colliders.Count == 0) {
					rigidbodies[i].isKinematic = false;
				} else {
					rigidbodies[i].isKinematic = true;
				}
				
				transform.GetChild(i).localPosition = childPositions[i];
				transform.GetChild(i).localRotation = childRotations[i];
				yield return null;
			}
		}

		void OnTriggerEnter(Collider col) {
			if (GameManager.Instance != null && GameManager.Instance.gameState == GameManager.GameStates.Game && triggerLayer == (triggerLayer | (1 << col.gameObject.layer))) {
				triggered = true;
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
