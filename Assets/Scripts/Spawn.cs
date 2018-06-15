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

	private int DELAYBETWEENWAVES = 15;

	// List keeping track of how many of each unit to spawn. Order:
	// Knight, KungFuFighter, ..
	// Team one and two lists are inverted to preserve order of spawns
	private int[] numOfEachUnitToSpawn = {0, 0, 0, 0, 0, 0, 0, 0};
	private int[] unitPrices = {10, 20, 15, 40, 50, 10, 50, 50};
	public Unit[] unitPrefabs;
	private string[] unitNames = {"Knight", "KungFuFighter", "Archer", "Hammer", "Swordsman", "Mage", "DualSwords", "Sorceress"};

	private int goldGainPerTick = 1;

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
		// spawn units
		for (int i = 0; i < numOfEachUnitToSpawn.Length; i++){
			for (int c = 0; c < numOfEachUnitToSpawn[i]; c++){
				Unit newUnit = Instantiate(unitPrefabs[i], transform.position, transform.rotation);
	        	newUnit.GetComponent<Unit>().UNITNAME = unitNames[i];
	        	newUnit.GetComponent<Unit>().Initialize(playerId, this);
	        	NetworkServer.Spawn(newUnit.gameObject);
	        	numUnitsActive++;
        	}
		}
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

	//////////// Commands //////////////
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
		playerGold += goldGainPerTick;
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
}
