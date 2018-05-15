using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using VehicleBehaviour;

public class Lobby : NetworkBehaviour {

	[Header("Inputs")]
	[SerializeField] PlayerNumber playerNumber = PlayerNumber.Player1;
	[SerializeField] string carNavInput = "Horizontal";
	[SerializeField] string readyInput = "Jump";

	[Header("UI")]
	[SerializeField] Text pseudoText;
	[SerializeField] Button nextCarButton;
	[SerializeField] Button previousCarButton;
	[SerializeField] Image carPreview;
	[SerializeField] Image lifeBar;
	[SerializeField] Image speedBar;
	[SerializeField] Image boostBar;
	[SerializeField] Image weightBar;
	[SerializeField] Button readyButton;
	[SerializeField] Button notReadyButton;

	[Header("Cars")]	
	[SerializeField] List<WheelVehicle> vehicles;
	[SyncVar(hook="HookCurVehicle")]
	[SerializeField] int curVehicle;
	void HookCurVehicle(int v) {
		curVehicle = v;
		UpdateCarStat();
	}

	public bool isReady { get; protected set; } = false;

	[SyncVar]
    [SerializeField] ulong steamId;

	public override void OnStartServer() {
		GameManager.onStartCountDown += SpawnPlayer;

		if (SteamNetworkManager.Instance != null) {
        	StartCoroutine(SetNameWhenReady());
		} else {
            name = "Player " + playerControllerId;
            pseudoText.text = name;
		}
    }

	void OnDestroy() {
		GameManager.onStartCountDown -= SpawnPlayer;		
	}

    IEnumerator SetNameWhenReady()
    {
        // Wait for client to get authority, then retrieve the player's Steam ID
        var id = GetComponent<NetworkIdentity>();
        while (id.clientAuthorityOwner == null)
        {
            yield return null;
        }

        steamId = SteamNetworkManager.Instance.GetSteamIDForConnection(id.clientAuthorityOwner).m_SteamID;
	}

	public override void OnStartClient() {
		readyButton.gameObject.SetActive(false);
		notReadyButton.gameObject.SetActive(false);

		UpdateCarStat();
	}

	public override void OnStartAuthority() {
		nextCarButton.onClick.AddListener(NextCar);
		previousCarButton.onClick.AddListener(PreviousCar);

		readyButton.onClick.AddListener(Ready);
		notReadyButton.onClick.AddListener(NotReady);

		readyButton.gameObject.SetActive(true);
		notReadyButton.gameObject.SetActive(false);
	}

	float lastNavInput;
	void Update() {
		if (SteamNetworkManager.Instance != null) {
			name = SteamFriends.GetFriendPersonaName(new CSteamID(steamId));
            pseudoText.text = name;
		} else {
            name = "Player " + playerControllerId;
            pseudoText.text = name;
		}

		if (MultiOSControls.GetValue(readyInput, playerNumber) > 0 && isLocalPlayer) {
			Ready();
		} else if (MultiOSControls.GetValue(carNavInput, playerNumber) > 0 && isLocalPlayer && Time.realtimeSinceStartup - lastNavInput > 0.3f) {
			lastNavInput = Time.realtimeSinceStartup;
			CmdNextCar();
		} else if (MultiOSControls.GetValue(carNavInput, playerNumber) < 0 && isLocalPlayer && Time.realtimeSinceStartup - lastNavInput > 0.3f) {
			lastNavInput = Time.realtimeSinceStartup;			
			CmdPreviousCar();
		}
	}

	void NextCar() {
		CmdNextCar();
	}

	[Command]
	void CmdNextCar() {
		curVehicle = (curVehicle + 1) % vehicles.Count;
	}

	void PreviousCar() {
		CmdPreviousCar();
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

	void Ready() {
		CmdReady();
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

	void NotReady() {
		CmdNotReady();
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
		
		Vector3 startPos = Vector3.zero;
		startPos.x = -(GameManager.Instance.MaxPlayers * 4f / 2f) + (playerControllerId * 4f);
		startPos.y = 2;
		startPos.z = 0;

		GameObject player = GameObject.Instantiate(vehicles[curVehicle].gameObject, startPos, Quaternion.identity);

		NetworkServer.ReplacePlayerForConnection(connectionToClient, player, playerControllerId);
	}
}
