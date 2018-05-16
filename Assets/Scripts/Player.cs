using System.Collections;
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
	[SerializeField] string boostInput = "Boost";
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
	[SerializeField] float maxBoost = 10f;
	public float MaxBoost { get { return maxBoost; } }
	[SerializeField] float boost = 10f;
	public float Boost { get { return boost; } }
	[Range(0f, 1f)]
	[SerializeField] float boostRegen = 0.2f;
	public float BoostRegen { get { return boostRegen; } }
	[SerializeField] float boostForce = 5000;
	public float BoostForce { get { return boostForce; } }

	[SyncVar(hook="UpdateUsername")]
	string username;
	void UpdateUsername(string u) {
		username = u;
		name = username;
	}

	[Header("Prefabs")]
	[SerializeField] ScorePopup scorePopupPrefab;

	[Header("Boost")]	
	[SerializeField] ParticleSystem[] boostParticles;
	[SerializeField] AudioClip boostClip;
	[SerializeField] AudioSource boostSource;

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
		boost = maxBoost;

		ui = FindObjectOfType<PlayerUI>();
		if (ui != null && isLocalPlayer)
			ui.SetPlayer(this);

		if (onPlayerSpawn != null)
			onPlayerSpawn.Invoke(this);
		
		gameManager = GameManager.Instance;

		wheels = GetComponentsInChildren<WheelCollider>();
		
		vehicle = GetComponent<WheelVehicle>();
		if (!isLocalPlayer)
			vehicle.enabled = false;

		if (boostClip != null) {
			boostSource.clip = boostClip;
		}
	}

	void OnDestroy() {
		if (onPlayerDestroy != null)
			onPlayerDestroy.Invoke(this);	
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
	
	void Update() {
		if (isLocalPlayer) {
			boost += Time.deltaTime * boostRegen;
			if (boost > maxBoost) { boost = maxBoost; }

			if (MultiOSControls.GetValue(resetInput, playerNumber) > .5f && isLocalPlayer && !handlingdeath)
			{
				CmdReset();
				GameObject popup = Instantiate(scorePopupPrefab.gameObject, transform);
				ScorePopup scorePopup = popup.GetComponent<ScorePopup>();
				scorePopup.SetScore(-10000f);
			}
		}

		if (isServer) {
			if ((life <= 0 || transform.position.y < -10) && !handlingdeath) {
				StartCoroutine(HandleDeath(50000f));

				TargetPopup(connectionToClient, -50000f);
			}
		}
	}

	void FixedUpdate () {
		if (!isLocalPlayer)
			return;

		if (MultiOSControls.GetValue(boostInput, playerNumber) > 0.5f && boost > 0.1f) {
			rigidbody.AddForce(transform.forward * boostForce);

			boost -= Time.fixedDeltaTime;
			if (boost < 0f) { boost = 0f; }

			if (!boostParticles[0].isPlaying) {
				foreach (ParticleSystem boostParticle in boostParticles) {
					boostParticle.Play();
				}
			}

			if (!boostSource.isPlaying) {
				boostSource.Play();
			}
		} else {
			if (boostParticles[0].isPlaying) {
				foreach (ParticleSystem boostParticle in boostParticles) {
					boostParticle.Stop();
				}
			}

			if (boostSource.isPlaying) {
				boostSource.Stop();
			}
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
				life -= col.relativeVelocity.sqrMagnitude / 10f * col.rigidbody.mass / rigidbody.mass;
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
