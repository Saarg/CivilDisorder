﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Steamworks;
using UnityEngine.Networking.NetworkSystem;


[RequireComponent(typeof(VehicleBehaviour.WheelVehicle))]

public class Player : NetworkBehaviour {

	public delegate void OnPlayerSpawn(Player p);
	public static OnPlayerSpawn onPlayerSpawn;

	public delegate void OnPlayerDestroy(Player p);
	public static OnPlayerDestroy onPlayerDestroy;

	[Header("Inputs")]
	[SerializeField] PlayerNumber playerNumber = PlayerNumber.Player1;
	[SerializeField] string boostInput = "Boost";

	[Header("Stats")]
	[SyncVar]
	[SerializeField] float score = 0f;
	public float Score { get { return score; } }
	[SerializeField] float maxLife = 100f;
	public float MaxLife { get { return maxLife; } }
	[SyncVar]
	[SerializeField] float life = 100f;
	public float Life { get { return life; } }
	[SerializeField] float maxBoost = 10f;
	public float MaxBoost { get { return maxBoost; } }
	[SerializeField] float boost = 10f;
	public float Boost { get { return boost; } }
	[Range(0f, 1f)]
	[SerializeField] float boostRegen = 0.2f;
	public float BoostRegen { get { return boostRegen; } }
	[SerializeField] float boostForce = 5000;
	public float BoostForce { get { return boostForce; } }

	new Rigidbody rigidbody;

	GameManager gameManager;
	PlayerUI ui;

	[SyncVar]
    [SerializeField] ulong steamId;

    public override void OnStartClient()
    {
        if (SteamNetworkManager.Instance != null)
        	StartCoroutine(SetNameWhenReady());
		else
			name = "Player " + playerControllerId;
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
		name = SteamFriends.GetFriendPersonaName(new CSteamID(steamId));
	}

	void Start () {
		rigidbody = GetComponent<Rigidbody>();

		life = maxLife;
		boost = maxBoost;

		ui = FindObjectOfType<PlayerUI>();
		if (ui != null && isLocalPlayer)
			ui.SetPlayer(this);

		if (onPlayerSpawn != null)
			onPlayerSpawn.Invoke(this);
		
		gameManager = GameManager.Instance;
	}

	void OnDestroy() {
		if (onPlayerDestroy != null)
			onPlayerDestroy.Invoke(this);		
	}

	public void AddScore(float s) {
		if (gameManager.gameState != GameManager.GameStates.Game && isLocalPlayer) return;

		CmdAddScore(s);
	}

	public void AddScore(Collision c) {
		if (gameManager.gameState != GameManager.GameStates.Game && isLocalPlayer) return;		

		CmdAddScore(c.relativeVelocity.sqrMagnitude);
	}

	[Command]
	public void CmdAddScore(float s) {
		if (gameManager.gameState != GameManager.GameStates.Game) return;

		score += s;
	}
	
	void Update() {
		boost += Time.deltaTime * boostRegen;
		if (boost > maxBoost) { boost = maxBoost; }
	}

	void FixedUpdate () {
		if (MultiOSControls.GetValue(boostInput, playerNumber) > 0.5f && boost > 0.1f) {
			rigidbody.AddForce(transform.forward * boostForce);

			boost -= Time.fixedDeltaTime;
			if (boost < 0f) { boost = 0f; }
		}
	}

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

			if (angle > 150f) {
				AddScore(col.relativeVelocity.sqrMagnitude);
			} else {
				life -= col.relativeVelocity.sqrMagnitude / 30f * col.rigidbody.mass / rigidbody.mass;
			}
		}
	}
}
