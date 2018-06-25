using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanning : MonoBehaviour {
	private float panSpeed = 0.5f;
	private float adjustedPanSpeed;
    public Camera panningCamera;
    // pan from z -61.5 to 142
    private bool pauseCamera = false;
	
	// Update is called once per frame
	void Update () {
		if (!pauseCamera){
			// pan speed increase when near middle
			float distanceFromMiddle = Mathf.Abs(40.25f - transform.position.z);
			adjustedPanSpeed = Mathf.Abs(panSpeed - (distanceFromMiddle/240f));
			Vector3 move = new Vector3(-14.1f, 11.72f, transform.position.z + adjustedPanSpeed);
			transform.position = Vector3.Lerp(transform.position, move, 0.5f);
			if (transform.position.z > 142){ 
				StartCoroutine(CameraPause(2f));
			}
		}
	}

	public IEnumerator CameraPause(float pauseTime)
	{
		pauseCamera = true;
		yield return new WaitForSeconds(pauseTime);
		transform.position = new Vector3(-14.1f, 11.72f, -61.5f); 
		pauseCamera = false;
	}
}
