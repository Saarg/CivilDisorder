﻿using System.Text;
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
		if (p != null)
			vehicle = p.GetComponent<WheelVehicle>(); 
		
		if (player != null && vehicle != null) playerUI.gameObject.SetActive(true);
		else playerUI.gameObject.SetActive(true);
	}
	public Player GetPlayer() { return player; }
	public WheelVehicle GetPlayerVehicle() { return vehicle; }

	[SerializeField] GameObject playerUI;	

	[SerializeField] Text scoreText;
	[SerializeField] Image lifeBar;
	[SerializeField] Image boostBar;
	[SerializeField] RectTransform speedo;

	[SerializeField] Text timeText;

	GameManager gameManager;	
	
	void Update () {
		if (player == null || vehicle == null)
			return;

		scoreText.text = "Score: " + Mathf.FloorToInt(player.Score);
		scoreText.fontSize = Mathf.FloorToInt(Mathf.Clamp(40 + 20 * (player.Score / 200000), 40, 60));

		lifeBar.fillAmount = Mathf.Lerp(lifeBar.fillAmount, player.Life / player.MaxLife, Time.deltaTime);
		boostBar.fillAmount = Mathf.Lerp(boostBar.fillAmount, player.Boost / player.MaxBoost, Time.deltaTime);

		float speedoAngle = 90 - (Mathf.Abs(vehicle.speed) / 200f) * 160f;
		speedoAngle = Mathf.Clamp(speedoAngle, -80, 90);

		speedoAngle = Mathf.Lerp(speedo.rotation.eulerAngles.z, speedoAngle, Time.deltaTime * 2f);
		speedo.rotation = Quaternion.Euler(0, 0, speedoAngle);

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
		} else {
			timeText.text = "";
		}
	}
}
