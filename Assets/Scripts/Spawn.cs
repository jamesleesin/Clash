using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Spawn : NetworkBehaviour {
	// either 0 or 1 when set
	[SyncVar]
	public int playerId = -1;

	[SyncVar(hook="OnGold")]
	public int playerGold = 100;
	[SyncVar(hook="OnUnit")]
	public int numUnitsActive;
	[SyncVar(hook="OnTime")]
	public int timeUntilNextWave; 
	[SyncVar(hook="OnLimitUpdate")]
	public int playerUnitLimit; 
	[SyncVar(hook="OnNumUnitSpawnUpdate")]
	public int totalNumUnitsToSpawn = 0;

	private int DELAYBETWEENWAVES = 20;

	// upgrade bool array
	private SyncListBool upgradesAcquired = new SyncListBool();

	// List keeping track of how many of each unit to spawn. Order:
	// Knight, KungFuFighter, ..
	// Team one and two lists are inverted to preserve order of spawns
	private int[] numOfEachUnitToSpawn = {0, 0, 0, 0, 0, 0, 0, 0, 0};
	private int[] unitPrices = {65, 50, 95, 100, 175, 185, 150, 280, 270};
	public Unit[] unitPrefabs;
	private string[] unitNames = {"Knight", "KungFuFighter", "Archer", "Crossbow", "Swordsman", "DualSwords", "Mage", "Hammer", "Ninja"};

	// gain 2g/s base
	private int baseGoldGainPerTick = 2;

	public GameObject[] componentsToDisable;

	public Cannon myCannon;

	public override void OnStartClient(){
		if (GameManager.singleton == null){
			Debug.Log("GM is null");
		}
		else{
			// initialize all upgrades to false
			for (int i = 0; i < 8; i++){
				upgradesAcquired.Add(false);
			}
			if (playerId == 0){
				GameManager.singleton.goldAmountTeamOne.text = playerGold.ToString();
				GameManager.singleton.numUnitsTeamOne.text = "Active Units: " + numUnitsActive.ToString();
				GameManager.singleton.unitLimitTeamOne.text = "Unit Limit:  0/" + playerUnitLimit.ToString();
			}
			else if (playerId == 1){
				GameManager.singleton.goldAmountTeamTwo.text = playerGold.ToString();
				GameManager.singleton.numUnitsTeamTwo.text = "Active Units: " + numUnitsActive.ToString();
				GameManager.singleton.unitLimitTeamTwo.text = "Unit Limit: 0/" + playerUnitLimit.ToString();
			}
			// base limit is 5
			playerUnitLimit = 5;
    	}
	}

	void OnGold(int amt){
		playerGold = amt;
		if (playerId == 0){
			GameManager.singleton.goldAmountTeamOne.text = playerGold.ToString();
		}
		else if (playerId == 1){
			GameManager.singleton.goldAmountTeamTwo.text = playerGold.ToString();
		}
	}

	void OnUnit(int num){
		numUnitsActive = num;
		if (playerId == 0){
			GameManager.singleton.numUnitsTeamOne.text = "Active Units: " + numUnitsActive.ToString();
		}
		else if (playerId == 1){
			GameManager.singleton.numUnitsTeamTwo.text = "Active Units: " + numUnitsActive.ToString();
		}
	}

	void OnTime(int newTime){
		timeUntilNextWave = newTime;
		GameManager.singleton.waveTimer.text = timeUntilNextWave.ToString();
	}

	void OnLimitUpdate(int newLimit){
		playerUnitLimit = newLimit;
		if (playerId == 0){
			GameManager.singleton.unitLimitTeamOne.text = "Unit Limit: " + totalNumUnitsToSpawn.ToString() + "/" + playerUnitLimit.ToString();
		}
		else if (playerId == 1){
			GameManager.singleton.unitLimitTeamTwo.text = "Unit Limit: " + totalNumUnitsToSpawn.ToString() + "/" + playerUnitLimit.ToString();
		}
	}

	void OnNumUnitSpawnUpdate(int newNum){
		totalNumUnitsToSpawn = newNum;
		if (playerId == 0){
			GameManager.singleton.unitLimitTeamOne.text = "Unit Limit: " + totalNumUnitsToSpawn.ToString() + "/" + playerUnitLimit.ToString();
		}
		else if (playerId == 1){
			GameManager.singleton.unitLimitTeamTwo.text = "Unit Limit: " + totalNumUnitsToSpawn.ToString() + "/" + playerUnitLimit.ToString();
		}
	}

	public void EnableComponents(){
		for (int i = 0; i < componentsToDisable.Length; i++)
		{
			componentsToDisable[i].SetActive(true);
		}
	}

	public override void OnNetworkDestroy()
	{
		// Set this to false
	}

	public override void OnStartServer()
	{
		GameManager.singleton.AddPlayer(this);
	}


	public override void OnStartLocalPlayer()
	{
		GameManager.singleton.localPlayer = this;
		EnableComponents();
		if (playerId == 1){
			Camera.main.transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, 270, Camera.main.transform.eulerAngles.z);
		}
	}

	[Server]
	public void ServerAddUnit(int index){
		// add unit to array
		numOfEachUnitToSpawn[index]++;
		RpcAddUnit(index);
	}

	[ClientRpc]
	void RpcAddUnit(int index){
		if (!isServer){
			// this was already done for host player so we do it for client players
			numOfEachUnitToSpawn[index]++;
		}
	}

	[Server]
	public void SpawnUnits(){
		// by adding to numUnitsActive only once at the end of all spawns, 
		// we can remove many OnUnit network calls (from n calls to 1 call)
		int totalSpawned = 0;
		// spawn units
		for (int i = 0; i < numOfEachUnitToSpawn.Length; i++){
			for (int c = 0; c < numOfEachUnitToSpawn[i]; c++){
				Unit newUnit = Instantiate(unitPrefabs[i], transform.position, transform.rotation);
	        	newUnit.GetComponent<Unit>().UNITNAME = unitNames[i];
	        	newUnit.GetComponent<Unit>().Initialize(playerId, this);
	        	NetworkServer.Spawn(newUnit.gameObject);
	        	totalSpawned++;
        	}
		}
		numUnitsActive += totalSpawned;
		//RpcSpawnUnits();
	}

