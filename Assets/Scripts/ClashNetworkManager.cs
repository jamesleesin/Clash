using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

public class ClashNetworkManager : NetworkManager {
	const int maxPlayers = 2;
	Spawn[] playerSlots = new Spawn[2];
	Vector3 spawnLocation1 = new Vector3(-11.3f, 5f, -18.7f);
	Vector3 spawnLocation2 = new Vector3(-6.29f, 5f, 121.73f);
	Vector3 cannonLocation1 = new Vector3(-11.6f, 5f, 0f);
	Vector3 cannonLocation2 = new Vector3(-6.53f, 5f, 100.9f);
	public Cannon cannonPrefab;
	private NetworkManager networkManager;

	[SerializeField]
	private Text status;

	// boolean to prevent multiple clicking of buttons
	private bool creatingRoom = false;

	// start the match maker
	void Start(){
		ConnectionConfig myConfig = new ConnectionConfig();
        myConfig.AddChannel(QosType.Unreliable);
        myConfig.AddChannel(QosType.UnreliableFragmented);
        myConfig.NetworkDropThreshold = 50;         //50%
        myConfig.OverflowDropThreshold = 10;         //10%

		networkManager = NetworkManager.singleton;
		if (networkManager.matchMaker == null){
			networkManager.StartMatchMaker();
		}
	}

	// called when user presses the create room button
	public void CreateRoom(){
		string roomName = GameObject.Find("RoomName").GetComponent<Text>().text;
		if (roomName == "" || roomName == null){
			status.text = "Invalid Room Name";
			return;
		}
		if (roomName != "" && roomName != null && !creatingRoom){
			creatingRoom = true;
			Debug.Log("Created room");
			networkManager.matchMaker.CreateMatch(roomName, 2, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
			status.text = "Creating room...";
		}
	}

	// join room with the name in the input box
	public void JoinRoom(){
		networkManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);

		Debug.Log("Joining room");
		//this.matchMaker.JoinMatch(_match.networkId, "", "", "", 0, 0, OnMatchJoined);
		//ClearRoomList();

		status.text = "Joining...";
	}

	// called from join room, lists all matches
	public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList){
		base.OnMatchList(success, extendedInfo, matchList);
		status.text = "";
		if (matchList == null){
			status.text = "Could not get matches.";
			return;
		}
		string roomName = GameObject.Find("JoinRoomName").GetComponent<Text>().text;
		// look for room with name entered
		for (int m = 0; m < matchList.Count; m++){
			if (matchList[m].name == roomName){
				this.matchMaker.JoinMatch(matchList[m].networkId, "", "", "", 0, 0, OnMatchJoined);
				return;
			}
			else{
				Debug.Log(roomName + ", " + matchList[m].name);
			}
		}
		status.text = "No matches found.";
	}

	// upon adding new player, check to see which player they are and spawn them correctly
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		// find empty player slot
		for (int slot=0; slot < maxPlayers; slot++)
		{
			if (playerSlots[slot] == null)
			{
				Vector3 pos;
				Quaternion rot;
				Vector3 cannonPos;

				if (slot == 0){
					pos = spawnLocation1;
					rot = Quaternion.identity;
					cannonPos = cannonLocation1;
				}
				else{
					pos = spawnLocation2;
					rot = Quaternion.Euler(0, 180, 0);
					cannonPos = cannonLocation2;
				}
				var playerObj = (GameObject)GameObject.Instantiate(playerPrefab, pos, rot);
				var player = playerObj.GetComponent<Spawn>();
				Cannon newCannon = GameObject.Instantiate(cannonPrefab, cannonPos, rot);
				newCannon.Initialize(slot);
				NetworkServer.Spawn(newCannon.gameObject);		
				GameManager.singleton.AddCannon(newCannon);

				player.playerId = slot;
				playerSlots[slot] = player;

				Debug.Log("Adding player in slot " + slot);
				NetworkServer.AddPlayerForConnection(conn, playerObj, playerControllerId);
				return;
			}
		}

		//TODO: graceful  disconnect
		conn.Disconnect();
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController playerController)
	{
		// remove players from slots
		var player = playerController.gameObject.GetComponent<Spawn>();
		playerSlots[player.playerId] = null;
		GameManager.singleton.RemovePlayer(player);

		base.OnServerRemovePlayer(conn, playerController);
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		foreach (var playerController in conn.playerControllers)
		{
			var player = playerController.gameObject.GetComponent<Spawn>();
			playerSlots[player.playerId] = null;
			GameManager.singleton.RemovePlayer(player);
		}
		creatingRoom = false;

		base.OnServerDisconnect(conn);
	}

	public override void OnStartClient(NetworkClient client)
	{
		Debug.Log("client started");
		//client.RegisterHandler(CardConstants.CardMsgId, OnCardMsg);
	}
/*
	void OnCardMsg(NetworkMessage netMsg)
	{
		var msg = netMsg.ReadMessage<CardConstants.CardMessage>();
		
		var other = ClientScene.FindLocalObject(msg.playerId);
		var player = other.GetComponent<Player>();
		player.MsgAddCard(msg.cardId);
	}*/

}
