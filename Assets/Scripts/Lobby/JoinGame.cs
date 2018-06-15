using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class JoinGame : NetworkLobbyManager {
	[SerializeField]
	private uint roomSize = 2;
	private string roomName;

	List<GameObject> roomList = new List<GameObject>();
	private NetworkManager networkManager;

	[SerializeField]
	private Text status;

	[SerializeField]
	private GameObject roomListItemPrefab;
	[SerializeField]
	private Transform roomListParent;

	void Start(){
		networkManager = NetworkManager.singleton;
		if (networkManager.matchMaker == null){
			networkManager.StartMatchMaker();
		}
		RefreshRoomList();
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

	public void RefreshRoomList(){
		ClearRoomList();
		networkManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
		status.text = "Loading...";
	}

	public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList){
		base.OnMatchList(success, extendedInfo, matchList);
		status.text = "";
		if (matchList == null){
			status.text = "Could not get matches.";
			return;
		}
		ClearRoomList();
		for (int m = 0; m < matchList.Count; m++){
			GameObject _roomListItemGO = Instantiate(roomListItemPrefab);
			_roomListItemGO.transform.SetParent(roomListParent);

			RoomListItem _roomListItem = _roomListItemGO.GetComponent<RoomListItem>();
			if (_roomListItem != null){
				_roomListItem.Setup(matchList[m], JoinRoom);
			}

			roomList.Add(_roomListItemGO);
		}
		if (roomList.Count == 0){
			status.text = "No matches found.";
		}
	}

	void ClearRoomList(){
		for (int i = 0; i < roomList.Count; i++){
			Destroy(roomList[i]);
		}
		roomList.Clear();
	}

	public void JoinRoom(MatchInfoSnapshot _match){
		Debug.Log("Joining room");
		this.matchMaker.JoinMatch(_match.networkId, "", "", "", 0, 0, OnMatchJoined);
		ClearRoomList();
		status.text = "Joining...";
	}
}
