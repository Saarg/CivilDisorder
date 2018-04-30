using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class NetworkSelector : MonoBehaviour {

	[SerializeField] NetworkManager unetmanager;
	[SerializeField] SteamNetworkManager steamnetmanager;

	void Start() {
		if (SteamNetworkManager.Instance != null) {
			Destroy(unetmanager.gameObject);
		} else {
			
		}
	}

	public void StartHost() {
		if (SteamNetworkManager.Instance != null) {
			steamnetmanager.CreateLobby();
		} else {
			unetmanager.StartHost();
		}
	}

	public void FindMatch() {
		if (SteamNetworkManager.Instance != null) {
			steamnetmanager.FindMatch();
		} else {
			unetmanager.StartClient();
		}
	}

	public void Disconect() {
		if (SteamNetworkManager.Instance != null) {
			NetworkServer.Shutdown();
		} else {
			unetmanager.StopClient();
			unetmanager.StopHost();
		}

		SceneManager.LoadScene(0);
		return;
	}
}
