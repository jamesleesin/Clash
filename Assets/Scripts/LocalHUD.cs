using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalHUD : MonoBehaviour {
	public Text numKnightSpawnPerWave;
	public Text numKungFuFighterSpawnPerWave;
	public Text numArcherSpawnPerWave;
	public Text numCrossbowSpawnPerWave;
	public Text numSwordsmanSpawnPerWave;
	public Text numDualSwordsSpawnPerWave;
	public Text numMageSpawnPerWave;
	public Text numHammerSpawnPerWave;
	public Text numNinjaSpawnPerWave;
	private Spawn mySpawn;
	
	public void SetSpawn(Spawn s){
		mySpawn = s;
	}

	// Update is called once per frame
	void Update () {
		if (mySpawn == null){
			mySpawn = transform.GetComponent<GameManager>().GetLocalSpawn();
		}
		else{
			numKnightSpawnPerWave.text = mySpawn.GetNumSpawnPerWave(0).ToString();
			numKungFuFighterSpawnPerWave.text = mySpawn.GetNumSpawnPerWave(1).ToString();
			numArcherSpawnPerWave.text = mySpawn.GetNumSpawnPerWave(2).ToString();
			numCrossbowSpawnPerWave.text = mySpawn.GetNumSpawnPerWave(3).ToString();
			numSwordsmanSpawnPerWave.text = mySpawn.GetNumSpawnPerWave(4).ToString();
			numDualSwordsSpawnPerWave.text = mySpawn.GetNumSpawnPerWave(5).ToString();
			numMageSpawnPerWave.text = mySpawn.GetNumSpawnPerWave(6).ToString();
			numHammerSpawnPerWave.text = mySpawn.GetNumSpawnPerWave(7).ToString();
			numNinjaSpawnPerWave.text = mySpawn.GetNumSpawnPerWave(8).ToString();
		}
	}
}
