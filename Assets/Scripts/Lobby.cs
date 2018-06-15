using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
 using UnityEngine.Networking.NetworkSystem;


public class MsgTypes
{
   public const short PlayerPrefab = MsgType.Highest + 1;
 
   public class PlayerPrefabMsg : MessageBase
   {
     public short controllerID;    
     public short prefabIndex;
   }
}

public class Lobby : NetworkLobbyManager {
	private int teamNumTaken = -1;
	public short playerPrefabIndex;

	// Use this for initialization
	void Start () {
		MatchmakingStart();
		MatchmakingListMatches();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void MatchmakingStart(){
		this.StartMatchMaker();
	}

	void MatchmakingListMatches(){
		this.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
	}

	public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList){
		base.OnMatchList(success, extendedInfo, matchList);
		if (!success){
			Debug.Log("List failed: " + extendedInfo);
		}
		else{
			if (matchList.Count > 0){
				// found match
				MatchmakingJoinMatch(matchList[0]);
			}
			else{
				MatchmakingCreateMatch();
			}
		}
	}

	void MatchmakingJoinMatch(MatchInfoSnapshot firstMatch){
		this.matchMaker.JoinMatch(firstMatch.networkId, "", "", "", 0, 0, OnMatchJoined);
	}

	public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo){
		base.OnMatchJoined(success, extendedInfo, matchInfo);
		if (!success){
			Debug.Log("Failed to join match");
		}
		else{
			Debug.Log("Joined: " + matchInfo.networkId);
		}
	}

	void MatchmakingCreateMatch(){
		this.matchMaker.CreateMatch("Match 1", 15, true, "", "", "", 0, 0, OnMatchCreate);
	}

	public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo){
		base.OnMatchCreate(success, extendedInfo, matchInfo);
		if (!success){
			Debug.Log("Failed to create match");
		}
		else{
			Debug.Log("Created match: " + matchInfo.networkId);
		}
	}

	public override void OnStartServer()
	{
		NetworkServer.RegisterHandler(MsgTypes.PlayerPrefab, OnResponsePrefab);
		base.OnStartServer();
	}

	//Called on client when connect
    public override void OnClientConnect(NetworkConnection conn) {       
        client.RegisterHandler(MsgTypes.PlayerPrefab, OnRequestPrefab);
		base.OnClientConnect(conn);
		Debug.Log("Client connected.");
    }

    private void OnRequestPrefab(NetworkMessage netMsg)
    {
		MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
		msg.controllerID = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>().controllerID;
		msg.prefabIndex = playerPrefabIndex;
		client.Send(MsgTypes.PlayerPrefab, msg);
		Debug.Log("on request prefab");
    }

    private void OnResponsePrefab(NetworkMessage netMsg)
    {
		MsgTypes.PlayerPrefabMsg msg = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>();  
		playerPrefab = spawnPrefabs[msg.prefabIndex];
		base.OnServerAddPlayer(netMsg.conn, msg.controllerID);
		Debug.Log(playerPrefab.name + " spawned!");
    }

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
		msg.controllerID = playerControllerId;
		NetworkServer.SendToClient(conn.connectionId, MsgTypes.PlayerPrefab, msg);
	}
}
