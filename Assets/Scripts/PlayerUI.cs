using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VehicleBehaviour;

public class PlayerUI : MonoBehaviour {

	[SerializeField] Player player;
	WheelVehicle vehicle;
	
	public void SetPlayer(Player p) { 
		player = p;
		if (p != null) {
			vehicle = p.GetComponent<WheelVehicle>();		
		}

		if (player != null && vehicle != null) playerUI.gameObject.SetActive(true);
		else playerUI.gameObject.SetActive(false);
	}
	public Player GetPlayer() { return player; }
	public WheelVehicle GetPlayerVehicle() { return vehicle; }

	[SerializeField] GameObject playerUI;	

	[SerializeField] Text scoreText;
	[SerializeField] Image lifeBar;
	[SerializeField] Image boostBar;
	[SerializeField] RectTransform speedo;

	[SerializeField] Text timeText;

	[SerializeField] Text[] playersScoreText;

	GameManager gameManager;
	List<Player> players = new List<Player>();

	void Start()
	{
		Player.onPlayerSpawn += OnSpawnPlayer;
		Player.onPlayerDestroy += OnDestroyPlayer;
	}
	
	void Update () {
		if (player == null || vehicle == null || 
			(GameManager.Instance.gameState != GameManager.GameStates.Game && GameManager.Instance.gameState != GameManager.GameStates.CountDown)
		) {
			playerUI.gameObject.SetActive(false);
			return;
		}
		playerUI.gameObject.SetActive(true);

		scoreText.text = "Score: " + Mathf.FloorToInt(player.Score);
		scoreText.fontSize = Mathf.FloorToInt(Mathf.Clamp(40 + 20 * (player.Score / 200000), 40, 60));

		lifeBar.fillAmount = Mathf.Lerp(lifeBar.fillAmount, player.Life / player.MaxLife, Time.deltaTime);
		boostBar.fillAmount = Mathf.Lerp(boostBar.fillAmount, vehicle.Boost / vehicle.MaxBoost, Time.deltaTime);

		float speedoAngle = 90 - (Mathf.Abs(vehicle.Speed) / 200f) * 160f;
		speedoAngle = Mathf.Clamp(speedoAngle, -80, 90);

		speedoAngle = Mathf.Lerp(speedo.rotation.eulerAngles.z, speedoAngle, Time.deltaTime * 2f);
		speedo.localRotation = Quaternion.Euler(0, 0, speedoAngle);

		if (gameManager == null && GameManager.Instance != null) {
			gameManager = GameManager.Instance;
		}

		if (gameManager != null && gameManager.gameState == GameManager.GameStates.Game) {
			StringBuilder sb = new StringBuilder();
			sb.Append(Mathf.FloorToInt(gameManager.GameTimeLeft / 60).ToString());
			sb.Append(":");
			if (gameManager.GameTimeLeft % 60 < 10)
				sb.Append("0");
			sb.Append(Mathf.FloorToInt(gameManager.GameTimeLeft % 60).ToString());

			timeText.text = sb.ToString();
			timeText.fontSize = Mathf.FloorToInt(Mathf.Clamp(60 + 20 * ((gameManager.GameTime - gameManager.GameTimeLeft) / gameManager.GameTime), 60, 80));

			int i;
			for (i = 0; i < players.Count; i++)
			{
				if (i < players.Count-1 && players[i].Score > players[i+1].Score) {
					Player p = players[i];
					players[i] = players[i+1];
					players[i+1] = p;
				}

				playersScoreText[i].gameObject.SetActive(true);
				playersScoreText[i].text = (players[i].name + ": " + Mathf.FloorToInt(players[i].Score)).ToUpper();
			}

			for (; i < playersScoreText.Length; i++)
			{
				playersScoreText[i].gameObject.SetActive(false);
			}
		} else {
			timeText.text = "";
		}
	}

	void OnSpawnPlayer(Player p) {
		players.Add(p);
	}

	void OnDestroyPlayer(Player p) {
		players.Remove(p);
	}
}
