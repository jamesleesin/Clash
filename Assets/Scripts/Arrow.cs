﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Arrow : NetworkBehaviour {
	private int team;
	// for layermasks, dont change
	private int layerTeamOne = 8;
	private int layerTeamTwo = 9;

	private const int arrowDamage = 1;
	private bool active = false;
	private float destroyTimer = 10f;

	public Arrow arrowPrefab;
	Quaternion initialRot;

	// Network lerping
	[SyncVar(hook="OnChangePosition")]
    Vector3 realPosition;
    [SyncVar(hook="OnChangeRotation")]
    Quaternion realRotation;
    private float updateIntervalPos;
    private float updateIntervalRot;

	public void Initialize(int t){
		team = t;
		if (team == 0){
			gameObject.layer = 10;
		}
		else if (team == 1){
			gameObject.layer = 11;
		}
		active = true;
		initialRot = transform.rotation;
		realPosition = transform.position;
		realRotation = transform.rotation;
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
            updateIntervalPos += Time.deltaTime;
            updateIntervalRot += Time.deltaTime;
            if (updateIntervalPos > 0.1f) // 10 times per second
            {
                updateIntervalPos = 0;
                CmdSyncPos(transform.position);
            }
            if (updateIntervalRot > 0.03f){
            	updateIntervalRot = 0;
                CmdSyncRot(transform.rotation);
            }
        }
        else
        {	
        	// 0.2 refresh rate and 0.1 percentage looks good for max range attacks
            transform.position = Vector3.Lerp(transform.position, realPosition, 0.15f);
            transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 1f);
        }
	}

	[Command]
    void CmdSyncPos(Vector3 position)
    {
        realPosition = position;
    }

    [Command]
    void CmdSyncRot(Quaternion rotation)
    {
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
	        ContactPoint contact = collision.contacts[0];
	        if (collision.gameObject.tag == "Unit"){
		        if (collision.gameObject != null){
			    	collision.gameObject.transform.GetComponent<Unit>().TakeDamage(arrowDamage);
				}
			}
			else if (collision.gameObject.tag == "Building"){
		        if (collision.gameObject != null){
			    	collision.gameObject.transform.parent.GetComponent<Building>().TakeDamage(arrowDamage);
				}
			}
	        active = false;
	        StartCoroutine(DelayDestroy(0.2f));
	    }
    }

}