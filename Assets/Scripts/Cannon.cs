using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(sendInterval = 0.12f)]
public class Cannon : NetworkBehaviour {
	private Spawn enemySpawn;
	private int team;
	public Cannonball cannonballPrefab;

	// set values that dont change once set
	[SyncVar(hook="OnChangeMaxHealth")]
	private int MAXHP;
	private float RANGE;
	private float DELAYBETWEENATTACKS;
	private int DAMAGE;
	private float ROTATIONSPEED;
	private int PHYSICALRESIST;
	private int MAGICRESIST;
	private bool RANGED = false;

	// unit stats that change
	[SyncVar(hook="OnChangeHealth")]
	public int hp;
	private float attackCooldownTimer;
	public Transform hpDisplay;

	private GameObject targetUnit;
	public string AIMode = "Closest";
	Quaternion targetRot;

	// Network lerping
    [SyncVar(hook="OnChangeRotation")]
    Quaternion realRotation;
    Quaternion lastSentRotation;
    private float updateIntervalTimer;
    private float updateInterval = 0.08f;


	// Use this for initialization
	void Start () {
		realRotation = transform.rotation;
		lastSentRotation = transform.rotation;
	}
	
	public void Initialize(int t){
		team = t;
		if (team == 0){
			gameObject.layer = 8;
		}
		else if (team == 1){
			gameObject.layer = 9;
		}

		MAXHP = 500;
		RANGE = 23f;
		DELAYBETWEENATTACKS = 5f;
		DAMAGE = 10;
		ROTATIONSPEED = 0.5f;
		PHYSICALRESIST = 2;
		MAGICRESIST = 2;

		// initialize stats
		hp = MAXHP;
	}

	public void SetEnemySpawn(Spawn s){
		enemySpawn = s;
	}

	// spawn a cannonball and fire it
	[Server]
	void Fire(){
		attackCooldownTimer = DELAYBETWEENATTACKS;
		Vector3 itemSpawnPos = new Vector3(transform.position.x, transform.position.y + 1.85f, transform.position.z) + transform.forward * 3f;
		Vector3 dir = targetUnit.transform.position - itemSpawnPos;
		Cannonball newCannonball = Instantiate(cannonballPrefab, itemSpawnPos, Quaternion.identity);
		newCannonball.Initialize(team, DAMAGE);
		NetworkServer.Spawn(newCannonball.gameObject);
		//newShuriken.transform.Rotate(90, 0, 0);
		newCannonball.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(dir) * 50f, ForceMode.Impulse);
	}

	/*
	* Find a new target to focus
	* AIMode - Closest: Target closest enemy 
	* AIMode - Lowest: Target lowest HP enemy within a certain range
	*/
	void FindTarget(){
		// Use overlap sphere to find units in a certain range
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, RANGE);
		List<Transform> enemiesInRange = new List<Transform>();
	    int i = 0;
	    while (i < hitColliders.Length)
	    {
	    	// only target units
	    	if (hitColliders[i].gameObject.tag == "Unit"){
		    	// no targetting itself
		    	if (this.gameObject != hitColliders[i].gameObject){
		    		// no friendly fire
		    		if (hitColliders[i].gameObject.layer != gameObject.layer){
			        	enemiesInRange.Add(hitColliders[i].transform);
			    	}
			    }
			}
		    i++;
	    }

	    // Target closest enemy in range
		if (AIMode == "Closest"){
			targetUnit = GetClosestEnemy(enemiesInRange);
		}
	}

	/*
	* Find the closest enemy from an array of enemy transforms.
	* Use distance squared to avoid an expensive square root operation. This still preserves order by proximity.
	*/
	GameObject GetClosestEnemy(List<Transform> enemies){
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach(Transform potentialTarget in enemies)
        {
            Vector3 directionToTarget = potentialTarget.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if(dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
     
     	if (bestTarget != null){
        	return bestTarget.gameObject;
    	}
    	else{
    		return null;
    	}
	}

	// Update is called once per frame
	void Update () {
		if (hpDisplay != null){
			hpDisplay.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
		}
		if (isServer){
			if(targetUnit != null)
			{
				// if target is in range and attack is off cooldown then attack
				if (Vector3.Distance(targetUnit.transform.position, transform.position) <= RANGE && attackCooldownTimer <= 0f){
					Fire();
				}
			}
			else{
				FindTarget();
			}
		}

		// smooth position and rotation.
		// If server, sync position to all clients if change in position is > 1f
		// Sync rotation to all clients if change in rotation angle is > 8 degrees
		// If client, then Lerp between current position/rotation and server updated position/rotation
		if (isServer){
            updateIntervalTimer += Time.deltaTime;
            if (updateIntervalTimer > updateInterval)
            {
                updateIntervalTimer = 0;
				if (Quaternion.Angle(transform.rotation, lastSentRotation) > 12f){
					CmdSyncRot(transform.rotation);
					lastSentRotation = transform.rotation;
				}
            }
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.3f);
        }

		// attack cooldown
		if (attackCooldownTimer > 0f){
			attackCooldownTimer -= Time.deltaTime;
		}
	}

	void FixedUpdate(){
		if (isServer){
			RotateTowardTargetDirection();
		}	
	}

/*
	* Rotate to look at the target location
	*/
	void RotateTowardTargetDirection()  
	{
		if(targetUnit != null)
		{
			// Turn towards unit
			Vector3 pos = targetUnit.transform.position - transform.position;
			if (pos != Vector3.zero)
				targetRot = Quaternion.LookRotation(pos);
		}
		transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, ROTATIONSPEED);
	}

	[Command]
    void CmdSyncRot(Quaternion rotation)
    {
        realRotation = rotation;
    }

    /*
	* Take amount damage from a source. Only servers can take damage
	*/
	public void TakeDamage(int amount, int damageType){
		if (!isServer){
			return;
		}

		hp -= amount - (damageType == 0 ? PHYSICALRESIST : MAGICRESIST);
		if (hp <= 0){
			// give other team gold
			enemySpawn.CmdGainCannonGold();
			Destroy(this.gameObject);
		}
	}

	// when destroyed, inform the gamemanager
	void OnDestroy(){
		// when destroyed, other team gets gold as a reward 
	}

	/*
	* Return HP
	*/
	public int GetHP(){
		return hp;
	}

	/*
	* Return team
	*/
	public int GetTeam(){
		return team;
	}

	/************** HOOKS ******************/
	void OnChangeHealth(int newHealth){
		hp = newHealth;
		hpDisplay.localScale = new Vector3(newHealth/(float)MAXHP * 3f, 0.2f, 0.2f);
	}

	void OnChangeMaxHealth(int newHealth){
		MAXHP = newHealth;
	}

	void OnChangeRotation(Quaternion rot){
		realRotation = rot;
	}
}
