﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Steamworks;
using UnityEngine.Networking.NetworkSystem;
using VehicleBehaviour;


[RequireComponent(typeof(WheelVehicle))]

public class Player : NetworkBehaviour {

	public delegate void OnPlayerSpawn(Player p);
	public static OnPlayerSpawn onPlayerSpawn;

	public delegate void OnPlayerDestroy(Player p);
	public static OnPlayerDestroy onPlayerDestroy;

	[Header("Inputs")]
	[SerializeField] PlayerNumber playerNumber = PlayerNumber.Player1;
	[SerializeField] string resetInput = "Reset";

	[Header("Stats")]
	[SyncVar]
	[SerializeField] float score = 0f;
	public float Score { get { return score; } }
	[SerializeField] float maxLife = 100f;
	public float MaxLife { get { return maxLife; } }
	[SyncVar]
	[SerializeField] float life = 100f;
	public float Life { get { return life; } }

	[SyncVar(hook="UpdateUsername")]
	string username;
	void UpdateUsername(string u) {
		username = u;
		name = username;
	}

	[Header("Prefabs")]
	[SerializeField] ScorePopup scorePopupPrefab;

	WheelVehicle vehicle;
	new Rigidbody rigidbody;

	GameManager gameManager;
	PlayerUI ui;

	WheelCollider[] wheels;
	
    [SerializeField] ulong steamId;

    IEnumerator SetNameWhenReady()
    {
        // Wait for client to get authority, then retrieve the player's Steam ID
        var id = GetComponent<NetworkIdentity>();
        while (id.clientAuthorityOwner == null)
        {
            yield return null;
        }

        steamId = SteamNetworkManager.Instance.GetSteamIDForConnection(id.clientAuthorityOwner).m_SteamID;
		username = SteamFriends.GetFriendPersonaName(new CSteamID(steamId));
	}

	public override void OnStartServer() {
		if (SteamNetworkManager.Instance != null)
        	StartCoroutine(SetNameWhenReady());
		else
			username = "Player " + playerControllerId;
	} 

	void Start () {
		rigidbody = GetComponent<Rigidbody>();

		life = maxLife;

		ui = FindObjectOfType<PlayerUI>();
		if (ui != null && isLocalPlayer)
			ui.SetPlayer(this);

		if (onPlayerSpawn != null)
			onPlayerSpawn.Invoke(this);
		
		gameManager = GameManager.Instance;

		wheels = GetComponentsInChildren<WheelCollider>();
		
		vehicle = GetComponent<WheelVehicle>();
	}

	void OnDestroy() {
		if (onPlayerDestroy != null)
			onPlayerDestroy.Invoke(this);	

		if (isLocalPlayer && SteamManager.Initialized) {
			if (score > 50000f) {
				SteamUserStats.SetAchievement("50K");                
				SteamUserStats.SetStat("50K", 1);
				SteamUserStats.StoreStats();
			}
			if (score > 100000f) {
				SteamUserStats.SetAchievement("100K");                
				SteamUserStats.SetStat("100K", 1);
				SteamUserStats.StoreStats();
			}
			if (score > 200000f) {
				SteamUserStats.SetAchievement("200K");                
				SteamUserStats.SetStat("200K", 1);
				SteamUserStats.StoreStats();
			}
			if (score > 500000f) {
				SteamUserStats.SetAchievement("500K");                
				SteamUserStats.SetStat("500K", 1);
				SteamUserStats.StoreStats();
			}
		}
	}

	public void AddScore(float s) {
		if (gameManager.gameState != GameManager.GameStates.Game || !isLocalPlayer) return;

		CmdAddScore(s);

		if (s < 100f)
			return;
		
		GameObject popup = Instantiate(scorePopupPrefab.gameObject, transform);
		ScorePopup scorePopup = popup.GetComponent<ScorePopup>();
		scorePopup.SetScore(s);
	}

	public void AddScore(Collision c) {
		if (gameManager.gameState != GameManager.GameStates.Game || !isLocalPlayer) return;		

		CmdAddScore(c.relativeVelocity.sqrMagnitude);

		if (c.relativeVelocity.sqrMagnitude < 100f)
			return;

		GameObject popup = Instantiate(scorePopupPrefab.gameObject, transform);
		ScorePopup scorePopup = popup.GetComponent<ScorePopup>();
		scorePopup.SetScore(c.relativeVelocity.sqrMagnitude);
	}

	[Command]
	public void CmdAddScore(float s) {
		if (gameManager.gameState != GameManager.GameStates.Game) return;

		score += s;
	}
	
	float lastReset;
	void Update() {
		if (isLocalPlayer) {
			if (MultiOSControls.GetValue(resetInput, playerNumber) > .5f && isLocalPlayer && !handlingdeath && Time.realtimeSinceStartup - lastReset > 0.5f)
			{
				lastReset = Time.realtimeSinceStartup;
				CmdReset();
			}
		}
		
		if (isServer) {
			if ((life <= 0 || transform.position.y < -10) && !handlingdeath) {
				StartCoroutine(HandleDeath(50000f));

				TargetPopup(connectionToClient, -50000f);
			}
		}
	}

	bool airTimecount = false; // Used to prevent the spawn from scoring
	float airTimeScore = 0;

