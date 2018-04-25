using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

	// Singleton
	static GameManager instance;
	public static GameManager Instance {
		get {
			return instance;
		}
	}

	// Events
	public delegate void OnStartCountDown();
	public OnStartCountDown onStartCountDown;

	public delegate void OnStartGame();
	public OnStartGame onStartGame;

	public delegate void OnEndGame();
	public OnEndGame onEndGame;

	// States
	public GameStates gameState { get; protected set; } = GameStates.Waiting;

	void NextState() {
		if (isServer)
			RpcNextState();
	}

	[ClientRpc]
	void RpcNextState() {
		switch (this.gameState) {
			case GameStates.Waiting:
				WaitingExit();
				gameState = GameStates.CountDown;
				CountDownEnter();				
				break;
			case GameStates.CountDown:
				CountDownExit();
				gameState = GameStates.Game;
				GameEnter();			
				break;
			case GameStates.Game:
				GameExit();
				gameState = GameStates.Finished;	
				break;
		}
	}


	// Settings
	[Header("Settings")]
	[SerializeField] int minPlayers;
	List<Player> players = new List<Player>();

	[SerializeField] int countDownTime;
	float countDownStartTime;
	[SyncVar] float countDownTimeLeft;
	[SerializeField] int gameTime;
	float gameStartTime;
	[SyncVar] float gameTimeLeft;

	// UI
	[Header("UI")]
	[SerializeField] Canvas countDownCanvas;
	[SerializeField] Text countDownText;

	void Awake () {
		if (instance != null) {
			Debug.LogWarning("More than one gamemanager in the scene");
			Destroy(this);
			return;
		}

		instance = this;

		Player.onPlayerSpawn += OnAddPlayer;
		Player.onPlayerDestroy += OnRemovePlayer;
	}

	void Start() {
		WaitingEnter();
	}
	
	void Update () {
		switch (this.gameState) {
			case GameStates.Waiting:
				WaitingUpdate();				
				break;
			case GameStates.CountDown:
				CountDownUpdate();			
				break;
			case GameStates.Game:
				GameUpdate();	
				break;
		}
	}

	void OnAddPlayer(Player p) {
		players.Add(p);
	}

	void OnRemovePlayer(Player p) {
		players.Remove(p);		
	}

	// STATES
	public enum GameStates
	{
		Waiting,
		CountDown,
		Game,
		Finished
	}

	// Waiting state
	void WaitingEnter() {

	}

	void WaitingUpdate() {
		if (players.Count >= minPlayers) {
			NextState();
		}
	}
	void WaitingExit() {

	}

	// CountDown state
	void CountDownEnter() {
		if (onStartCountDown != null)
			onStartCountDown.Invoke();

		if (isServer) {
			countDownStartTime = Time.realtimeSinceStartup;
		}

		countDownText.text = Mathf.CeilToInt(countDownTime).ToString();
		countDownText.fontSize = 200;
		countDownCanvas.gameObject.SetActive(true);		
	}

	void CountDownUpdate() {
		if (isServer) {		
			countDownTimeLeft = countDownTime - (Time.realtimeSinceStartup - countDownStartTime);

			if (countDownTimeLeft < 0) {
				NextState();
			}
		}

		countDownText.text = Mathf.CeilToInt(countDownTimeLeft).ToString();
		countDownText.fontSize = Mathf.FloorToInt(200 + (1 - countDownTimeLeft%1) * 200);
	}
	void CountDownExit() {
		countDownCanvas.gameObject.SetActive(false);		
	}

	// Game state
	void GameEnter() {
		if (onStartGame != null)
			onStartGame.Invoke();

		if (isServer) {
			gameStartTime = Time.realtimeSinceStartup;
		}
	}

	void GameUpdate() {
		if (isServer) {		
			gameTimeLeft = gameTime - (Time.realtimeSinceStartup - gameStartTime);

			if (gameTimeLeft < 0) {
				NextState();
			}
		}
	}
	void GameExit() {
		if (onEndGame != null)
			onEndGame.Invoke();
	}
}
