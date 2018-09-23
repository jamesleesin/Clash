﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class GameManager : NetworkBehaviour {
	public static GameManager singleton;
	
	[SyncVar]
	public int gameState; // 0 game not ready, 1 game started, 2 game over

	[SyncVar]
	public int timeUntilNextWave;

	// these variables are updated from Spawn calling the GameManager singleton
	public Text goldAmountTeamOne;
	public Text goldAmountTeamTwo;
	public Text unitLimitTeamOne;
	public Text unitLimitTeamTwo;
	public Text numUnitsTeamOne;
	public Text numUnitsTeamTwo;
	public Text waveTimer;

	// upgrades shop
	public GameObject upgradesShop;

	// local player!
	public Spawn localPlayer;

	private List<Spawn> players = new List<Spawn>();
	private List<Cannon> cannons = new List<Cannon>();

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
			// set cannons 
			cannons[0].SetEnemySpawn(players[1]);
			cannons[1].SetEnemySpawn(players[0]);

			// start the game, everyone joined
			ServerStartGame();
		}
		else{
			// still waiting
			ServerEnterGameState(0);
		}
	}

	public void AddCannon(Cannon cannon){
		cannons.Add(cannon);
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
				ClientState_GameOver(0);
				break;
			case 3:
				ClientState_GameOver(1);
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
	void ClientState_GameOver(int didIWin){
		if (isServer)
			return;
			
		if (didIWin == 0){
			GameObject.Find("ResultVictory").GetComponent<Text>().enabled = true;
		}
		else{
			GameObject.Find("ResultDefeat").GetComponent<Text>().enabled = true;
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

	// ----------------------------- Utility Functions ---------------//
	// return function for local spawn
	public Spawn GetLocalSpawn(){
		return localPlayer;
	}

	// Players losing their base, show defeat or victory screen
	public void MySpawnWasKilled(){
		for(int p = 0; p < players.Count; p++){
			// check HP of each player
			if (players[p].GetBuildingHp() <= 0){
				// this player lost
				if (players[p] == localPlayer){
					GameObject.Find("ResultDefeat").GetComponent<Text>().enabled = true;
					ServerEnterGameState(2);
				}
				else{
					GameObject.Find("ResultVictory").GetComponent<Text>().enabled = true;
					ServerEnterGameState(3);
				}
			}
		}
	}

	// ------------------------ Client UI Hooks -------------------------------
	// open upgrades menu
	public void BrowseUpgrades(){
		if (gameState == 1){
			// if active close shop else open shop
			if (upgradesShop.activeSelf)
				upgradesShop.SetActive(false);
			else
				upgradesShop.SetActive(true);
		}
	}
	// close upgrades menu
	public void CloseUpgrades(){
		upgradesShop.SetActive(false);
	}

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

	// spawn a crossbow
	public void SpawnCrossbow(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(3);
	}

	// spawn a swordsman
	public void SpawnSwordsman(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(4);
	}

	// spawn dualswords
	public void SpawnDualSwords(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(5);
	}

	// spawn a mage
	public void SpawnMage(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(6);
	}

	// spawn a hammer unit 
	public void SpawnHammer(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(7);
	}

	// spawn a ninja
	public void SpawnNinja(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(8);
	}

	/*
	// spawn a sorceress
	public void SpawnSorceress(){
		if (gameState == 1)
			localPlayer.CmdIncreaseUnit(8);
	}*/

	// -------------- UPGRADES -------------- //
	public void PurchaseMiningTools(){
		if (!localPlayer.UpgradeStatus(0)){
			if (localPlayer.GetPlayerGold() > 120){
				localPlayer.CmdPurchaseUpgrade(0, 120);
				EventSystem.current.currentSelectedGameObject.GetComponent<Button>().interactable = false;
			}
		}
	}

	public void OpenNewMine(){
		if (!localPlayer.UpgradeStatus(1)){
			if (localPlayer.GetPlayerGold() > 120){
				localPlayer.CmdPurchaseUpgrade(1, 120);
				EventSystem.current.currentSelectedGameObject.GetComponent<Button>().interactable = false;
			}
		}
	}

	public void BuildGoldRefinery(){
		if (!localPlayer.UpgradeStatus(2)){
			if (localPlayer.GetPlayerGold() > 120){
				localPlayer.CmdPurchaseUpgrade(2, 120);
				EventSystem.current.currentSelectedGameObject.GetComponent<Button>().interactable = false;
			}		
		}
	}

	public void HireAdditionalMiners(){
		if (!localPlayer.UpgradeStatus(3)){
			if (localPlayer.GetPlayerGold() > 120){
				localPlayer.CmdPurchaseUpgrade(3, 120);
				EventSystem.current.currentSelectedGameObject.GetComponent<Button>().interactable = false;
			}
		}
	}

	public void IncreaseFoodSupply1(){
		if (!localPlayer.UpgradeStatus(4)){
			if (localPlayer.GetPlayerGold() > 50){
				localPlayer.CmdPurchaseUpgrade(4, 50);
				EventSystem.current.currentSelectedGameObject.GetComponent<Button>().interactable = false;
			}
		}
	}

	public void IncreaseFoodSupply2(){
		if (!localPlayer.UpgradeStatus(5)){
			if (localPlayer.GetPlayerGold() > 50){
				localPlayer.CmdPurchaseUpgrade(5, 50);
				EventSystem.current.currentSelectedGameObject.GetComponent<Button>().interactable = false;
			}
		}
	}

	public void IncreaseFoodSupply3(){
		if (!localPlayer.UpgradeStatus(6)){
			if (localPlayer.GetPlayerGold() > 50){
				localPlayer.CmdPurchaseUpgrade(6, 50);
				EventSystem.current.currentSelectedGameObject.GetComponent<Button>().interactable = false;
			}
		}
	}

	public void IncreaseFoodSupply4(){
		if (!localPlayer.UpgradeStatus(7)){
			if (localPlayer.GetPlayerGold() > 50){
				localPlayer.CmdPurchaseUpgrade(7, 50);
				EventSystem.current.currentSelectedGameObject.GetComponent<Button>().interactable = false;
			}
		}
	}
}
