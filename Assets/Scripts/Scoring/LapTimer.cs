using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerLap {
	public Player player;
	public float startTime;
	public int lastCP_Index;
}

[Serializable]
public class PlayerTime {
	public string playerName;
	public float time;
}

public class LapTimer : MonoBehaviour {

	[SerializeField] List<PlayerLap> playersLap = new List<PlayerLap>();
	[SerializeField] List<GameObject> checkPoints;

	public void OnCheckPointCrossed(GameObject cp, Collider other) {
		int i = checkPoints.IndexOf(cp);

		// Find the lap data
		PlayerLap pl = playersLap.Find((playerLap) => {
			if (other.transform.parent == null || other.transform.parent.parent == null)
				return false;
				
			return playerLap.player != null && playerLap.player.gameObject == other.transform.parent.parent.gameObject;
		});

		// If it's the startPoint
		if (i == 0 && pl == null) {
			pl = new PlayerLap();
			pl.player = other.gameObject.GetComponentInParent<Player>();

			if (pl.player == null)
				return;
			
			pl.startTime = Time.realtimeSinceStartup;
			pl.lastCP_Index = i;

			playersLap.Add(pl);
			return;
		}

		// If no data
		if (pl == null) return;

		// If we skiped a cp
		if (pl.lastCP_Index + 1 != i && pl.lastCP_Index != i) {
			playersLap.Remove(pl);
			return;
		}

		// If it's the endpoint
		if (i == checkPoints.Count - 1) {
			float time = Time.realtimeSinceStartup - pl.startTime;
			Player p = pl.player;
			playersLap.Remove(pl);

			p.AddScore(1000000f / time);
			return;
		}

		pl.lastCP_Index = i;
	}
}
