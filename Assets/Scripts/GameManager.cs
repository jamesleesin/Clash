using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {
	public static GameManager singleton;
	
	[SyncVar]
	public int gameState; // 0 game not ready, 1 game started, 2 game over

	[SyncVar]
	public int timeUntilNextWave;

	public Text goldAmountTeamOne;
	public Text goldAmountTeamTwo;
	public Text numUnitsTeamOne;
	public Text numUnitsTeamTwo;
	public Text waveTimer;

	// victory or defeat
	public Text victoryResultText;
	public Text defeatResultText;

	// local player!
	public Spawn localPlayer;

	private List<Spawn> players = new List<Spawn>();

	//private int timeUntilNextWave;

	private void Awake(){
		singleton = this;
	}

	public override void OnStartClient()
	{
		//cardSprites = Resources.LoadAll<Sprite>("cards");
		ClientHandleState(gameState);
	}

	// Add player to the game
	public void AddPlayer(Spawn spawn){
		players.Add(spawn);
		if (players.Count == 2){
			// start the game, everyone joined
			ServerStartGame();
		}
		else{
			// still waiting
			ServerEnterGameState(0);
		}
	}

	public void RemovePlayer(Spawn spawn){
		players.Remove(spawn);
	}

	[Server]
	void ServerEnterGameState(int newState){
		gameState = newState;
		RpcGameState(newState);
	}

	[ClientRpc]
	private void RpcGameState(int newState){
		ClientHandleState(newState);
	}

	[Client]
	void ClientHandleState(int newState){
		gameState = newState;
		ClientDisableAllButtons();
		switch(newState){
			// waiting on players
			case 0:
				ClientState_WaitingForPlayers();
				break;
			// start game
			case 1:
				ClientState_StartGame();
				break;
			// game ended
			case 2:
				ClientState_GameOver();
				break;
		}
			
	}

	[Client]
	public void ClientDisableAllButtons(){
		// disable all buttons, enable them later
	}

	// ------------------------ Client State Functions -------------------------------

	[Client]
	void ClientState_WaitingForPlayers(){
		// enable units that user can buy
	}

	[Client]
	void ClientState_StartGame(){
		// enable units that user can buy
	}

	[Client]
	// need to show game results to all clients as well
	void ClientState_GameOver(){
		if (localPlayer.transform.GetComponent<Building>().GetHp()<=0){
			defeatResultText.gameObject.SetActive(true);
		}
		else{
			victoryResultText.gameObject.SetActive(true);
		}
	}

	// ------------------------ Server State Functions -------------------------------

	[Server]
	public void ServerStartGame(){
		ServerEnterGameState(1);
		// start the game here
		StartGame();
	}

	[Server]
	void StartGame(){
		Debug.Log("Starting game");
		// dont let units collider with same team 
		Physics.IgnoreLayerCollision(8, 8);
		Physics.IgnoreLayerCollision(9, 9);

		// dont let each teams items collide with friendly team members
		Physics.IgnoreLayerCollision(8, 10);
		Physics.IgnoreLayerCollision(9, 11);
		// dont let items interact with other items
		Physics.IgnoreLayerCollision(10, 10);
		Physics.IgnoreLayerCollision(10, 11);
		Physics.IgnoreLayerCollision(11, 11);
	
		InvokeRepeating("UpdateTimeUntilNextWave", 0.0f, 1f);
		InvokeRepeating("PlayerGoldGain", 0.0f, 1f);
	}

	[Server]
	void UpdateTimeUntilNextWave(){
		for(int p = 0; p < players.Count; p++){
			players[p].CmdUpdateTimeUntilNextWave();
		}
	}

	[Server]
	void PlayerGoldGain(){
		for(int p = 0; p < players.Count; p++){
			players[p].CmdGainTimeGold();
		}
	}

	// ------------------------ Client UI Hooks -------------------------------
	// spawn a knight 
	public void SpawnKnight(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(0);
	}

	// spawn a kungfufighter 
	public void SpawnKungFuFighter(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(1);
	}

	// spawn an archer
	public void SpawnArcher(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(2);
	}

	// spawn a hammer unit 
	public void SpawnHammer(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(3);
	}

	// spawn a swordsman
	public void SpawnSwordsman(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(4);
	}

	// spawn a mage
	public void SpawnMage(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(5);
	}

	// spawn dualswords
	public void SpawnDualSwords(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(6);
	}

	// spawn a sorceress
	public void SpawnSorceress(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(7);
	}


	// return function for local spawn
	public Spawn GetLocalSpawn(){
		return localPlayer;
	}

	// Players losing their base
	public void MySpawnWasKilled(){
		if (localPlayer.transform.GetComponent<Building>().GetHp()<=0){
			defeatResultText.gameObject.SetActive(true);
		}
		else{
			victoryResultText.gameObject.SetActive(true);
		}
		// enter game over state
		ServerEnterGameState(2);
	}
}
