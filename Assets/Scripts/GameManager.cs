using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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
				GameFinishEnter();
				break;
			case GameStates.Finished:
				// GameFinishExit();
				// gameState = GameStates.Waiting;
				// WaitingEnter();
				break;
		}
	}

	// Settings
	[Header("Lobby settings")]
	[SerializeField] GameObject lobbyHolder;
	public GameObject LobbyHolder { get { return lobbyHolder; } }
	[SerializeField] Lobby lobbyPlayer;
	List<Lobby> lobbyPlayers = new List<Lobby>();
	public void AddLobbyPlayer(Lobby l) {
		if (!lobbyPlayers.Contains(l))
			lobbyPlayers.Add(l);
	}

	public enum LobbyAccess { Private, Friends, Public }
	public LobbyAccess lobbyAccess = LobbyAccess.Friends;

	public int curTrack = 1;
	[SyncVar] public int track;

	[SyncVar]
	[SerializeField] int minPlayers = 2;
	public int MinPlayers { get { return minPlayers; } }
	[SyncVar]
	[SerializeField] int maxPlayers = 4;
	public int MaxPlayers { get { return maxPlayers; } }	

	[Header("Game settings")]
	List<Player> players = new List<Player>();

	[SerializeField] int countDownTime;
	float countDownStartTime;
	[SyncVar] float countDownTimeLeft;
	[SyncVar]
	[SerializeField] int gameTime;
	public float GameTime { get { return gameTime; } }	
	float gameStartTime;
	[SyncVar] float gameTimeLeft;
	public float GameTimeLeft { get { return gameTimeLeft; } }

	// UI
	[Header("UI")]
	[SerializeField] Canvas countDownCanvas;
	[SerializeField] Text countDownText;
	[SerializeField] Dropdown lobbyAccessDropDown;
	[SerializeField] Text playerCountText;
	[SerializeField] Slider maxPlayerSlider;
	[SerializeField] Text maxPlayerText;	
	[SerializeField] Slider gameTimeSlider;
	[SerializeField] Text gameTimeText;
	[SerializeField] GameObject endOfGameUI;
	[SerializeField] Text mainText;
	[SerializeField] Button forceStartButton;

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

	public void OnChangePlayerNumber (float v) {
		maxPlayers = Mathf.FloorToInt(v);
		maxPlayerText.text = maxPlayers.ToString() + " player game";
	}

	public void OnChangeGameTime (float v) {
		gameTime = Mathf.FloorToInt(v * 60f);
		gameTimeText.text = v.ToString() + " minutes game";
	}

	public void OnChangeLobbyAccess(int access) {
		lobbyAccess = (LobbyAccess) access;

		if (SteamNetworkManager.Instance != null)
			SteamNetworkManager.Instance.lobbyAccess = (ELobbyType) access;
	}

	public override void OnStartServer() {
		NetworkServer.RegisterHandler(MsgType.Connect, OnPlayerConnect);

		GameFinishExit();
		gameState = GameStates.Waiting;
		WaitingEnter();

		if (SteamNetworkManager.Instance != null)
        	lobbyAccessDropDown.gameObject.SetActive(true);
		else
        	lobbyAccessDropDown.gameObject.SetActive(false);
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
			case GameStates.Finished:
				GameFinishUpdate();	
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
		AddLobbyPlayer(l.GetComponent<Lobby>());
	}
	
	void UpdatePositionPlayerUI() {
		for(int i = 0; i < lobbyPlayers.Count; i++) {
			RectTransform t = lobbyPlayers[i].transform as RectTransform;

			t.SetParent(lobbyHolder.transform);
			t.anchoredPosition3D = new Vector3(-(lobbyPlayers.Count * 300)/2 + (i * 300) + 150, 0, 0);
			t.localScale = Vector3.one;
			t.localRotation = Quaternion.identity;
		}
	}

	void OnAddPlayer(Player p) {
		players.Add(p);
	}

	void OnRemovePlayer(Player p) {
		players.Remove(p);		
	}

	IEnumerator LoadAsyncScene(int scene)
    {
		AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(curTrack);

		while (!asyncLoad.isDone)
        {
            yield return null;
        }

		curTrack = scene;
        asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }	
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
		forceStartButton.gameObject.SetActive(false);

		if (SteamNetworkManager.Instance != null) {
			SteamNetworkManager.Instance.SetLobbyMemberLimit(maxPlayers);
		}
	}

	void WaitingUpdate() {
		UpdatePositionPlayerUI();

		playerCountText.text = lobbyPlayers.Count.ToString() + "/" + maxPlayers.ToString();

		if (lobbyPlayers.Count >= minPlayers && isServer) {
			forceStartButton.gameObject.SetActive(true);
		}

		if (curTrack != track)
			StartCoroutine(LoadAsyncScene(track));

		bool allReady = true;
		lobbyPlayers.ForEach((Lobby l) => {
			if (!l.isReady)
				allReady = false;
		});

		if (allReady && lobbyPlayers.Count >= maxPlayers) {
			NextState();
		}
	}
	void WaitingExit() {
		lobbyHolder.SetActive(false);
		forceStartButton.gameObject.SetActive(false);	

		if (SteamNetworkManager.Instance != null) {
			SteamNetworkManager.Instance.LeaveLobby();	
		}	

		lobbyPlayers.Clear();
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

	// GameFinish state
	void GameFinishEnter() {
		endOfGameUI.SetActive(true);

		Player[] players = GameObject.FindObjectsOfType<Player>();

		int bestIndex = 0;

		for (int i = 1; i < players.Length; i++)
		{
			if (players[bestIndex].Score < players[i].Score)
				bestIndex = i;
		}

		mainText.text = (players[bestIndex].name + " is the best\n" + Mathf.FloorToInt(players[bestIndex].Score) + " points").ToUpper();		
		if (players[bestIndex].isLocalPlayer) {
			SteamUserStats.SetAchievement("WIN_A_GAME");                
			SteamUserStats.SetStat("WIN_A_GAME", 1);
			SteamUserStats.StoreStats();
		}
	}

	void GameFinishUpdate() {

	}
	
	void GameFinishExit() {
		endOfGameUI.SetActive(false);
	}

	public void ResetState() {
		switch (this.gameState) {
			case GameStates.Waiting:
				WaitingExit();				
				break;
			case GameStates.CountDown:
				CountDownExit();			
				break;
			case GameStates.Game:
				GameExit();
				break;
			case GameStates.Finished:
				GameFinishExit();
				break;
		}

		gameState = GameStates.Waiting;
		WaitingEnter();
	}

	public void Quit() {
		Application.Quit();
	}
}
