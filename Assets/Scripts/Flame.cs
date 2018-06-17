using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// fire has no team, will burn everything in aoe
public class Flame : NetworkBehaviour {
	private float destroyTimer = 10f;
	private const int flameDamage = 3;
	float interval = 0.4f;
	float timer = 0;
	
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

	// damage all units inside it
	void OnTriggerStay(Collider other)
    {
    	if (timer > interval){
	        if (other.gameObject.tag == "Unit")
	        {
	            other.gameObject.transform.GetComponent<Unit>().TakeDamage(flameDamage, 1);
	        }
	        else if (other.gameObject.tag == "Building"){
	            other.gameObject.transform.parent.GetComponent<Building>().TakeDamage(flameDamage, 1);
			}
			timer = 0;
		}
    }
}
