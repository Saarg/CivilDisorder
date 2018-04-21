using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]

public class ScoreMaker : MonoBehaviour {

	[SerializeField]
	LayerMask scoreMakerLayer = 1 << 9;

	void OnCollisionEnter(Collision col) {
		if (scoreMakerLayer == (scoreMakerLayer | (1 << col.gameObject.layer))) {
			col.gameObject.SendMessageUpwards("AddScore", col, SendMessageOptions.DontRequireReceiver);

			this.enabled = false;
		}
	}
}
