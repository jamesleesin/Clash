using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ClashNetworkManager : NetworkManager {
	const int maxPlayers = 2;
	Spawn[] playerSlots = new Spawn[2];

	Vector3 spawnLocation1 = new Vector3(-11.3f, 5f, -18.7f);
	Vector3 spawnLocation2 = new Vector3(-6.29f, 5f, 121.73f);

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		// find empty player slot
		for (int slot=0; slot < maxPlayers; slot++)
		{
			if (playerSlots[slot] == null)
			{
				Vector3 pos;
				Quaternion rot;

				if (slot == 0){
					pos = spawnLocation1;
					rot = Quaternion.identity;
				}
				else{
					pos = spawnLocation2;
					rot = Quaternion.Euler(0, 180, 0);
				}
				var playerObj = (GameObject)GameObject.Instantiate(playerPrefab, pos, rot);
				var player = playerObj.GetComponent<Spawn>();

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
