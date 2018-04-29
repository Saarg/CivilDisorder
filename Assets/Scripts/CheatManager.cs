using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class CheatManager : MonoBehaviour {

	[SerializeField] Camera mainCamera;
	bool freeCamera = false;
	[SerializeField] GameObject playerUI_GO;
	[SerializeField] GameObject graphy;
	
	void Update () {
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt)) {
			if (Input.GetKeyDown(KeyCode.N) && GameManager.Instance != null) {
				GameManager.Instance.NextState();
			}

			if (Input.GetKeyDown(KeyCode.F) && mainCamera != null) {
				freeCamera = !freeCamera;

				if (playerUI_GO != null)
					playerUI_GO.SetActive(!freeCamera);
				
				mainCamera.GetComponent<CameraFollow>().enabled = !freeCamera;
				mainCamera.GetComponent<FreeCamera>().enabled = freeCamera;
			}
		}
	}
}
