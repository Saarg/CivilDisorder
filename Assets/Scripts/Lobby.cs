using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using VehicleBehaviour;

public class Lobby : NetworkBehaviour {

	[SerializeField] InputField pseudoInput;
	[SerializeField] Button nextCarButton;
	[SerializeField] Button previousCarButton;
	[SerializeField] Image carPreview;
	[SerializeField] Image lifeBar;
	[SerializeField] Image speedBar;
	[SerializeField] Image boostBar;
	[SerializeField] Image weightBar;
	[SerializeField] Button readyButton;
	[SerializeField] Button notReadyButton;

	[SerializeField] List<WheelVehicle> vehicles;
	[SyncVar(hook="HookCurVehicle")]
	[SerializeField] int curVehicle;
	void HookCurVehicle(int v) {
		curVehicle = v;
		UpdateCarStat();
	}

	public bool isReady { get; protected set; } = false;

	public override void OnStartServer() {
		GameManager.onStartCountDown += SpawnPlayer;
	}

	public override void OnStartClient() {
		readyButton.gameObject.SetActive(false);
		notReadyButton.gameObject.SetActive(false);

		UpdateCarStat();
	}

	public override void OnStartAuthority() {
		nextCarButton.onClick.AddListener(CmdNextCar);
		previousCarButton.onClick.AddListener(CmdPreviousCar);

		readyButton.onClick.AddListener(CmdReady);
		notReadyButton.onClick.AddListener(CmdNotReady);

		readyButton.gameObject.SetActive(true);
		notReadyButton.gameObject.SetActive(false);
	}

	[Command]
	void CmdNextCar() {
		curVehicle = (curVehicle + 1) % vehicles.Count;
	}

	[Command]
	void CmdPreviousCar() {
		curVehicle = curVehicle == 0 ? vehicles.Count - 1 : curVehicle - 1;
	}

	void UpdateCarStat() {
		WheelVehicle v = vehicles[curVehicle];
		Player p = vehicles[curVehicle].GetComponent<Player>();
		Rigidbody rb = vehicles[curVehicle].GetComponent<Rigidbody>();

		carPreview.sprite = v.preview;

		lifeBar.fillAmount = p.MaxLife / 200f;
		speedBar.fillAmount = 0.5f;
		boostBar.fillAmount = p.MaxBoost / 20f;
		weightBar.fillAmount = rb.mass / 3000f;
	}

	[Command]
	void CmdReady() {
		isReady = true;

		RpcReady();
	}

	[ClientRpc]
	void RpcReady() {
		if (hasAuthority) {
			readyButton.gameObject.SetActive(false);
			notReadyButton.gameObject.SetActive(true);
		} else {
			// feedback other player is ready
		}
	}

	[Command]
	void CmdNotReady() {
		isReady = false;

		RpcNotReady();
	}

	[ClientRpc]
	void RpcNotReady() {
		if (hasAuthority) {
			readyButton.gameObject.SetActive(true);
			notReadyButton.gameObject.SetActive(false);
		} else {
			// feedback other player is not ready
		}
	}

	void SpawnPlayer() {
		if (!isServer)
			return;
		
		GameObject player = GameObject.Instantiate(vehicles[curVehicle].gameObject);
		NetworkServer.ReplacePlayerForConnection(connectionToClient, player, playerControllerId);
	}
}