/*
	[ClientRpc]
	void RpcInitializeUnit(Unit unit, string name){
		if (!isServer){
			// this was already done for host player so we do it for client players
			unit.GetComponent<Unit>().UNITNAME = name;
	        unit.GetComponent<Unit>().Initialize(playerId, this);
		}
	}*/

	// -------------------------- Commands ---------------------//
	[Command]
	public void CmdUpdateTimeUntilNextWave(){
		if (timeUntilNextWave > 0){
			timeUntilNextWave--;
		}
		else{
			timeUntilNextWave = DELAYBETWEENWAVES;
			// Spawn wave
			Debug.Log("Spawn wave for " + playerId);
			SpawnUnits();
		}
	}

	[Command]
	public void CmdIncreaseUnit(int index){
		if (EnoughGoldForSpawn(unitPrices[index]) && totalNumUnitsToSpawn < playerUnitLimit){
			playerGold -= unitPrices[index];
			ServerAddUnit(index);
			totalNumUnitsToSpawn++;
		}
	}

	[Command]
	public void CmdUnitDied(){
		numUnitsActive--;
	}

	[Command]
	public void CmdGainCannonGold(){
		playerGold += 200;
	}

	[Command]
	// add player gold
	public void CmdGainTimeGold(){
		playerGold += baseGoldGainPerTick;
		// first 4 upgrades are for gold
		for (int i = 0; i <= 3; i++){
			if (upgradesAcquired[i])
				playerGold += 1;
		}
	}

	[Command]
	// if upgrade has not been bought yet, check gold amount
	public void CmdPurchaseUpgrade(int index, int cost){
		if (playerGold >= cost){
			playerGold -= cost;
			upgradesAcquired[index] = true;

			// if upgrade is index 4, 5, 6, or 7 then it is a supply upgrade
			if (index >= 4 && index <= 7){
				// boost max supply by 5
				playerUnitLimit += 5;
			}
		}
	}

	// -------------------- Utility functions -----------------------//
	public int GetBuildingHp(){
		return transform.GetComponent<Building>().GetHp();
	}

	public int GetPlayerGold(){
		return playerGold;
	}

	// return true if player has enough gold for spawn, else false
	public bool EnoughGoldForSpawn(int amt){
		return playerGold >= amt ? true : false;
	}

    void DisableComponents (){
		for (int i = 0; i < componentsToDisable.Length; i++)
		{
			componentsToDisable[i].SetActive(false);
		}
	}

	public int[] GetSpawnArray(){
		return numOfEachUnitToSpawn;
	}

	// return the number of a certain unit spawned per wave
	public int GetNumSpawnPerWave(int index){
		return numOfEachUnitToSpawn[index];
	}

	// base destroyed
	public void BuildingDestroyed(){
		GameObject.Find("GameManager").GetComponent<GameManager>().MySpawnWasKilled();
	}

	// returns whether or not the upgrade has been bought already
	public bool UpgradeStatus(int index){
		return upgradesAcquired[index];
	}
}

