using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureBonus : MonoBehaviour {

	[SerializeField] float bonus = 20000f;
	[SerializeField] List<Player> players;

	public void OnPlayerEnter(Collider player) {
		Player p = player.transform.GetComponentInParent<Player>();

		if (!players.Contains(p)) {
			players.Add(p);
		}
	}

	public void OnPlayerBail(Collider player) {
		Player p = player.transform.GetComponentInParent<Player>();

		if (players.Contains(p)) {
			players.Remove(p);
		}
	}

	public void OnPlayerExit(Collider player) {
		Player p = player.transform.GetComponentInParent<Player>();

		if (players.Contains(p) && p != null) {
			p.AddScore(bonus);
			players.Remove(p);
		}
	}
}
