using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]

public class ScoreMaker : MonoBehaviour {

	[SerializeField] AudioClip impactClip;
	AudioSource source;

	[SerializeField]
	LayerMask scoreMakerLayer = 1 << 9;

	void Start() {
		source = GetComponent<AudioSource>();
		source.clip = impactClip;
	}

	void OnCollisionEnter(Collision col) {
		source.Play();
		source.pitch = Random.Range(0.1f, 1.9f);

		if (scoreMakerLayer == (scoreMakerLayer | (1 << col.gameObject.layer))) {
			col.gameObject.SendMessageUpwards("AddScore", col, SendMessageOptions.DontRequireReceiver);

			this.enabled = false;
		}
	}
}
