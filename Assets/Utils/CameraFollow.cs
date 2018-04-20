using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils {
	public class CameraFollow : MonoBehaviour {

		[SerializeField] Transform target;
		[SerializeField] Vector3 offset;
		[Range(0, 10)]
		[SerializeField] float lerpPositionMultiplier = 1f;
		[Range(0, 10)]		
		[SerializeField] float lerpRotationMultiplier = 1f;

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		void LateUpdate() {
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
