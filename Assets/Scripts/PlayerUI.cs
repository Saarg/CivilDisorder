using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VehicleBehaviour;

public class PlayerUI : MonoBehaviour {

	[SerializeField] Player player;
	WheelVehicle vehicle;
	
	public void SetPlayer(Player p) { player = p; vehicle = p.GetComponent<WheelVehicle>(); }
	public Player GetPlayer() { return player; }
	public WheelVehicle GetPlayerVehicle() { return vehicle; }

	[SerializeField] Text scoreText;
	[SerializeField] Image lifeBar;
	[SerializeField] Image boostBar;
	[SerializeField] RectTransform speedo;

	void Start () {
		
	}
	
	void Update () {
		scoreText.text = "Score: " + Mathf.FloorToInt(player.Score);
		scoreText.transform.localScale = Vector3.Lerp(scoreText.transform.localScale, Vector3.one + Vector3.one * (player.Score / 200000), Time.deltaTime);

		lifeBar.fillAmount = Mathf.Lerp(lifeBar.fillAmount, player.Life / player.MaxLife, Time.deltaTime);
		boostBar.fillAmount = Mathf.Lerp(boostBar.fillAmount, player.Boost / player.MaxBoost, Time.deltaTime);

		float speedoAngle = 90 - (Mathf.Abs(vehicle.speed) / 200f) * 160f;
		speedoAngle = Mathf.Clamp(speedoAngle, -80, 90);

		speedoAngle = Mathf.Lerp(speedo.rotation.eulerAngles.z, speedoAngle, Time.deltaTime * 2f);
		speedo.rotation = Quaternion.Euler(0, 0, speedoAngle);
	}
}
