﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Steamworks;
using UnityEngine.Networking.NetworkSystem;


public class Player : NetworkBehaviour {

	[Header("Inputs")]
	[SerializeField] PlayerNumber playerNumber = PlayerNumber.Player1;
	[SerializeField] string boostInput = "Boost";

	[Header("Stats")]	
	[SerializeField] float score = 0f;
	public float Score { get { return score; } }
	[SerializeField] float maxLife = 100f;
	public float MaxLife { get { return maxLife; } }
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
	PlayerUI ui;

	[SyncVar]
    [SerializeField] ulong steamId;

    public override void OnStartServer()
    {
        base.OnStartServer();

        StartCoroutine(SetNameWhenReady());
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

	void Start () {
		rigidbody = GetComponent<Rigidbody>();

		life = maxLife;
		boost = maxBoost;

		ui = FindObjectOfType<PlayerUI>();
		if (ui != null)
			ui.SetPlayer(this);
	}

	public void AddScore(float s) {
		score += s;
	}

	public void AddScore(Collision c) {
		score += c.relativeVelocity.sqrMagnitude;
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
}
