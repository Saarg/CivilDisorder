using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour {

	[SerializeField] LapTimer lapTimer;

	// Use this for initialization
	void Start () {
		if (lapTimer == null)
			lapTimer = transform.parent.GetComponent<LapTimer>();
	}
	
	void OnTriggerEnter(Collider other)
	{
		lapTimer.OnCheckPointCrossed(gameObject, other);
	}
}
