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
	public static OnStartCountDown onStartCountDown;

	public delegate void OnStartGame();
	public static OnStartGame onStartGame;

	public delegate void OnEndGame();
	public static OnEndGame onEndGame;

	// States
	public GameStates gameState { get; protected set; } = GameStates.Waiting;

	public void NextState() {
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
	[Header("Lobby settings")]
	[SerializeField] GameObject lobbyHolder;
	[SerializeField] Lobby lobbyPlayer;
	List<Lobby> lobbyPlayers = new List<Lobby>();

	[SerializeField] int minPlayers = 2;
	[SerializeField] int maxPlayers = 4;

	[Header("Game settings")]
	List<Player> players = new List<Player>();

	[SerializeField] int countDownTime;
	float countDownStartTime;
	[SyncVar] float countDownTimeLeft;
	[SerializeField] int gameTime;
	public float GameTime { get { return gameTime; } }	
	float gameStartTime;
	[SyncVar] float gameTimeLeft;
	public float GameTimeLeft { get { return gameTimeLeft; } }

	// UI
	[Header("UI")]
	[SerializeField] Canvas countDownCanvas;
	[SerializeField] Text countDownText;
	[SerializeField] Text playerCountText;
	[SerializeField] Slider maxPlayerSlider;
	[SerializeField] Text maxPlayerText;	
	[SerializeField] Slider gameTimeSlider;
	[SerializeField] Text gameTimeText;

	void Awake () {
		if (instance != null) {
			Debug.LogWarning("More than one gamemanager in the scene");
			Destroy(this);
			return;
		}

		instance = this;

		Player.onPlayerSpawn += OnAddPlayer;
		Player.onPlayerDestroy += OnRemovePlayer;

		maxPlayerSlider.onValueChanged.AddListener((float v) => {
			maxPlayers = Mathf.FloorToInt(v);
			maxPlayerText.text = maxPlayers.ToString() + " player game";
		});

		gameTimeSlider.onValueChanged.AddListener((float v) => {
			gameTime = Mathf.FloorToInt(v * 60f);
			gameTimeText.text = v.ToString() + " minutes game";
		});
	}

	void Start() {
		WaitingEnter();
	}

	public override void OnStartServer() {
		NetworkServer.RegisterHandler(MsgType.Connect, OnPlayerConnect);
	}

	public override void OnStartClient() {
		lobbyPlayers.AddRange(GameObject.FindObjectsOfType<Lobby>());
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

	void OnPlayerConnect(NetworkMessage netMsg) {
		if (gameState == GameStates.Waiting && lobbyPlayers.Count < maxPlayers) {
			StartCoroutine(SpawnLobbyPlayerWhenReady(netMsg.conn));
		} else {
			// Spawn as spec
		}
	}

	IEnumerator SpawnLobbyPlayerWhenReady(NetworkConnection conn) {
		while(!conn.isReady) {			
			yield return new WaitForSeconds(0.1f);
		}
					
		SpawnLobbyPlayer(conn);
	}

	void SpawnLobbyPlayer(NetworkConnection conn) {
		GameObject lobbyPlayerGO = GameObject.Instantiate(lobbyPlayer.gameObject);

		NetworkServer.AddPlayerForConnection(conn, lobbyPlayerGO, (short) lobbyPlayers.Count);
		// NetworkServer.SpawnWithClientAuthority(lobbyPlayerGO, conn);

		RpcAddLobbyPlayer(lobbyPlayerGO);
	}

	[ClientRpc]
	void RpcAddLobbyPlayer(GameObject l) {	
		lobbyPlayers.Add(l.GetComponent<Lobby>());
	}
	
	void UpdatePositionPlayerUI() {
		for(int i = 0; i < lobbyPlayers.Count; i++) {
			RectTransform t = lobbyPlayers[i].transform as RectTransform;

			t.SetParent(lobbyHolder.transform);
			t.anchoredPosition = new Vector2(-(lobbyPlayers.Count * 300)/2 + (i * 300) + 150, 0);
			t.localScale = Vector3.one;
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
		lobbyHolder.SetActive(true);
	}

	void WaitingUpdate() {
		UpdatePositionPlayerUI();

		playerCountText.text = lobbyPlayers.Count.ToString() + "/" + maxPlayers.ToString();

		bool allReady = true;
		lobbyPlayers.ForEach((Lobby l) => {
			if (!l.isReady)
				allReady = false;
		});

		if (allReady && lobbyPlayers.Count >= minPlayers) {
			NextState();
		}
	}
	void WaitingExit() {
		lobbyHolder.SetActive(false);			
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
