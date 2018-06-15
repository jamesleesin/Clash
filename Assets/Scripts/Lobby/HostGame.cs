using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HostGame : MonoBehaviour {
	[SerializeField]
	private uint roomSize = 2;
	private string roomName;
	[SerializeField]
	private NetworkManager networkManager;

	void Start(){
		networkManager = NetworkManager.singleton;
		if (networkManager.matchMaker == null){
			networkManager.StartMatchMaker();
		}
	}

	public void SetRoomName(){
		roomName = GameObject.Find("RoomName").GetComponent<InputField>().text;
		Debug.Log("Room name set to " + roomName);
	}


	public void CreateRoom(){
		if (roomName != "" && roomName != null){
			Debug.Log("Created room");
			networkManager.matchMaker.CreateMatch(roomName, 2, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
		}
	}
}