	float driftStart;
	float driftScore;
	void FixedUpdate () {
		if (!isLocalPlayer)
			return;
		
		// AirTime scoring
		int groundedWheels = wheels.Length;
		foreach (WheelCollider wheel in wheels) {
			if (!wheel.isGrounded)
				groundedWheels--;
		}
		if (groundedWheels <= wheels.Length/2 && rigidbody.velocity.sqrMagnitude > 10f)
			airTimeScore += (wheels.Length - groundedWheels) * Time.fixedDeltaTime * 200f;

		if (groundedWheels == wheels.Length) {
			if (airTimeScore > 100f && airTimecount && rigidbody.velocity.sqrMagnitude > 10f) {
				AddScore(Mathf.Clamp(airTimeScore, 0f, 2000f));
			}

			airTimeScore = 0;

			airTimecount = true;
		}

		// Drift scoring
		if (driftStart != 0) {
			float angle = Vector3.Angle(transform.forward, rigidbody.velocity);
			angle = angle < 0 ? -angle : angle;

			driftScore += Time.fixedDeltaTime * angle * 10f;

			if (angle < 10f && driftScore > 100f) {
				AddScore(driftScore);
			}

			if (angle < 10f || angle > 90f || rigidbody.velocity.sqrMagnitude < 100) {
				driftScore = 0;
				driftStart = 0;
			}
		}

		if (vehicle.drift) {
			if (driftStart == 0)
				driftStart = Time.realtimeSinceStartup;
		}
	}

	public bool collisionDetected = false;
	void OnCollisionEnter(Collision col) {
		if (gameObject.layer == col.gameObject.layer && isServer) {
			Vector3 myDir = transform.forward;
			Vector3 normal = col.relativeVelocity;

			myDir.y = 0;
			myDir.Normalize();
			normal.y = 0;
			normal.Normalize();

			float angle = Vector3.Angle(myDir, normal);

			// Debug.DrawLine(transform.position, transform.position + myDir, Color.red, 10f);
			// Debug.DrawLine(transform.position, transform.position + normal, Color.green, 10f);

			collisionDetected = true;

			if (angle > 150f) {
				AddScore(col.relativeVelocity.sqrMagnitude);
			} else {
				life -= col.relativeVelocity.sqrMagnitude / 10f * col.rigidbody.mass / rigidbody.mass;
				
				Player otherP = col.gameObject.GetComponentInParent<Player>();

				if (SteamManager.Initialized){
					if (isLocalPlayer && life <= 0) {
						SteamUserStats.SetAchievement("REKT");                
						SteamUserStats.SetStat("REKT", 1);
						SteamUserStats.StoreStats();
					} else if (otherP != null && otherP.isLocalPlayer && life <= 0) {
						SteamUserStats.SetAchievement("KILL");                
						SteamUserStats.SetStat("KILL", 1);
						SteamUserStats.StoreStats();
					}
				}
			}
		}
	}

	[Command]
	void CmdReset() {
		StartCoroutine(HandleDeath(10000f));
	}

	[SyncVar] bool handlingdeath = false;
	IEnumerator HandleDeath(float malus) {
		if (isServer && !handlingdeath) {
			handlingdeath = true;
			RpcDisassemble();
			score = score > malus ? score - malus : 0;

			yield return new WaitForSeconds(3f);
			vehicle.ResetPos();			
			RpcAssemble();
			life = maxLife;

			handlingdeath = false;
		}
	}

	[ClientRpc]
	void RpcDisassemble() {
		vehicle.toogleHandbrake(true);
		rigidbody.isKinematic = true;

		if (isLocalPlayer) {
			GameObject popup = Instantiate(scorePopupPrefab.gameObject, transform);
			ScorePopup scorePopup = popup.GetComponent<ScorePopup>();
			scorePopup.SetScore(-10000f);
		}

		Vector3 velocity = rigidbody.velocity;

		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;

		Transform body = transform.Find("Body");

		foreach (Transform child in body)
		{
			Rigidbody r = child.gameObject.AddComponent<Rigidbody>();
			r.mass = rigidbody.mass / body.childCount;
			r.velocity = velocity;
		}

		foreach (WheelCollider wc in wheels) {
			foreach (Transform child in wc.transform)
			{
				child.gameObject.SetActive(false);
			}
		}
	}

	[TargetRpc]
	void TargetPopup(NetworkConnection target, float s) {
		if (isLocalPlayer) {
			GameObject popup = Instantiate(scorePopupPrefab.gameObject, transform);
			ScorePopup scorePopup = popup.GetComponent<ScorePopup>();
			scorePopup.SetScore(s);
		}
	}

	[ClientRpc]
	void RpcAssemble() {
		vehicle.toogleHandbrake(false);
		vehicle.ResetPos();
		rigidbody.isKinematic = false;		

		Transform body = transform.Find("Body");
		foreach (Transform child in body)
		{
			Destroy(child.GetComponent<Rigidbody>());
			child.localPosition = Vector3.zero;
			child.localRotation = Quaternion.identity;
		}

		foreach (WheelCollider wc in wheels) {
			foreach (Transform child in wc.transform)
			{
				child.gameObject.SetActive(true);
			}
		}
	}
}
