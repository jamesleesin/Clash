using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public float dragSpeed = 2;
    private Vector3 dragOrigin;
    public Camera birdseyeCamera;

 	// Drag the camera 
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }
 
        if (!Input.GetMouseButton(0)) return;
 
 		// Cam moves right when dragging right to left
 		// Cam moves up when dragging top to bottom
        Vector3 pos = birdseyeCamera.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(pos.y * dragSpeed, 0, -pos.x * dragSpeed);
 
        transform.Translate(move, Space.World);  
        if (transform.position.z > 145){ transform.position = new Vector3(transform.position.x, transform.position.y, 145); }
		if (transform.position.z < -65){ transform.position = new Vector3(transform.position.x, transform.position.y, -65); }
		if (transform.position.x > 30){ transform.position = new Vector3(30, transform.position.y, transform.position.z); }
		if (transform.position.x < -10){ transform.position = new Vector3(-10, transform.position.y, transform.position.z); }
    }
}
