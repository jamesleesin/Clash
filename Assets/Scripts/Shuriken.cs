﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Shuriken : NetworkBehaviour {
	private int team;
	// for layermasks, dont change
	//private int layerTeamOne = 8;
	//private int layerTeamTwo = 9;

	private int shurikenDamage = 7;
	private bool active = false;
	private float destroyTimer = 10f;

	//Quaternion initialRot;

	[SyncVar(hook="OnChangePosition")]
    Vector3 realPosition;
    [SyncVar(hook="OnChangeRotation")]
    Quaternion realRotation;
    private float updateInterval;
	
	public void Initialize(int t, int d){
		team = t;
		shurikenDamage = d;
		if (team == 0){
			gameObject.layer = 10;
		}
		else if (team == 1){
			gameObject.layer = 11;
		}
		//initialRot = transform.rotation;
		realPosition = transform.position;
		realRotation = transform.rotation;
		active = true;
	}

	// Update is called once per frame
	void Update () {
		if (destroyTimer > 0f){
			destroyTimer -= Time.deltaTime;
		}
		else{
			StartCoroutine(DelayDestroy(0f));
		}

		// smooth position and rotation
		if (isServer){
            updateInterval += Time.deltaTime;
            if (updateInterval > 0.15f) // 10 times per second
            {
                updateInterval = 0;
                CmdSync(transform.position, transform.rotation);
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 1f);
        }
	}

	
	[Command]
    void CmdSync(Vector3 position, Quaternion rotation)
    {
        realPosition = position;
        realRotation = rotation;
    }

	void OnChangePosition(Vector3 pos){
		realPosition = pos;
	}

	void OnChangeRotation(Quaternion rot){
		realRotation = rot;
	}

	[Server]
	public IEnumerator DelayDestroy(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (isServer)
			NetworkServer.Destroy(this.gameObject);
	}

	/* 
	* On collision with unit
	*/
	void OnCollisionEnter(Collision collision)
    {
    	if (active){
	        //ContactPoint contact = collision.contacts[0];
	        if (collision.gameObject.tag == "Unit"){
		        if (collision.gameObject.transform.GetComponent<Cannon>() != null){
			    	collision.gameObject.transform.GetComponent<Cannon>().TakeDamage(shurikenDamage, 1);
			    }
			    else{
			    	collision.gameObject.transform.GetComponent<Unit>().TakeDamage(shurikenDamage, 1);
			    }
			}
			else if (collision.gameObject.tag == "Building"){
		        if (collision.gameObject != null){
			    	collision.gameObject.transform.parent.GetComponent<Building>().TakeDamage(shurikenDamage, 1);
				}
			}
			active = false;
		    StartCoroutine(DelayDestroy(0.2f));
		}
    }
}
