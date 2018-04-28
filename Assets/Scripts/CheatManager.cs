using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatManager : MonoBehaviour {

	[SerializeField] GameObject graphy;
	
	void Update () {
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt)) {
			if (Input.GetKeyDown(KeyCode.N)) {
				GameManager.Instance.NextState();
			}
		}
	}
}
