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

	private int DELAYBETWEENWAVES = 20;

	// upgrade bool array
	private bool[] upgradesAcquired = {false, false, false, false};

	// List keeping track of how many of each unit to spawn. Order:
	// Knight, KungFuFighter, ..
	// Team one and two lists are inverted to preserve order of spawns
	private int[] numOfEachUnitToSpawn = {0, 0, 40, 0, 0, 0, 0, 1, 0};
	private int[] unitPrices = {65, 50, 95, 100, 150, 165, 150, 250, 240};
	public Unit[] unitPrefabs;
	private string[] unitNames = {"Knight", "KungFuFighter", "Archer", "Crossbow", "Swordsman", "DualSwords", "Mage", "Hammer", "Ninja"};

	// gain 2g/s base
	private int baseGoldGainPerTick = 2;

	public GameObject[] componentsToDisable;

	public override void OnStartClient(){
		if (GameManager.singleton == null){
			Debug.Log("GM is null");
		}
		else{
			if (playerId == 0){
				GameManager.singleton.goldAmountTeamOne.text = playerGold.ToString();
				GameManager.singleton.numUnitsTeamOne.text = numUnitsActive.ToString();
			}
			else if (playerId == 1){
				GameManager.singleton.goldAmountTeamTwo.text = playerGold.ToString();
				GameManager.singleton.numUnitsTeamTwo.text = numUnitsActive.ToString();
			}
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
			GameManager.singleton.numUnitsTeamOne.text = numUnitsActive.ToString();
		}
		else if (playerId == 1){
			GameManager.singleton.numUnitsTeamTwo.text = numUnitsActive.ToString();
		}
	}

	void OnTime(int newTime){
		timeUntilNextWave = newTime;
		GameManager.singleton.waveTimer.text = timeUntilNextWave.ToString();
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
		if (EnoughGoldForSpawn(unitPrices[index])){
			Debug.Log("Increase unit " + index + " by 1");
			playerGold -= unitPrices[index];
			ServerAddUnit(index);
		}
	}

	[Command]
	public void CmdUnitDied(){
		numUnitsActive--;
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
	// upgrade =s
	public void CmdPurchaseUpgrade(int index, int cost){
		// if can buy 
		if (!upgradesAcquired[index] && playerGold >= cost){
			playerGold -= cost;
			upgradesAcquired[index] = true;
		}
	}


	// -------------------- Utility functions -----------------------//

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
}
