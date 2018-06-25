using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Destroy this to win
public class Building : NetworkBehaviour {
	public Spawn mySpawn;

	[SyncVar(hook="OnChangeHealth")]
	private int hp;
	private int MAXHP = 1500;
	public Transform hpDisplay;
	private int PHYSICALRESIST = 1;
	private int MAGICRESIST = 1;

	// Use this for initialization
	void Start()
	{
		hp = MAXHP;
	}

	void Update(){
		if (hpDisplay != null){
			hpDisplay.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
		}
	}

	public int MyTeam(){
		return mySpawn.playerId;
	}

	/*
	* Take amount damage from a source
	*/
	public void TakeDamage(int amount, int damageType){
		if (!isServer){
			return;
		}

		if (hp > 0){
			hp -= amount - (damageType == 0 ? PHYSICALRESIST : MAGICRESIST);
			Debug.Log("take " + amount + " damage");
			if (hp <= 0){
				// game over for this team. Destroy the building from the local HUD
				transform.GetComponent<Spawn>().BuildingDestroyed();
			}
		}
	}

	public int GetHp(){
		return hp;
	}

	void OnChangeHealth(int newHealth){
		hp = newHealth;
		hpDisplay.localScale = new Vector3(newHealth/(float)MAXHP * 5f, 0.3f, 0.3f);
	}
}
