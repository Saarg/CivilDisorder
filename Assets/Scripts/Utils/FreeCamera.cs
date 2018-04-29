using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils {
	public class FreeCamera : MonoBehaviour {
	
		public float cameraSensitivity = 90;
		public float climbSpeed = 4;
		public float normalMoveSpeed = 10;
		public float slowMoveFactor = 0.25f;
		public float fastMoveFactor = 3;
	
		private float rotationX = 0.0f;
		private float rotationY = 0.0f;
	
		void Update ()
		{
			Vector3 newPos = transform.position;
			Quaternion newRot = transform.localRotation;

			rotationX += MultiOSControls.GetValue("CameraRotationX", PlayerNumber.FreeCamera) * cameraSensitivity * Time.deltaTime;
			rotationY += MultiOSControls.GetValue("CameraRotationY", PlayerNumber.FreeCamera) * cameraSensitivity * Time.deltaTime;
			rotationY = Mathf.Clamp (rotationY, -90, 90);
	
			newRot = Quaternion.AngleAxis(rotationX, Vector3.up);
			newRot *= Quaternion.AngleAxis(rotationY, Vector3.left);
	
			if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
			{
				newPos += transform.forward * (normalMoveSpeed * fastMoveFactor) * MultiOSControls.GetValue("VerticalFreeCam", PlayerNumber.FreeCamera) * Time.deltaTime;
				newPos += transform.right * (normalMoveSpeed * fastMoveFactor) * MultiOSControls.GetValue("HorizontalFreeCam", PlayerNumber.FreeCamera) * Time.deltaTime;
			}
			else if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl))
			{
				newPos += transform.forward * (normalMoveSpeed * slowMoveFactor) * MultiOSControls.GetValue("VerticalFreeCam", PlayerNumber.FreeCamera) * Time.deltaTime;
				newPos += transform.right * (normalMoveSpeed * slowMoveFactor) * MultiOSControls.GetValue("HorizontalFreeCam", PlayerNumber.FreeCamera) * Time.deltaTime;
			}
			else
			{
				newPos += transform.forward * normalMoveSpeed * MultiOSControls.GetValue("VerticalFreeCam", PlayerNumber.FreeCamera) * Time.deltaTime;
				newPos += transform.right * normalMoveSpeed * MultiOSControls.GetValue("HorizontalFreeCam", PlayerNumber.FreeCamera) * Time.deltaTime;
			}
	
	
			if (MultiOSControls.GetValue("Up", PlayerNumber.FreeCamera) != 0) {newPos += transform.up * climbSpeed * Time.deltaTime;}
			if (MultiOSControls.GetValue("Down", PlayerNumber.FreeCamera) != 0) {newPos -= transform.up * climbSpeed * Time.deltaTime;}
	
			transform.position = Vector3.Lerp(transform.position, newPos, 0.9f);
			transform.localRotation = Quaternion.Lerp(transform.localRotation, newRot, Time.deltaTime);

			if (MultiOSControls.GetValue("HideCursor", PlayerNumber.FreeCamera) != 0)
			{
				Screen.lockCursor = (Screen.lockCursor == false) ? true : false;
			}
		}
	}
}