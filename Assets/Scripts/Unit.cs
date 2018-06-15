﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Unit : NetworkBehaviour {
	private Spawn mySpawn;
	private int team;
	// for layermasks, dont change
	private int layerTeamOne = 8;
	private int layerTeamTwo = 9;

	// unit prefabs
	public Arrow arrowPrefab;
	private Arrow newArrow;
	public Meteor meteorPrefab;
	public Icicle iciclePrefab;

	// set values that dont change once set
	[SyncVar(hook="OnChangeMaxHealth")]
	private int MAXHP;
	private float RANGE;
	private float DELAYBETWEENATTACKS;
	private int DAMAGE;
	private float ROTATIONSPEED;
	public string UNITNAME;
	private int PHYSICALRESIST;
	private int MAGICRESIST;
	private bool RANGED = false;

	// unit stats that change
	[SyncVar(hook="OnChangeHealth")]
	public int hp;
	private float attackCooldownTimer;
	public Transform hpDisplay;

	[SyncVar]
	private bool attacking = false;
	private bool targetingBuilding = false;

	// AI
	private Vector3 targetDestination;
	private Vector3 deltaDestination;
	private GameObject targetUnit;
	public string AIMode = "Closest";
	UnityEngine.AI.NavMeshAgent navAgent;
	Quaternion targetRot;
	GameObject enemySpawn;


	// animation
	private Animator animator;
	public NetworkAnimator networkAnimator;

	// Network lerping
	[SyncVar(hook="OnChangePosition")]
    Vector3 realPosition;
    [SyncVar(hook="OnChangeRotation")]
    Quaternion realRotation;
    private float updateInterval;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		gameObject.GetComponent<NetworkAnimator>().SetParameterAutoSend(0,true); 
		gameObject.GetComponent<NetworkAnimator>().SetParameterAutoSend(1,true); 
		gameObject.GetComponent<NetworkAnimator>().SetParameterAutoSend(2,true); 
		realPosition = transform.position;
		realRotation = transform.rotation;
	}

	public void Initialize(int t, Spawn s){
		mySpawn = s;
		team = t;
		if (team == 0){
			gameObject.layer = 8;
		}
		else if (team == 1){
			gameObject.layer = 9;
		}

		// Movement speed is controlled by setting run animation playback speed
		if (UNITNAME == "Knight"){
			MAXHP = 10;
			RANGE = 2.5f;
			DELAYBETWEENATTACKS = 2f;
			DAMAGE = 2;
			ROTATIONSPEED = 0.5f;
			PHYSICALRESIST = 1;
			MAGICRESIST = 1;
			// Movement speed 0.5
		}
		else if (UNITNAME == "KungFuFighter"){
			MAXHP = 8;
			RANGE = 3.3f;
			DELAYBETWEENATTACKS = 1.2f;
			DAMAGE = 1;
			ROTATIONSPEED = 0.5f;
			PHYSICALRESIST = 0;
			MAGICRESIST = 1;
			// Movement speed 0.8
		}
		else if (UNITNAME == "Archer"){
			MAXHP = 5;
			RANGE = 25f;
			DELAYBETWEENATTACKS = 3f;
			DAMAGE = 1;
			ROTATIONSPEED = 0.4f;
			PHYSICALRESIST = 0;
			MAGICRESIST = 0;
			RANGED = true;
			// Movement speed 0.4
		}
		else if (UNITNAME == "Hammer"){
			MAXHP = 8;
			RANGE = 4f;
			DELAYBETWEENATTACKS = 3f;
			DAMAGE = 2;
			ROTATIONSPEED = 0.5f;
			PHYSICALRESIST = 0;
			MAGICRESIST = 1;
			// Movement speed 0.6
		}
		else if (UNITNAME == "Swordsman"){
			MAXHP = 8;
			RANGE = 6f;
			DELAYBETWEENATTACKS = 2.5f;
			DAMAGE = 2;
			ROTATIONSPEED = 0.5f;
			PHYSICALRESIST = 0;
			MAGICRESIST = 1;
			// Movement speed 0.6
		}
		else if (UNITNAME == "Mage"){
			MAXHP = 8;
			RANGE = 20f;
			DELAYBETWEENATTACKS = 3f;
			DAMAGE = 0;
			ROTATIONSPEED = 0.5f;
			PHYSICALRESIST = 0;
			MAGICRESIST = 1;
			RANGED = true;
			// Movement speed 0.6
		}
		else if (UNITNAME == "DualSwords"){
			MAXHP = 8;
			RANGE = 2.3f;
			DELAYBETWEENATTACKS = 2f;
			DAMAGE = 2;
			ROTATIONSPEED = 0.5f;
			PHYSICALRESIST = 0;
			MAGICRESIST = 1;
			// Movement speed 0.8
		}
		else if (UNITNAME == "Sorceress"){
			MAXHP = 8;
			RANGE = 15f;
			DELAYBETWEENATTACKS = 3f;
			DAMAGE = 0;
			ROTATIONSPEED = 0.5f;
			PHYSICALRESIST = 0;
			MAGICRESIST = 1;
			RANGED = true;
			// Movement speed 0.8
		}

		// initialize stats
		hp = MAXHP;
		// recalculate movement targets 10 times per second
		InvokeRepeating("MoveUnit", 0.0f, 0.1f);
		// find a new target every 1 sec
		InvokeRepeating("FindTarget", 0.0f, 1f);
	}
	
	/*
	* Call this function to pause for pauseTime seconds for attack animation
	*/
	public IEnumerator AttackingPause(float pauseTime)
	{
		animator.ResetTrigger("Attack1Trigger");
		//if (!isServer){
		gameObject.GetComponent<NetworkAnimator>().SetTrigger("Attack1Trigger");
		//}
		//else{
			//animator.SetTrigger("Attack1Trigger");
		//}

		yield return new WaitForSeconds(pauseTime);
		attacking = false;
	}

	/*
	* Trigger an attack animation
	*/
	void Attack(){
		animator.SetBool("Moving", false);
		attacking = true;
		attackCooldownTimer = DELAYBETWEENATTACKS;
		if (UNITNAME == "Knight"){
			StartCoroutine (AttackingPause(0.8f));
		}
		else if (UNITNAME == "KungFuFighter"){
			StartCoroutine (AttackingPause(1.1f));
		}
		else if (UNITNAME == "Archer"){
			StartCoroutine (AttackingPause(1.0f));
		}
		else if (UNITNAME == "Hammer"){
			StartCoroutine (AttackingPause(1.7f));
		}
		else if (UNITNAME == "Swordsman"){
			StartCoroutine (AttackingPause(1.3f));
		}
		else if (UNITNAME == "Mage"){
			StartCoroutine (AttackingPause(1.3f));
		}
		else if (UNITNAME == "DualSwords"){
			StartCoroutine (AttackingPause(1.2f));
		}
		else if (UNITNAME == "Sorceress"){
			StartCoroutine (AttackingPause(1.3f));
		}
	}

	/*
	* The moment where damage should be dealt to an enemy
	*/
	void Hit(){
		if (isServer){
			// dont need layermask because we prevent friendly fire in game manager 
			/*int layerMask = 1;
			if (team == 0){
				layerMask = 1 << layerTeamTwo;
			}
			else if (team == 1){
				layerMask = 1 << layerTeamOne;
			}*/

			// Raycast in front and hit closest enemy. Different units have different attack effects.
			RaycastHit hitInfo;
			// Knight/KungFuFighter melee attacks. Single target
			if (UNITNAME == "Knight" || UNITNAME == "KungFuFighter" || UNITNAME == "DualSwords"){
				// origin, radius, direction, hitinfo, maxdistance
		        if (Physics.SphereCast(transform.position, 0.7f, transform.forward, out hitInfo, RANGE))
		        {
	            	Debug.Log(hitInfo.collider.gameObject.layer + ", " + gameObject.layer);

	            	if (hitInfo.collider.gameObject.tag == "Unit"){
	            		hitInfo.collider.transform.GetComponent<Unit>().TakeDamage(DAMAGE);
	            	}
	            	else if (hitInfo.collider.gameObject.tag == "Building"){
	            		hitInfo.collider.gameObject.transform.parent.GetComponent<Building>().TakeDamage(DAMAGE);
	            	}
		        }
			}
			// hammer/swordsman melee attacks. Splash damage!
			else if (UNITNAME == "Hammer" || UNITNAME == "Swordsman"){
				// origin, radius, direction, maxdistance
				RaycastHit[] hitUnits = Physics.SphereCastAll(transform.position, 1f, transform.forward, RANGE);
				for (int unit = 0; unit < hitUnits.Length; unit++)
		        {
		            if (hitUnits[unit].collider.gameObject.layer != gameObject.layer)
		            {
		            	if (hitUnits[unit].collider.gameObject.tag == "Unit"){
		            		hitUnits[unit].collider.transform.GetComponent<Unit>().TakeDamage(DAMAGE);
		            	}
		            	else if (hitUnits[unit].collider.gameObject.tag == "Building"){
		            		hitUnits[unit].collider.gameObject.transform.parent.GetComponent<Building>().TakeDamage(DAMAGE);
		            	}
		            }
		        }
			}
		}
	}

	/*
	* Find enemy spawn
	*/
	void FindEnemySpawn(){
		// Use overlap sphere to find units in a certain range
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, 200);
	    int i = 0;
	    while (i < hitColliders.Length)
	    {
			if (hitColliders[i].gameObject.tag == "Building"){
				if (hitColliders[i].gameObject.transform.parent.GetComponent<Building>().MyTeam() != team){
					enemySpawn = hitColliders[i].gameObject;
				}
			}
		    i++;
	    }
	}


	/*
	* Find a new target to focus
	* AIMode - Closest: Target closest enemy 
	* AIMode - Lowest: Target lowest HP enemy within a certain range
	*/
	void FindTarget(){
		// Use overlap sphere to find units in a certain range
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, 200);
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
		// Target closest enemy in range with the lowest HP
		else if (AIMode == "Lowest"){
			targetUnit = GetLowestEnemy(enemiesInRange);
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
     		targetingBuilding = false;
        	return bestTarget.gameObject;
    	}
    	else{
    		targetingBuilding = true;
    		return enemySpawn;
    	}
	}

	/*
	* Find the lowest hp enemy in range
	*/
	GameObject GetLowestEnemy(List<Transform> enemies){
        Transform bestTarget = null;
        float lowestHP = Mathf.Infinity;
        foreach(Transform potentialTarget in enemies)
        {
        	int unitHP = potentialTarget.gameObject.GetComponent<Unit>().GetHP();
            if(unitHP < lowestHP)
            {
                lowestHP = unitHP;
                bestTarget = potentialTarget;
            }
        }
     
        if (bestTarget != null){
        	targetingBuilding = false;
        	return bestTarget.gameObject;
    	}
    	else{
    		targetingBuilding = true;
    		return enemySpawn;
    	}
	}


	/*
	* Handle movement of the unit
	*/
	void MoveUnit(){
		// Aim at the target unit's position minus range
		if (targetUnit != null){
			Vector3 dir = targetUnit.transform.position - transform.position;
			if (RANGED){
				targetDestination = targetUnit.transform.position - Vector3.Normalize(dir) * RANGE;
			}
			else{
				targetDestination = targetUnit.transform.position - Vector3.Normalize(dir) * (targetingBuilding ? RANGE + 1.8f : RANGE);
			}
			// "Sticky" positioning, only set a new target position if the delta position is > 3
			// if the new distance is too short then don't bother moving (prevents jitters)
			if ((deltaDestination - targetDestination).sqrMagnitude > 0.08f){
				Debug.DrawLine(transform.position, targetDestination, Color.white);
				navAgent.SetDestination(targetDestination);

				// if the unit has some time before their next attack then they can "kite" for 0.3s
				if (!attacking){
					animator.SetBool("Moving", true);
				}
				else{
					animator.SetBool("Moving", false);
				}
			}
			// if the unit is currently close to target, and movement adjustment is small then dont animate moving
			else if (Vector3.Distance(transform.position, targetUnit.transform.position) > RANGE){
				animator.SetBool("Moving", true);
			}

			deltaDestination = targetDestination;
		}
	}


	/*
	* Rotate to look at the target location
	*/
	void RotateTowardMovementDirection()  
	{
		if(targetUnit != null)
		{
			if (!RANGED){
				// Turn towards unit if attacking, not moving, or if distance between target destination < range
				if (!animator.GetBool("Moving") || attacking || Vector3.Distance(targetDestination, transform.position) < (targetingBuilding ? RANGE + 2.0f : RANGE)){
					Vector3 pos = targetUnit.transform.position - transform.position;
					if (pos != Vector3.zero)
						targetRot = Quaternion.LookRotation(pos);
				}
				else{
					Vector3 pos = targetDestination - transform.position;
					if (pos != Vector3.zero)
						targetRot = Quaternion.LookRotation(pos);
				}
			}	
			// if ranged
			else{
				// Turn towards unit if attacking, or not moving
				if (!animator.GetBool("Moving") || attacking){
					Vector3 pos = targetUnit.transform.position - transform.position;
					if (pos != Vector3.zero)
						targetRot = Quaternion.LookRotation(pos);
				}
				else{
					Vector3 pos = targetDestination - transform.position;
					if (pos != Vector3.zero)
						targetRot = Quaternion.LookRotation(pos);
				}
			}
		}
		transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, ROTATIONSPEED);
	}

	// Update is called once per frame
	void Update () {
		if (hpDisplay != null){
			hpDisplay.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
		}


		if (isServer){
			if(enemySpawn == null){
				FindEnemySpawn();
			}

			if(targetUnit != null)
			{
				// if target is in range and attack is off cooldown then attack
				if (Vector3.Distance(targetUnit.transform.position, transform.position) <= (targetingBuilding ? RANGE + 2.0f : RANGE) && attackCooldownTimer <= 0f){
					Attack();
				}
			}
			else{
				FindTarget();
				if (animator.GetBool("Moving")){ animator.SetBool("Moving", true); }
			}
		}

		// smooth position and rotation
		if (isServer){
            updateInterval += Time.deltaTime;
            if (updateInterval > 0.067f) // 15 times per second
            {
                updateInterval = 0;
                CmdSync(transform.position, transform.rotation);
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.5f);
        }


		// attack cooldown
		if (attackCooldownTimer > 0f){
			attackCooldownTimer -= Time.deltaTime;
		}
	}

	void FixedUpdate(){
		if (isServer){
			RotateTowardMovementDirection();
		}	
	}

    [Command]
    void CmdSync(Vector3 position, Quaternion rotation)
    {
        realPosition = position;
        realRotation = rotation;
    }


	/*
	* Take amount damage from a source. Only servers can take damage
	*/
	public void TakeDamage(int amount){
		if (!isServer){
			return;
		}

		hp -= amount;
		if (hp <= 0){
			Destroy(this.gameObject);
		}
	}

	// when destroyed, inform the gamemanager
	void OnDestroy(){
		if (mySpawn != null){
			mySpawn.CmdUnitDied();
		}
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
		if (MAXHP < 100){
    		hpDisplay.localScale = new Vector3(newHealth/(float)MAXHP, 0.2f, 0.2f);
    	}
    	else{
    		hpDisplay.localScale = new Vector3(newHealth/(float)MAXHP * 3f, 0.1f, 0.1f);
    	}
	}

	void OnChangeMaxHealth(int newHealth){
		MAXHP = newHealth;
	}

	void OnChangePosition(Vector3 pos){
		realPosition = pos;
	}

	void OnChangeRotation(Quaternion rot){
		realRotation = rot;
	}



	//////////// ARCHER FUNCTIONS ///////////////

	/*
	* Animation event called for archers when they prep their bow
	*/
	[Server]
	public void CreateArrow(){
		if (isServer){
			Vector3 itemSpawnPos = new Vector3(transform.position.x, transform.position.y + 1.9f, transform.position.z);
			Quaternion rot = transform.rotation * Quaternion.Euler(0, 90, 0);
			newArrow = Instantiate(arrowPrefab, itemSpawnPos, rot);
			newArrow.Initialize(team);
			Debug.Log("create arrow on team " + team);
			newArrow.GetComponent<Rigidbody>().useGravity = false;
			NetworkServer.Spawn(newArrow.gameObject);
		}
	}

	/*
	* Animation event called for archers when they release the arrow
	*/
	[Server]
	public void FireArrow(){
		if (isServer){
			if (newArrow != null){
				newArrow.GetComponent<Rigidbody>().useGravity = true;
				if (targetUnit != null){
					newArrow.transform.rotation = Quaternion.LookRotation(targetUnit.transform.position - transform.position);
					newArrow.transform.Rotate(0, 90, 0);
					Vector3 dir = targetUnit.transform.position - transform.position;
					newArrow.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(dir) * 50f, ForceMode.Impulse);
				}
			}
		}
	}


	/////////////// MAGE FUNCTIONS ////////////////////
	/*
	* Animation event called for mages to spawn a meteor
	*/
	[Server]
	void CreateMeteor(){
		Vector3 itemSpawnPos = new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z);
		Vector3 dir = targetUnit.transform.position - itemSpawnPos + Vector3.up * 1.5f;
		Quaternion rot = Quaternion.LookRotation(dir);
		Meteor newMeteor = Instantiate(meteorPrefab, itemSpawnPos, rot);
		newMeteor.Initialize(team);
		NetworkServer.Spawn(newMeteor.gameObject);
		newMeteor.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(dir) * 200f, ForceMode.Impulse);
	}



	/////////////// SORCERESS FUNCTIONS ////////////////////
	/*
	* Animation event called for sorceress to cast ice blast
	*/
	[Server]
	void IceBlast(){
		Vector3 itemSpawnPos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
		// add a little variance to icicles
		float varianceRange = Mathf.Sqrt(Vector3.Distance(targetUnit.transform.position, transform.position));
		Vector3 variance = Vector3.forward * Random.Range(-varianceRange, varianceRange) + Vector3.left * Random.Range(-varianceRange, varianceRange); 
		Vector3 dir = targetUnit.transform.position - itemSpawnPos + Vector3.up * 1.5f + variance;
		Quaternion rot = Quaternion.LookRotation(dir);
		Icicle newIcicle = Instantiate(iciclePrefab, itemSpawnPos, rot);
		newIcicle.Initialize(team);
		NetworkServer.Spawn(newIcicle.gameObject);
		newIcicle.transform.Rotate(90, 0, 0);
		newIcicle.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(dir) * 40f, ForceMode.Impulse);
	}

	//////////// TRIGGERS FOR MOVEMENT ///////////////
	/*
	* When right foot is moved
	*/
	void FootR(){
		//Debug.Log(UNITNAME + " move right foot.");
	}

	/*
	* When right foot is moved
	*/
	void FootL(){
		//Debug.Log(UNITNAME + " move left foot.");
	}
}