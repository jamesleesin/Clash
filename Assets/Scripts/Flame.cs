using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// fire has no team, will burn everything in aoe
public class Flame : NetworkBehaviour {
	private float destroyTimer = 5f;
	private const int flameDamage = 4;
	float interval = 0.5f;
	float timer = 0;
	private int team;
	
	// Update is called once per frame
	void Update () {
		if (destroyTimer > 0f){
			destroyTimer -= Time.deltaTime;
		}
		else{
			Destroy(this.gameObject);
		}

		if (timer < interval){ timer += Time.deltaTime; }
	}

	public void Initialize(int t){
		team = t;
		if (team == 0){
			gameObject.layer = 10;
		}
		else if (team == 1){
			gameObject.layer = 11;
		}
	}

	// damage all units inside it
	void OnTriggerStay(Collider other)
    {
    	if (timer > interval){
	        if (other.gameObject.tag == "Unit")
	        {
	            if (other.gameObject.transform.GetComponent<Cannon>() != null){
			    	other.gameObject.transform.GetComponent<Cannon>().TakeDamage(flameDamage, 1);
			    }
			    else{
			    	other.gameObject.transform.GetComponent<Unit>().TakeDamage(flameDamage, 1);
			    }
	        }
	        else if (other.gameObject.tag == "Building"){
	            other.gameObject.transform.parent.GetComponent<Building>().TakeDamage(flameDamage, 1);
			}
			timer = 0;
		}
    }
}
